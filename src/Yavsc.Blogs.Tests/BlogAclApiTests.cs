using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Abstract.BlogSpot;
using Yavsc.Models;
using Yavsc.Models.Access;
using Yavsc.Models.Blog;
using Yavsc.Tests.Shared;
using static Yavsc.Constants;

namespace Yavsc.Blogs.Tests;

/// <summary>
/// Behavioural tests for <c>BlogAclApiController.PostCircleAuthorizationToBlogPost</c>:
/// <c>POST /api/v1/blogacl</c> with a JSON body of
/// <c>CircleAuthorizationToBlogPost</c> (CircleId + BlogPostId).
///
/// <para>Same fixture as <see cref="CircleMembersApiTests"/>:
/// <see cref="BlogsWebServerFixture"/> provides a SQLite
/// <c>:memory:</c> <c>ApplicationDbContext</c> (so FKs are
/// enforced the way a real relational engine would) and JWT
/// bearer auth via <c>TestTokenIssuer</c>. No mocks — the real
/// DbContext receives the real INSERT attempt.</para>
///
/// <para>The bug being pinned by these tests: the POST endpoint
/// calls <c>_context.CircleAuthorizationToBlogPost.Add(...)</c>
/// then <c>SaveChangesAsync</c>. The entity has a composite
/// key (CircleId + BlogPostId) and two FKs; EF Core refuses
/// the INSERT with
/// <c>System.InvalidOperationException: The value of
/// 'CircleAuthorizationToBlogPost.BlogPostId' is unknown when
/// attempting to save changes</c> when the principal entities
/// (the existing <c>BlogPost</c> and <c>Circle</c>) are not
/// attached to the DbContext in the same change-tracker graph.</para>
/// </summary>
[Collection("Yavsc Blogs")]
public sealed class BlogAclApiTests : IClassFixture<BlogsWebServerFixture>
{
    private readonly BlogsWebServerFixture _fixture;

    public BlogAclApiTests(BlogsWebServerFixture fixture)
    {
        _fixture = fixture;
    }


    private string BlogUrl()
        => $"{_fixture.Addresses.First(a => a.StartsWith("https://"))}/{APIPrefix}/{BlogSpotPath}";
    private string BlogAclUrl()
        => $"{_fixture.Addresses.First(a => a.StartsWith("https://"))}/{APIPrefix}/{BlogAclPath}";

