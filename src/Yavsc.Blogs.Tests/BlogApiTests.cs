using System.Net;
using System.Text.Json;

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
}
