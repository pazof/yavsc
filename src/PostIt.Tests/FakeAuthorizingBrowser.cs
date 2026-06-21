using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.OidcClient.Browser;

namespace PostIt.Tests;

/// <summary>
/// A minimal <see cref="IBrowser"/> for tests. Captures the authorize
/// URL emitted by OidcClient, extracts its <c>state</c>, and returns a
/// BrowserResult that mimics the OIDC redirect-with-code callback.
///
/// The paired <see cref="OidcStubAuthority"/>'s token endpoint accepts
/// any authorization code, so we don't need to mint a real one here.
/// </summary>
public sealed class FakeAuthorizingBrowser
{
    private readonly string _redirectUri;
    private readonly HttpClient _http = new();

    public FakeAuthorizingBrowser(string redirectUri)
    {
        _redirectUri = redirectUri;
    }

    public IdentityModel.OidcClient.Browser.IBrowser CreateBrowser() => new Impl(_redirectUri, _http);

    private sealed class Impl : IdentityModel.OidcClient.Browser.IBrowser
    {
        private readonly string _redirectUri;
        private readonly HttpClient _http;

        public Impl(string redirectUri, HttpClient http)
        {
            _redirectUri = redirectUri;
            _http = http;
        }

        public async Task<BrowserResult> InvokeAsync(BrowserOptions options, System.Threading.CancellationToken cancellationToken = default)
        {
            // Touch the authorize URL so any 4xx/5xx surfaces; we don't
            // actually need its response body because we synthesize the
            // redirect below from the original URL's query string.
            var startUri = new Uri(options.StartUrl);
            try
            {
                using var resp = await _http.GetAsync(startUri, cancellationToken);
                // Ignore the status: the stub has no real /connect/authorize.
            }
            catch
            {
                // Network errors are expected against the stub; continue.
            }

            // Pull `state` from the authorize URL so the OidcClient can
            // verify it against its own nonces.
            var state = ParseQuery(startUri.Query).GetValueOrDefault("state");
            if (string.IsNullOrEmpty(state))
            {
                return new BrowserResult
                {
                    ResultType = BrowserResultType.UserCancel,
                    ErrorDescription = "no state in authorize URL"
                };
            }

            // Synthesize the redirect that the OIDC server would have
            // sent back. The scheme and path match whatever the test
            // configured (loopback for the historical test harness,
            // postit://callback for the custom-scheme path).
            var baseUri = _redirectUri;
            if (!baseUri.EndsWith("/")) baseUri += "/";
            var redirectUri =
                $"{baseUri}?code=test-auth-code&state={Uri.EscapeDataString(state)}";

            return new BrowserResult
            {
                ResultType = BrowserResultType.Success,
                Response = redirectUri
            };
        }

        private static System.Collections.Generic.Dictionary<string, string> ParseQuery(string query)
        {
            var dict = new System.Collections.Generic.Dictionary<string, string>(StringComparer.Ordinal);
            if (string.IsNullOrEmpty(query)) return dict;
            if (query.StartsWith("?")) query = query[1..];
            foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var eq = pair.IndexOf('=');
                if (eq < 0) { dict[pair] = ""; continue; }
                dict[pair[..eq]] = Uri.UnescapeDataString(pair[(eq + 1)..]);
            }
            return dict;
        }
    }
}
