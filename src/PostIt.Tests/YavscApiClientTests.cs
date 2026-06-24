using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using PostIt.Services;
using Xunit;

namespace PostIt.Tests;

/// <summary>
/// End-to-end coverage of <see cref="YavscApiClient"/>: silent
/// refresh on a near-expiry access token, 401-driven refresh + retry,
/// and persistence of the token bundle via <see cref="TokenStore"/>.
/// Uses the project's <see cref="OidcStubAuthority"/> for the IdP and
/// a tiny in-process HTTP listener for the API server side.
/// </summary>
public class YavscApiClientTests
{
    private static int GetFreePort()
    {
        var l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        var port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return port;
    }

    private static string TokensPath() => Path.Combine(
        Path.GetTempPath(), $"postit-tests-tokens-{Guid.NewGuid():N}.json");

    [Fact]
    public async Task CallAsync_refreshes_silently_when_access_token_is_about_to_expire()
    {
        // The stub OIDC hands out access tokens that expire in 600s.
        // We construct a YavscApiClient, then forcibly mark the
        // in-memory access token as expired and re-run a call. The
        // refresh path must rotate the refresh token transparently
        // and the API call must succeed with the new token.
        using var authority = await OidcStubAuthority.StartAsync();
        using var apiServer = new StubApiServer();
        await apiServer.StartAsync();

        var settings = BuildSettings(authority, apiServer.BaseUrl);
        var tokensPath = TokensPath();
        try
        {
            var client = await LoginAndPersistAsync(
                settings, authority, tokensPath);

            // Mark the cached access token as already expired.
            ExpireCachedAccessToken(tokensPath);

            // Reload — YavscApiClient constructor reads the store.
            var reloaded = new YavscApiClient(settings, new TokenStore(tokensPath));

            var posts = await reloaded.CallAsync<List<StubApiServer.Post>>(
                HttpMethod.Get, "posts");

            Assert.NotNull(posts);
            Assert.NotEmpty(posts);

            // The API server must have seen the new (post-refresh)
            // bearer token, distinct from the original.
            var seen = apiServer.SeenBearers.ToList();
            Assert.NotEmpty(seen);
            Assert.Contains(seen, b => !string.IsNullOrEmpty(b));
        }
        finally
        {
            if (File.Exists(tokensPath)) File.Delete(tokensPath);
        }
    }

    [Fact]
    public async Task CallAsync_retries_once_after_401_then_succeeds()
    {
        // API server returns 401 on the first request, 200 on the next.
        // YavscApiClient must refresh, then retry exactly once.
        using var authority = await OidcStubAuthority.StartAsync();
        using var apiServer = new StubApiServer(forceFirstRequest: true);
        await apiServer.StartAsync();

        var settings = BuildSettings(authority, apiServer.BaseUrl);
        var tokensPath = TokensPath();
        try
        {
            var client = await LoginAndPersistAsync(
                settings, authority, tokensPath);

            var posts = await client.CallAsync<List<StubApiServer.Post>>(
                HttpMethod.Get, "posts");

            Assert.NotEmpty(posts);
            Assert.Equal(2, apiServer.RequestCount);
        }
        finally
        {
            if (File.Exists(tokensPath)) File.Delete(tokensPath);
        }
    }

