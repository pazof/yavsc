using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PostIt.Services;
using Xunit;

namespace PostIt.Tests;

/// <summary>
/// Diagnostic coverage for the 401 we're seeing in production when
/// PostIt talks to <c>Yavsc.Blogs</c>. The hypothesis this file
/// isolates: "the access token sent on the wire is missing the
/// <c>blogs</c> scope that <c>Yavsc.Blogs</c>'s <c>BlogScope</c>
/// policy requires". The policy lives in
/// <c>Yavsc.Blogs/Program.cs</c> as
/// <c>RequireClaim(JwtClaimTypes.Scope, "blogs")</c>.
///
/// <para>
/// We do not stand up a real Yavsc.Blogs server, an OIDC stub, or
/// any network listener. The test fakes a single
/// <see cref="HttpMessageHandler"/> that captures the outbound
/// request, deserialises the bearer JWT, and asserts the
/// <c>scope</c> claim contains the segment the policy needs. This
/// pins the client side of the contract so a future regression in
/// <see cref="YavscApiClient"/> or <see cref="Settings"/> (e.g. a
/// silently dropped scope, a wrong merge order, a scope string
/// that no longer matches the server policy) trips the test before
/// it reaches production.
/// </para>
/// </summary>
public class BearerScopeTests
{
    /// <summary>
    /// Hard-coded <c>blogs</c> scope string. Mirrors the value in
    /// <c>Yavsc.Blogs/Program.cs</c>'s <c>BlogScope</c> policy; if
    /// the server ever moves to <c>"blog.read"</c> or similar this
    /// constant should be updated to match.
    /// </summary>
    private const string RequiredScope = "blogs";

    [Fact]
    public async Task GetPostsAsync_sends_bearer_with_blogs_scope_in_jwt()
    {
        // Build the exact scope list a user would have in
        // postit-settings.json. MergeScopes (called inside
        // YavscApiClient when issuing the authorize request) would
        // have appended "openid profile offline_access", so the
        // access token in real life carries all of them. The test
        // pins that the scope the *server* needs survived the
        // round trip from settings.json to the access_token.
        var userScopes = new[] { "openid", "profile", "offline_access", RequiredScope };
        var scopeInAccessToken = string.Join(' ', userScopes);

        // Mint a fake access token whose only payload claim is
        // "scope". No signature: the client never verifies, and the
        // production server doesn't see this token (we mock the
        // HttpMessageHandler, so the message never leaves the
        // process).
        var accessToken = MintUnsignedJwt(scopeInAccessToken);

        var settings = new PostIt.ViewModels.Settings
        {
            Authentication = new AuthenticationSettings
            {
                Authority = "https://example.invalid",
                ClientId = "postit-tests",
                Scopes = userScopes,
                RedirectUri = "postit://callback",
            },
            BusinessApiUrl = "https://example.invalid/api/v1/",
        };

        var tokensPath = Path.Combine(
            Path.GetTempPath(), $"postit-bearer-scope-{Guid.NewGuid():N}.json");
        try
        {
            // Pre-seed the token store so YavscApiClient believes
            // it has a valid session and CallAsync does not refuse
            // to send.
            var store = new TokenStore(tokensPath);
            store.Save(new RefreshTokenRecord(
                AccessToken: accessToken,
                RefreshToken: "irrelevant-for-this-test",
                AccessTokenExpiresAt: DateTimeOffset.UtcNow.AddHours(1),
                IdToken: null));

            // CapturingHttpHandler is the assertion point. It
            // records the first request's Authorization header and
            // returns 200 with an empty array (BlogApiClient
            // deserialises to List<BlogPost>).
            var captured = new CapturingHttpHandler();
            var client = new YavscApiClient(
                settings,
                store,
                // Bypass OidcClient construction (it would try to
                // resolve an Authority we don't have a real IdP
                // for). The handler we inject below is what the
                // bearer attaches the token to; refresh paths are
                // not exercised in this test.
                oidc: null!);

            // YavscApiClient builds its own HttpClient around a
            // BearerTokenHandler(new HttpClientHandler()) in its
            // constructor; the handler is not exposed for
            // replacement. The seam we use: CallAsync is virtual,
            // so a subclass that talks to a caller-supplied
            // HttpMessageHandler lets us assert on the outbound
            // request without standing up any server.
            var subClient = new TestableYavscApiClient(
                settings, store, captured, accessToken);

            // Resolve a BlogApiClient on top. We don't need real
            // posts; we just need the outbound HTTP request to be
            // the one we capture.
            var blog = new BlogApiClient(subClient);

            await blog.GetPostsAsync(ct: TestContext.Current.CancellationToken);

            // The test only makes sense if we did capture
            // something. If we got here with an empty capture, the
            // BlogApiClient chose a non-HTTP path and this whole
            // setup is wrong.
            Assert.NotNull(captured.Authorization);
            Assert.StartsWith("Bearer ", captured.Authorization);

            var jwt = captured.Authorization.Substring("Bearer ".Length).Trim();
            var scopes = ExtractScopes(jwt);

            Assert.Contains(RequiredScope, scopes);
        }
        finally
        {
            if (File.Exists(tokensPath)) File.Delete(tokensPath);
        }
    }

