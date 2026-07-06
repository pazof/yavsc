using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Models;
using Yavsc.Models.Blog;
using Yavsc.Tests.Shared;

namespace Yavsc.Blogs.Tests;

/// <summary>
/// Behavioural tests for <c>BlogApiController</c>. Built on the
/// <see cref="BlogsWebServerFixture"/> scaffold: in-memory
/// <c>ApplicationDbContext</c>, real <c>BlogSpotService</c>,
/// <c>X-Test-Role</c> for the <c>[Authorize("BlogScope")]</c>
/// attribute.
/// </summary>
public sealed class BlogApiTests : IClassFixture<BlogsWebServerFixture>
{
    private readonly BlogsWebServerFixture _fixture;

    public BlogApiTests(BlogsWebServerFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>Reset the in-memory database to a known empty state.
    /// <c>UseInMemoryDatabase</c> shares its store across the
    /// lifetime of the <see cref="BlogsWebServerFixture"/> instance,
    /// so without a per-test reset the test order would leak
    /// state between tests.</summary>
    private void ResetDatabase()
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }

    /// <summary>The fixture's <c>WebApplication</c> is bound to
    /// <c>https://localhost:&lt;random&gt;</c> via
    /// <see cref="WebHostFixture.Addresses"/>. We pick the first
    /// https URL and append the controller route
    /// (<c>/api/v1/blog</c>, matching the production
    /// <c>[Route(APIPrefix + "/blog")]</c>).</summary>
    private string BlogsUrl =>
        _fixture.Addresses.First(a => a.StartsWith("https://")) + "/api/v1/blog";

    private HttpClient NewClient()
    {
        // The fixture's self-signed certificate is not in the user's
        // trust store, so we accept anything (same pattern as
        // Yavsc.Org.Tests' BypassSslValidationHandler).
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
        var http = new HttpClient(handler)
        {
            BaseAddress = new Uri(_fixture.Addresses.First(a => a.StartsWith("https://")))
        };
        http.DefaultRequestHeaders.Add(TestAuthPolicyProvider.HeaderName, TestAuthPolicyProvider.AdminRole);
        return http;
    }

    [Fact]
    public async Task GetBlogs_returns_200_with_empty_list_when_no_posts()
    {
        ResetDatabase();
        using var http = NewClient();

        var response = await http.GetAsync("/api/v1/blog");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        // Empty table → empty JSON array. We compare as a JsonDocument
        // so a future change in formatting (whitespace, indentation)
        // doesn't break the assertion.
        using var doc = JsonDocument.Parse(body);
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
        Assert.Equal(0, doc.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task PostBlog_creates_a_post_and_Get_returns_it_in_the_list()
    {
        ResetDatabase();
        using var http = NewClient();

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
            DateModified = DateTime.UtcNow
        };

        var postResponse = await http.PostAsJsonAsync("/api/v1/blog", draft);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        // The POST returns the server-issued post (with a real Id).
        var created = await postResponse.Content.ReadFromJsonAsync<BlogPost>();
        Assert.NotNull(created);
        Assert.NotEqual(0, created!.Id);
        Assert.Equal(draft.Title, created.Title);

        // The list should now contain exactly one entry.
        var listResponse = await http.GetAsync("/api/v1/blog");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        using var doc = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
        Assert.Equal(1, doc.RootElement.GetArrayLength());
        Assert.Equal(created.Id, doc.RootElement[0].GetProperty("id").GetInt64());
    }
}
