using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Blogspot;
using Yavsc.Models;
using Yavsc.Models.Access;
using Yavsc.Models.Blog;
using Yavsc.Models.Relationship;
using Yavsc.Tests.Shared;

namespace Yavsc.Blogs.Tests;

/// <summary>
/// Behavioural tests for <c>Visibility</c> on blog posts.
///
/// <para>Same fixture as <see cref="BlogApiTests"/>:
/// in-memory <c>ApplicationDbContext</c>, JWT bearer auth with
/// HS256 via <see cref="TestTokenIssuer"/>. The tests below
/// drive the controller surface (<c>GET /api/v1/blog</c> and
/// <c>GET /api/v1/blog/{id}</c>) and assert that visibility
/// scopes the read path the way
/// <see cref="BlogSpotService"/>'s filter expects.</para>
///
/// <para>Each test seeds its own posts directly through the
/// in-memory DbContext — going through POST would force
/// <c>Visibility</c> through the wire DTO which is fine, but
/// keeping it in the fixture avoids serialisation noise around
/// the visibility default (we want to test each visibility
/// value explicitly, not the JSON round-trip).</para>
/// </summary>
[Collection("JwtClaimMapping")]
public sealed class BlogVisibilityTests : IClassFixture<BlogsWebServerFixture>
{
    private readonly BlogsWebServerFixture _fixture;

    public BlogVisibilityTests(BlogsWebServerFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>Reset the in-memory database and seed the
    /// shared test users.</summary>
    private void ResetDatabase()
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
        });
        db.Users.Add(new ApplicationUser
        {
            Id = "bob",
            UserName = "bob",
            Email = "bob@example.com",
            EmailConfirmed = true,
        });
        db.SaveChanges();
    }

    /// <summary>Insert a blog post authored by <paramref name="authorId"/>
    /// directly via the DbContext and return its id. The ACL,
    /// when supplied, is added to the same context.</summary>
    private long SeedPost(string authorId, Visibility visibility, params long[] aclCircleIds)
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var post = new BlogPost
        {
            AuthorId = authorId,
            Title = $"post-by-{authorId}",
            Article = "test article",
            Visibility = visibility,
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow,
        };
        db.BlogSpot.Add(post);
        db.SaveChanges();

        foreach (var circleId in aclCircleIds)
        {
            db.CircleAuthorizationToBlogPost.Add(new CircleAuthorizationToBlogPost
            {
                BlogPostId = post.Id,
                CircleId = circleId,
                Comment = false,
            });
        }
        db.SaveChanges();

        return post.Id;
    }

    /// <summary>Seed a circle owned by <paramref name="ownerId"/>
    /// and return its id. The ACL grant for a post then points
    /// at this circle; the post stays readable only to circle
    /// members.</summary>
    private long SeedCircle(string ownerId, string name)
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var circle = new Circle { OwnerId = ownerId, Name = name };
        db.Circle.Add(circle);
        db.SaveChanges();
        return circle.Id;
    }

    private string BlogsUrl =>
        _fixture.Addresses.First(a => a.StartsWith("https://")) + "/api/v1/blog";

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
        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer", TestTokenIssuer.Issue(subject));
        return http;
    }

    private static int CountPosts(JsonDocument doc)
        => doc.RootElement.GetArrayLength();

    [Fact]
    public async Task Private_post_is_only_visible_to_its_author()
    {
        ResetDatabase();
        SeedPost("alice", Visibility.Private);

        // Alice (the author) sees it.
        using (var alice = NewClient("alice"))
        {
            var response = await alice.GetAsync(BlogsUrl);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(1, CountPosts(doc));
        }

        // Bob (a different authenticated user) does not.
        using (var bob = NewClient("bob"))
        {
            var response = await bob.GetAsync(BlogsUrl);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(0, CountPosts(doc));
        }
    }

    [Fact]
    public async Task Public_post_with_empty_ACL_is_visible_to_everyone_authenticated()
    {
        ResetDatabase();
        SeedPost("alice", Visibility.Public);

        using var bob = NewClient("bob");
        var response = await bob.GetAsync(BlogsUrl);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await bob.GetAsync(BlogsUrl).Result.Content.ReadAsStringAsync());
        Assert.Equal(1, CountPosts(doc));
    }

    [Fact]
    public async Task Public_post_with_nonempty_ACL_is_restricted_by_the_ACL()
    {
        ResetDatabase();
        var familyCircleId = SeedCircle("alice", "Famille");

        // Alice grants the post to her own "Famille" circle.
        // Bob is not a member, so he must NOT see the post even
        // though Visibility is Public.
        SeedPost("alice", Visibility.Public, familyCircleId);

        using var bob = NewClient("bob");
        var response = await bob.GetAsync(BlogsUrl);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(0, CountPosts(doc));
    }

    [Fact]
    public async Task Private_post_is_not_visible_even_when_ACL_would_have_allowed()
    {
        ResetDatabase();
        var familyCircleId = SeedCircle("alice", "Famille");
        // The ACL would let Bob in, but Visibility.Private
        // overrides it — only the author can read.
        SeedPost("alice", Visibility.Private, familyCircleId);

        using var bob = NewClient("bob");
        var response = await bob.GetAsync(BlogsUrl);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(0, CountPosts(doc));
    }

    [Fact]
    public async Task Post_persists_Visibility_through_the_DTO_wire()
    {
        ResetDatabase();
        using var http = NewClient("alice");

        var draft = new BlogPost
        {
            Id = 0,
            AuthorId = "alice",
            Title = "Un post visible",
            Article = "Contenu.",
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow,
            Visibility = Visibility.Public,
        };

        var postResponse = await http.PostAsJsonAsync(BlogsUrl, draft);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        // The wire DTO should round-trip Visibility (System.Text.Json
        // serialises the enum as its underlying int — see
        // Yavsc.Abstract.Blogspot.Visibility).
        using var doc = JsonDocument.Parse(await postResponse.Content.ReadAsStringAsync());
        Assert.Equal(1, doc.RootElement.GetProperty("visibility").GetInt32());
    }
}