    /// <summary>Delete any ACL rows tied to the fixture's seeded
    /// <c>(CircleId, BlogPostId)</c> pair. The shared SQLite store
    /// persists across tests, so tests that POST a successful ACL
    /// row would otherwise conflict with whichever other test runs
    /// next against the same pair — xUnit does not guarantee
    /// execution order. Calling this at the start of each
    /// insert-bearing test guarantees a clean slate regardless of
    /// the previous test's outcome.</summary>
    private void CleanupAcl()
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.CircleAuthorizationToBlogPost
            .Where(a => a.CircleId == _fixture.CircleId
                     && a.BlogPostId == _fixture.PostId)
            .ExecuteDelete();
    }

    private HttpClient NewClient(string subject)
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
        var http = new HttpClient(handler)
        {
            BaseAddress = new Uri(_fixture.Addresses.First(a => a.StartsWith("https://")))
        };
        // The Blogs fixture disables JwtSecurityTokenHandler's
        // inbound claim-type remap, so the JWT's "sub" stays "sub"
        // rather than being rewritten to ClaimTypes.NameIdentifier.
        // The controller, however, reads the user id via
        // User.FindFirstValue(ClaimTypes.NameIdentifier), so we add
        // an explicit nameid claim to keep the legacy lookup happy.
        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                TestTokenIssuer.Issue(
                    subject,
                    extraClaims: new[]
                    {
                        new System.Security.Claims.Claim(
                            System.Security.Claims.ClaimTypes.NameIdentifier,
                            subject),
                    }));
        return http;
    }

    /// <summary>
    /// Reproduces the prod 500 logged on 2026-08-21 on mercure:
    /// <c>InvalidOperationException: The value of
    /// 'CircleAuthorizationToBlogPost.BlogPostId' is unknown</c>
    /// when <see cref="PostAclDialogViewModel.AddAsync"/> POSTs the
    /// shape <c>{ "circleId": &lt;id&gt; }</c> — the exact body the
    /// PostIt client builds from <see cref="CircleAuthorization"/>
    /// (which only carries <c>CircleId</c>). The server deserialises
    /// it into <see cref="CircleAuthorizationToBlogPost"/>, leaves
    /// <c>BlogPostId</c> at its <c>default(long) = 0</c>, attaches
    /// no <c>Target</c> navigation, and EF Core refuses to INSERT
    /// during <c>PrepareToSave()</c>. The fix lives in PostIt
    /// (enrich the payload with <c>blogPostId</c> + <c>comment</c>)
    /// and on the wire DTO (<see cref="CircleAuthorization"/> must
    /// carry those fields); the server validates. Until that ships,
    /// this test stays red.
    /// </summary>
    [Fact]
    public async Task PostCircleAuthorization_returns_201_when_payload_mirrors_PostIt_shape_against_existing_circle_named_test()
    {
        // The prod circle already exists with Name="test", Public=true,
        // owned by the caller. We seed the same shape pre-POST so the
        // test reproduces the prod scenario end-to-end.
        CleanupAcl();
        using var http = NewClient(_fixture.DefaultUserLogin);

        var payload = new PostAccessControlRulePayload
        {
            CircleId = _fixture.CircleId,
            BlogPostId = _fixture.PostId
        };

        var response = await http.PostAsJsonAsync(
            BlogAclUrl(), payload,
        TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    /// <summary>
    /// Payload templates for <see cref="PostCircleAuthorization_never_returns_500"/>.
    /// Each row carries the shape we want to POST; <c>-1L</c> and
    /// <c>-2L</c> are negative sentinels that the test substitutes
    /// with the ids of freshly seeded <c>Circle</c> / <c>BlogPost</c>
    /// rows before sending, so every shape lands against a real
    /// principal entity and the seeded fixtures are not dead.
    /// </summary>
    public static IEnumerable<object[]> BlogAclPayloadsForNever500()
    {

        // circleId only (the historical bug shape, 2026-08-21 mercure):
        // must be rejected, never 500.
         return new object[][]
         {
            [
                new PostAccessControlRulePayload
                {
                    BlogPostId = -2,
                    CircleId = -1
                }
            ],
            [new PostAccessControlRulePayload
                {
                    BlogPostId = 1,
                    CircleId = -1
                }
            ],
            [new PostAccessControlRulePayload
                {
                    BlogPostId = 1,
                    CircleId = 1
                }
            ]
         } ;
    }

    /// <summary>
    /// Hard rule (Paul, 2026-08-21): a 500 is never acceptable
    /// </summary>
    [Theory]
    [MemberData(nameof(BlogAclPayloadsForNever500))]
    public async Task PostCircleAuthorization_never_returns_500(PostAccessControlRulePayload payload)
    {
        using var http = NewClient(_fixture.DefaultUserLogin);

        var response = await http.PostAsJsonAsync(
            BlogAclUrl(), payload,
            TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    async Task PostCircleAuthorization_dosent_return_500 ()
    {
        CleanupAcl();
        await PostCircleAuthorization_never_returns_500(

            new PostAccessControlRulePayload
            {
                BlogPostId = -1,
                CircleId = _fixture.CircleId
            }
        );

    }

    [Fact]
    async Task PostCircleAuthorization_dosent_return_500_on_success ()
    {
        CleanupAcl();
        await PostCircleAuthorization_never_returns_500(

            new PostAccessControlRulePayload
            {
                BlogPostId = _fixture.PostId,
                CircleId = _fixture.CircleId
            }
        );

    }

    [Fact]
    public async Task PostBlog_with_ACL_creates_a_post_and_Get_returns_it_in_the_list()
    {
        CleanupAcl();
        _fixture.SeedUser(_fixture.DefaultUserLogin);
        _fixture.SeedUser("tester");
        _fixture.SeedCircle(_fixture.DefaultUserLogin, "test",
        false,
         new String[]
        {
            _fixture.DefaultUserLogin,
            "tester"
        });
        using var http = NewClient(_fixture.DefaultUserLogin );

        // Create a minimal BlogPost. The server assigns Id, so we
        // send 0 + an explicit AuthorId; the production
        // BlogSpotService.Create() tolerates that.
        var draft = new BlogPost
        {
            Id = 0,
            Title = "Premier billet",
            AuthorId = "tester",
            Article = "Contenu de test.",
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow,
            ACL = new List<CircleAuthorizationToBlogPost>(
                new CircleAuthorizationToBlogPost[]
                {
                    new CircleAuthorizationToBlogPost
                    {
                        CircleId = _fixture.CircleId,
                        BlogPostId = _fixture.PostId
                    }
                }
            )
        };

        var postResponse = await http.PostAsJsonAsync(
            BlogUrl(),
            draft,
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        // The POST returns the server-issued post (with a real Id).
        var created = await postResponse.Content.ReadFromJsonAsync<BlogPost>(
            TestContext.Current.CancellationToken
        );
        Assert.NotNull(created);
        Assert.NotEqual(0, created!.Id);
        Assert.Equal(draft.Title, created.Title);

        // The list should now contain exactly one entry.
        var listResponse = await http.GetAsync(
            BlogUrl(),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        using var doc = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken
        ));
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
        Assert.Equal(2, doc.RootElement.GetArrayLength());
        Assert.Equal(created.Id, doc.RootElement[0].GetProperty("id").GetInt64());

        // detail should return the same post, with ACL and tags.
        var detailResponse = await http.GetAsync(
            $"{BlogUrl()}/{created.Id}",
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        using var detailDoc = JsonDocument.Parse(await detailResponse.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken
        ));
        Assert.Equal(JsonValueKind.Object, detailDoc.RootElement.ValueKind);
        Assert.Equal(created.Id, detailDoc.RootElement.GetProperty("id").GetInt64());
        Assert.Equal(1, detailDoc.RootElement.GetProperty("acl").GetArrayLength()); 
    }

}
