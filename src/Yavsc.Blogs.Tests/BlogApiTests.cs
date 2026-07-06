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
/// <c>ApplicationDbContext</c>, real <c>BlogSpotService</c>, and a
/// real <c>AddJwtBearer</c> validating HS256 tokens signed by
/// <see cref="TestTokenIssuer"/>. The production <c>BlogScope</c>
/// policy runs unmodified — sending <c>Authorization: Bearer …</c>
/// with a valid token is what gets a request through, omitting the
/// header (or sending a token signed with the wrong key) gets a
/// 401 back from the framework.
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

    /// <summary>Build an authenticated client: a real
    /// <c>Authorization: Bearer &lt;jwt&gt;</c> header where the JWT
    /// is signed by <see cref="TestTokenIssuer"/> and carries
    /// <c>sub = subject</c>. The production <c>BlogScope</c> policy
    /// reads <c>scope=blogs</c> off the same token, so
    /// <c>TestTokenIssuer.Issue</c>'s default scope is enough.</summary>
    private HttpClient NewClient(string subject = "tester")
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
        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer", TestTokenIssuer.Issue(subject));
        return http;
    }

    /// <summary>Build an unauthenticated client. Used to assert that
    /// the <c>BlogScope</c> policy fails closed when no bearer
    /// token is presented.</summary>
    private HttpClient NewAnonymousClient()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
        return new HttpClient(handler)
        {
            BaseAddress = new Uri(_fixture.Addresses.First(a => a.StartsWith("https://")))
        };
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

    [Fact]
    public async Task GetBlog_returns_401_when_no_token_is_provided()
    {
        ResetDatabase();
        using var http = NewAnonymousClient();

        // No Authorization header → the JwtBearer middleware
        // produces an unauthenticated principal, the BlogScope
        // policy's RequireAuthenticatedUser requirement fails, and
        // the framework returns 401. This is the proof that the
        // production policy is wired in the test host and not
        // short-circuited by a test-only auth bypass.
        var response = await http.GetAsync("/api/v1/blog");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PutBlog_with_valid_token_and_owner_returns_204_and_Get_reflects_update()
    {
        ResetDatabase();
        // The JWT's sub must match the post's AuthorId:
        // PermissionHandler.IsOwner checks blog.AuthorId == user.GetUserId(),
        // and UserHelpers.GetUserId reads "sub" off the principal.
        // A mismatched sub → AuthorizationFailureException →
        // Challenge() (401) from the controller. The 204 in this
        // test is the proof that the real authorization chain
        // accepted the request, end-to-end.
        using var http = NewClient(subject: "tester");

        // Seed a post we can update.
        var draft = new BlogPost
        {
            Id = 0,
            Title = "Avant",
            AuthorId = "tester",
            Article = "Contenu initial.",
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow
        };
        var postResponse = await http.PostAsJsonAsync("/api/v1/blog", draft);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var created = (await postResponse.Content.ReadFromJsonAsync<BlogPost>())!;

        // PUT with the server-issued Id; the controller rejects
        // mismatched id/blog.Id with 400, so we keep them aligned.
        var update = new BlogPost
        {
            Id = created.Id,
            Title = "Après",
            AuthorId = created.AuthorId,
            Article = created.Article,
            DateCreated = created.DateCreated,
            DateModified = DateTime.UtcNow
        };
        var putResponse = await http.PutAsJsonAsync($"/api/v1/blog/{created.Id}", update);
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

        // The list should now reflect the new title.
        var listResponse = await http.GetAsync("/api/v1/blog");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        using var doc = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
        Assert.Equal(1, doc.RootElement.GetArrayLength());
        Assert.Equal("Après", doc.RootElement[0].GetProperty("title").GetString());
    }

    [Fact]
    public async Task DeleteBlog_removes_a_post_and_Get_returns_an_empty_list()
    {
        ResetDatabase();
        using var http = NewClient();

        // Seed a post we can delete.
        var draft = new BlogPost
        {
            Id = 0,
            Title = "À supprimer",
            AuthorId = "tester",
            Article = "Contenu.",
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow
        };
        var postResponse = await http.PostAsJsonAsync("/api/v1/blog", draft);
        var created = (await postResponse.Content.ReadFromJsonAsync<BlogPost>())!;

        var deleteResponse = await http.DeleteAsync($"/api/v1/blog/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        // The list should now be empty.
        var listResponse = await http.GetAsync("/api/v1/blog");
        using var doc = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        Assert.Equal(0, doc.RootElement.GetArrayLength());
    }
}