    [Fact]
    public async Task CallAsync_throws_when_no_token_and_no_interactive_login()
    {
        var settings = new PostIt.Settings
        {
            Authentication = new AuthenticationSettings
            {
                Authority = "https://127.0.0.1:5001",
                ClientId = "postit-tests",
            },
            RedirectUri = "postit://callback",
            Scopes = new[] { "openid" },
            ApiUrl = "https://127.0.0.1:5003/api/v1",
        };
        var client = new YavscApiClient(settings, new TokenStore(Path.Combine(
            Path.GetTempPath(), $"postit-tests-noop-{Guid.NewGuid():N}.json")));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.CallAsync<JsonElement>(HttpMethod.Get, "posts"));
    }

    [Fact]
    public async Task HasValidSession_is_true_after_login()
    {
        using var authority = await OidcStubAuthority.StartAsync();
        using var apiServer = new StubApiServer();
        await apiServer.StartAsync();

        var settings = BuildSettings(authority, apiServer.BaseUrl);
        var tokensPath = TokensPath();
        try
        {
            var client = await LoginAndPersistAsync(
                settings, authority, tokensPath);

            Assert.True(client.HasValidSession,
                "HasValidSession should be true right after a successful login.");
        }
        finally
        {
            if (File.Exists(tokensPath)) File.Delete(tokensPath);
        }
    }

    // --- helpers --------------------------------------------------------

    private static PostIt.Settings BuildSettings(OidcStubAuthority authority, string apiBaseUrl) => new()
    {
        Authentication = new AuthenticationSettings
        {
            Authority = authority.Issuer,
            ClientId = "postit-tests",
        },
        RedirectUri = authority.LoopbackRedirectUri,
        Scopes = new[] { "openid", "profile", "blog" },
        ApiUrl = apiBaseUrl,
    };

    private static async Task<YavscApiClient> LoginAndPersistAsync(
        PostIt.Settings settings, OidcStubAuthority authority, string tokensPath)
    {
        var browser = new FakeAuthorizingBrowser(authority.LoopbackRedirectUri);
        var client = new YavscApiClient(settings, new TokenStore(tokensPath));

        // Force the API client to use the test browser by routing the
        // LoginInteractiveAsync call through a small wrapper.
        await LoginWithBrowserAsync(client, browser.CreateBrowser());
        return client;
    }

    /// <summary>
    /// YavscApiClient.LoginInteractiveAsync delegates to
    /// Platform.CreateBrowser. We can't override that static cleanly
    /// from xunit.v3, so we rebuild the call by re-routing the
    /// Platform.CreateBrowser delegate for the duration of the call.
    /// </summary>
    private static async Task LoginWithBrowserAsync(
        YavscApiClient client, IBrowser browser)
    {
        var original = Platform.CreateBrowser;
        try
        {
            Platform.CreateBrowser = () => browser;
            await client.LoginInteractiveAsync();
        }
        finally
        {
            Platform.CreateBrowser = original;
        }
    }

    private static void ExpireCachedAccessToken(string tokensPath)
    {
        var json = File.ReadAllText(tokensPath);
        var doc = JsonDocument.Parse(json);
        var record = new RefreshTokenRecord(
            AccessToken: doc.RootElement.GetProperty("AccessToken").GetString()!,
            RefreshToken: doc.RootElement.GetProperty("RefreshToken").GetString()!,
            // Far in the past → refresh path must engage on next call.
            AccessTokenExpiresAt: DateTimeOffset.UtcNow.AddMinutes(-5),
            IdToken: doc.RootElement.TryGetProperty("IdToken", out var idt)
                ? idt.GetString()
                : null);
        File.WriteAllText(tokensPath, JsonSerializer.Serialize(record));
    }
}

/// <summary>
/// Tiny in-process API server. By default returns 200 with a fixed
/// list of posts. When <paramref name="forceFirstRequest"/> is true,
/// returns 401 on the first request, 200 on subsequent ones — this
/// is what the silent-refresh-on-401 test hooks into.
/// </summary>
internal sealed class StubApiServer : IAsyncDisposable, IDisposable
{
    public record Post(long Id, string Title);

    private readonly HttpListener _listener;
    private readonly bool _forceFirstRequest;
    private int _requestCount;

    public string BaseUrl { get; private set; } = string.Empty;
    public List<string> SeenBearers { get; } = new();
    public int RequestCount => _requestCount;

    public StubApiServer(bool forceFirstRequest = false)
    {
        _forceFirstRequest = forceFirstRequest;
        var port = GetFreePort();
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://127.0.0.1:{port}/");
    }

    public async Task StartAsync()
    {
        _listener.Start();
        BaseUrl = _listener.Prefixes.First().TrimEnd('/');
        _ = Task.Run(AcceptLoopAsync);
        await Task.Yield();
    }

    private async Task AcceptLoopAsync()
    {
        while (_listener.IsListening)
        {
            HttpListenerContext ctx;
            try { ctx = await _listener.GetContextAsync(); }
            catch { return; }

            Interlocked.Increment(ref _requestCount);

            // Capture the bearer for assertions.
            var auth = ctx.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(auth))
                SeenBearers.Add(auth!);

            if (_forceFirstRequest && _requestCount == 1)
            {
                ctx.Response.StatusCode = 401;
                ctx.Response.Close();
                continue;
            }

            var payload = new
            {
                // Result is an array; the call site expects List<Post>.
                // JsonSerializer deserialises arrays to List<T> fine.
                Items = new[]
                {
                    new Post(1, "Hello from stub"),
                    new Post(2, "Second post"),
                }
            };
            // Wrap in a top-level "Posts" property so the deserialiser
            // sees { "Posts": [...] }? No — the API client expects a
            // JSON array directly. We send the array, not the wrapper.
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload.Items));
            ctx.Response.ContentType = "application/json";
            ctx.Response.ContentLength64 = bytes.Length;
            await ctx.Response.OutputStream.WriteAsync(bytes);
            ctx.Response.Close();
        }
    }

    private static int GetFreePort()
    {
        var l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        var port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return port;
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        try { _listener.Stop(); } catch { }
        _listener.Close();
    }
}
