using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Models;
using Yavsc.Models.Access;
using Yavsc.Models.Blog;
using Yavsc.Models.Relationship;
using Yavsc.Tests.Shared;
using static Yavsc.Constants;

namespace Yavsc.Blogs.Tests;

/// <summary>
/// Behavioural tests for <c>BlogAclApiController.PostCircleAuthorizationToBlogPost</c>:
/// <c>POST /api/v1/blogacl</c> with a JSON body of
/// <c>CircleAuthorizationToBlogPost</c> (CircleId + BlogPostId + Comment).
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

    /// <summary>Reset the in-memory database and seed <c>alice</c>.
    /// The shared SQLite <c>:memory:</c> store persists across
    /// requests, so each test starts from a clean slate.</summary>
    private void ResetDatabaseWithAlice()
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        db.Users.Add(new ApplicationUser
        {
            Id = "alice",
            UserName = "alice",
            Email = "alice@example.com",
            EmailConfirmed = true,
            FullName = "Alice Dupont",
            Avatar = "/avatars/alice.png",
        });
        db.SaveChanges();
    }

    /// <summary>Create a circle owned by <paramref name="ownerId"/>
    /// directly in the SQLite store and return its server-assigned
    /// id.</summary>
    private long SeedCircle(string ownerId, string name)
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var circle = new Circle { OwnerId = ownerId, Name = name };
        db.Circle.Add(circle);
        db.SaveChanges();
        return circle.Id;
    }

    /// <summary>Create a blog post owned by <paramref name="authorId"/>
    /// directly in the SQLite store and return its server-assigned
    /// id.</summary>
    private long SeedBlogPost(string authorId, string title)
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var post = new BlogPost
        {
            AuthorId = authorId,
            Title = title,
            Article = "Test article body.",
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow,
        };
        db.BlogSpot.Add(post);
        db.SaveChanges();
        return post.Id;
    }

    private string BlogAclUrl()
        => $"{_fixture.Addresses.First(a => a.StartsWith("https://"))}/{APIPrefix}/blogacl";

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

    /// <summary>PostIt sends only the FK ids (<c>CircleId</c> +
    /// <c>BlogPostId</c>) plus scalar fields, never the navigation
    /// properties <c>Target</c> / <c>Allowed</c>. The controller
    /// must accept that shape and persist the ACL row.</summary>
    [Fact]
    public async Task PostCircleAuthorization_returns_201_when_adding_existing_circle_to_existing_post()
    {
        ResetDatabaseWithAlice();
        var circleId = SeedCircle("alice", "Famille");
        var postId = SeedBlogPost("alice", "Billet de test");
        using var http = NewClient("alice");

        // Mirror PostIt's payload: scalar FK ids only, no nav props.
        var payload = new CircleAuthorizationToBlogPost
        {
            CircleId = circleId,
            BlogPostId = postId,
            Comment = true,
        };

        var response = await http.PostAsJsonAsync(BlogAclUrl(), payload);

        // Expected: 201 Created (per controller line 133: return
        // CreatedAtRoute("GetCircleAuthorizationToBlogPost", ...)).
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
