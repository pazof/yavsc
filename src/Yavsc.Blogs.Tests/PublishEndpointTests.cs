using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Models;
using Yavsc.Models.Blog;
using Yavsc.Tests.Shared;

namespace Yavsc.Blogs.Tests;

/// <summary>
/// Behavioural tests for the publication toggle endpoint:
/// <c>PUT /api/BlogApi/{id}/publish</c> with body
/// <c>{ "publish": bool }</c>.
///
/// <para>The endpoint is the PostIt-facing way to toggle
/// whether a post is publicly readable (via
/// <c>BlogSpotPublication</c>). It does NOT change the
/// ACL — a Public post with a non-empty ACL is still
/// restricted to the ACL's circles for authenticated
/// callers; only anonymous reads open up.</para>
///
/// <para>Same fixture as <see cref="BlogApiTests"/>:
/// in-memory <c>ApplicationDbContext</c>, JWT bearer auth
/// via <see cref="TestTokenIssuer"/>.</para>
/// </summary>
[Collection("Yavsc Blogs")]
public sealed class PublishEndpointTests : IClassFixture<BlogsWebServerFixture>
{
    private readonly BlogsWebServerFixture _fixture;

    public PublishEndpointTests(BlogsWebServerFixture fixture)
    {
        _fixture = fixture;
    }

    private void ResetDatabase()
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        // ApplicationUser has an AlternateKey on Email; the
        // InMemory provider refuses to track entities whose
        // alternate key is null, so we set it explicitly.
        db.Users.Add(new ApplicationUser
        {
            Id = "alice",
            UserName = "alice",
            Email = "alice@example.com",
            EmailConfirmed = true,
        });
        db.SaveChanges();
    }

    private long SeedPost(string authorId)
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var post = new BlogPost
        {
            AuthorId = authorId,
            Title = $"post-by-{authorId}",
            Article = "test",
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow,
        };
        db.BlogSpot.Add(post);
        db.SaveChanges();
        return post.Id;
    }

    private string PublishUrl(long id)
        => $"{_fixture.Addresses.First(a => a.StartsWith("https://"))}/api/v1/blog/{id}/publish";

    private string BlogsUrl
        => _fixture.Addresses.First(a => a.StartsWith("https://")) + "/api/v1/blog";

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

    [Fact]
    public async Task PutPublish_true_returns_204_and_sets_IsPublished_in_subsequent_GET()
    {
        ResetDatabase();
        var postId = SeedPost("alice");

        using var http = NewClient("alice");
        var put = await http.PutAsJsonAsync(PublishUrl(postId), new { publish = true }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        var get = await http.GetAsync($"{BlogsUrl}/{postId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        using var doc = JsonDocument.Parse(await get.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.True(doc.RootElement.GetProperty("isPublished").GetBoolean());
    }

    [Fact]
    public async Task PutPublish_false_returns_204_and_clears_IsPublished()
    {
        ResetDatabase();
        var postId = SeedPost("alice");

        using var http = NewClient("alice");
        await http.PutAsJsonAsync(PublishUrl(postId), new { publish = true }, TestContext.Current.CancellationToken);
        var put = await http.PutAsJsonAsync(PublishUrl(postId), new { publish = false }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        var get = await http.GetAsync($"{BlogsUrl}/{postId}", TestContext.Current.CancellationToken);
        using var doc = JsonDocument.Parse(await get.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.False(doc.RootElement.GetProperty("isPublished").GetBoolean());
    }

    [Fact]
    public async Task PutPublish_on_unknown_post_returns_404()
    {
        ResetDatabase();
        using var http = NewClient("alice");
        var put = await http.PutAsJsonAsync(PublishUrl(99999L), new { publish = true }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, put.StatusCode);
    }

    [Fact]
    public async Task PutPublish_by_non_author_returns_challenge()
    {
        ResetDatabase();
        var postId = SeedPost("alice");

        using var http = NewClient("bob");
        var put = await http.PutAsJsonAsync(PublishUrl(postId), new { publish = true }, TestContext.Current.CancellationToken);
        // 401 Challenge (the controller returns Challenge()
        // for AuthorizationFailureException). The exact code
        // is framework-dependent; what matters is "not 204".
        Assert.NotEqual(HttpStatusCode.NoContent, put.StatusCode);
    }
}