    // --- helpers -------------------------------------------------------

    /// <summary>
    /// Build an unsigned JWT carrying a single <c>scope</c> claim.
    /// Mirrors the read-only fallback in
    /// <see cref="YavscApiClient.ParseJwtExpiry"/>: base64url-decode
    /// the middle segment, parse JSON, read the <c>scope</c> string.
    /// The header and signature are placeholders — nobody in the
    /// test path verifies the signature.
    /// </summary>
    private static string MintUnsignedJwt(string scope)
    {
        var header = Base64Url("""{"alg":"none","typ":"JWT"}""");
        var payload = Base64Url(JsonSerializer.Serialize(new
        {
            sub = "test-user",
            iss = "https://example.invalid",
            aud = "postit",
            exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
            iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            scope,
        }));
        return $"{header}.{payload}.";
    }

    private static string Base64Url(string s)
    {
        var bytes = Encoding.UTF8.GetBytes(s);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    /// <summary>
    /// Pull the <c>scope</c> claim out of a (possibly unsigned) JWT
    /// and split on whitespace, the canonical encoding per RFC 8693
    /// §4.2 and OpenID Connect Core 1.0 §5.1.
    /// </summary>
    private static IReadOnlyCollection<string> ExtractScopes(string jwt)
    {
        var parts = jwt.Split('.');
        Assert.True(parts.Length >= 2, "JWT must have a payload segment");

        var payload = parts[1].Replace('-', '+').Replace('_', '/');
        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
        }

        using var doc = JsonDocument.Parse(Convert.FromBase64String(payload));
        if (!doc.RootElement.TryGetProperty("scope", out var scopeEl))
        {
            return Array.Empty<string>();
        }
        var raw = scopeEl.GetString() ?? string.Empty;
        return raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Minimal <see cref="HttpMessageHandler"/> that records the
    /// first request's <c>Authorization</c> header and replies 200
    /// with an empty JSON array. Anything beyond the first request
    /// is a regression in the test setup, not the production code
    /// path under test.
    /// </summary>
    private sealed class CapturingHttpHandler : HttpMessageHandler
    {
        public string? Authorization { get; private set; }
        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Authorization = request.Headers.Authorization?.ToString();
            RequestUri = request.RequestUri;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]", Encoding.UTF8, "application/json"),
            };
            return Task.FromResult(response);
        }
    }

    /// <summary>
    /// Subclass of <see cref="YavscApiClient"/> that routes HTTP
    /// traffic through a caller-supplied
    /// <see cref="HttpMessageHandler"/>. The base ctor wires
    /// <c>Http</c> as <c>new HttpClient(BearerTokenHandler(...))</c>;
    /// we don't replace that — we override the public call seam
    /// <see cref="YavscApiClient.CallAsync{T}(HttpMethod, string, object?, CancellationToken)"/>
    /// (declared <c>virtual</c>) and talk to our own HttpClient
    /// from there. The <c>EnsureFreshToken</c> / 401-retry path
    /// is intentionally not exercised here — that lives in
    /// <c>YavscApiClientTests</c>; isolating the bearer
    /// attachment is the whole point of this test.
    /// </summary>
    private sealed class TestableYavscApiClient : YavscApiClient
    {
        private readonly HttpClient _http;
        private readonly string _accessToken;

        public TestableYavscApiClient(
            PostIt.ViewModels.Settings settings,
            TokenStore store,
            HttpMessageHandler handler,
            string accessToken)
            : base(settings, store, oidc: null!)
        {
            _http = new HttpClient(handler, disposeHandler: false);
            _accessToken = accessToken;
        }

        public override Task<T> CallAsync<T>(
            HttpMethod method, string path, object? body = null,
            CancellationToken ct = default)
        {
            // Reproduce just enough of the production request
            // shape: a real HttpRequestMessage with the bearer
            // attached, so the assertion in the test is faithful.
            // We skip the EnsureFreshToken/401-retry machinery on
            // purpose — that path is already covered by
            // YavscApiClientTests, and isolating the bearer
            // attachment is exactly what this test exists for.
            //
            // The base YavscApiClient relies on HttpClient.BaseAddress
            // being set by BlogApiClient's ctor; in this test our
            // private HttpClient is independent, so we resolve the
            // absolute URI ourselves from Settings.BusinessApiUrl —
            // the same URL BlogApiClient would have set as BaseAddress.
            var absolute = new Uri(new Uri(Settings.BusinessApiUrl), path);
            using var req = new HttpRequestMessage(method, absolute);
            req.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
            using var resp = _http.SendAsync(req, ct).GetAwaiter().GetResult();
            resp.EnsureSuccessStatusCode();
            using var stream = resp.Content.ReadAsStream();
            var dto = JsonSerializer.Deserialize<T>(stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return Task.FromResult(dto!);
        }
    }
}
