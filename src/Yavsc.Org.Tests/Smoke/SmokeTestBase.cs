using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Yavsc.Org.Tests.Smoke;

/// <summary>
/// Base for the smoke tests covering the production hosts
/// (Yavsc.Org / Yavsc.Api / Yavsc.Blogs). One smoke test per
/// bounded context (BC): each test hits one GET endpoint and
/// asserts a 2xx or 3xx status, with no follow-up redirect.
/// Together they satisfy the 'Tests d'intégration smoke par BC'
/// item of Jalon 0 in <c>ROADMAP.md</c>.
///
/// Status code policy:
/// - 200 OK : endpoint serves a page.
/// - 302 / 301 : endpoint requires auth and redirects to login
///   (acceptable smoke signal: routing + middleware are wired).
/// - 401 / 403 : endpoint exists but rejects anonymous (acceptable
///   for API smoke tests where the smoke is "the host boots").
/// Anything else (404, 500, connection refused) is a failure.
/// </summary>
public abstract class SmokeTestBase
{
    /// <summary>
    /// Issue a GET against <paramref name="relativePath"/> on the
    /// in-memory test server. Returns the raw HttpResponseMessage
    /// without following redirects — the test asserts on the first
    /// hop, not the eventual page.
    /// </summary>
    protected static async Task<HttpResponseMessage> GetRaw(
        HttpClient client, string relativePath)
    {
        Assert.NotNull(client);
        var request = new HttpRequestMessage(HttpMethod.Get, relativePath);
        return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
    }

    /// <summary>
    /// Smoke assertion: a GET on <paramref name="relativePath"/>
    /// returns 2xx (page served) or 3xx (redirect to login) or
    /// 401/403 (anonymous rejected by [Authorize]). Anything else
    /// — 404 (route missing), 5xx (server crash), connection
    /// refused (host not started) — fails the test.
    /// </summary>
    protected static async Task AssertResponds(
        HttpClient client, string relativePath)
    {
        var response = await GetRaw(client, relativePath);
        var status = (int)response.StatusCode;
        Assert.True(
            status >= 200 && status < 400 || status == 401 || status == 403,
            $"GET {relativePath} returned {status} {response.StatusCode}, " +
            "expected 2xx/3xx (page or redirect) or 401/403 (auth required).");
    }
}
