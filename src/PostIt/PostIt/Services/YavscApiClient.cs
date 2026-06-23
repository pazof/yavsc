using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.OidcClient;

namespace PostIt.Services;

/// <summary>
/// Thin HTTP client for the Yavsc API with a transparent, silent
/// refresh-on-401 strategy. Callers just call <c>CallAsync</c>; the
/// client takes care of (a) attaching the Bearer access token, (b)
/// refreshing it silently via the OidcClient when it's about to
/// expire or when the server rejects it, and (c) persisting the new
/// token bundle so a relaunch of PostIt picks up where it left off.
///
/// Threading: the refresh path is serialised by a semaphore. The
/// <see cref="BearerTokenHandler"/> only refreshes once even if many
/// concurrent requests are in flight.
/// </summary>
public class YavscApiClient : IAsyncDisposable
{
    // 60s of slack before the access_token's nominal expiry. Covers
    // network latency + JWT validation on the server side.
    private static readonly TimeSpan RefreshSkew = TimeSpan.FromSeconds(60);

    private readonly Settings _settings;
    private readonly OidcClient _oidc;
    private readonly TokenStore _store;
    private readonly HttpClient _http;
    private readonly BearerTokenHandler _bearer;
    private readonly SemaphoreSlim _refreshGate = new(1, 1);

    private RefreshTokenRecord? _tokens;

    public YavscApiClient(Settings settings, TokenStore store, OidcClient? oidc = null)
    {
        _settings = settings;
        _store = store;
        _oidc = oidc ?? new OidcClient(settings.GetOidcClientOptions());

        _bearer = new BearerTokenHandler(this);
        _http = new HttpClient(_bearer, disposeHandler: true)
        {
            // ApiUrl is e.g. "https://blogs.pschneider.fr/api/v1/" — keep the
            // trailing slash so relative paths ("posts") resolve correctly.
            BaseAddress = new Uri(settings.ApiUrl)
        };

        _tokens = store.Load();
    }

    /// <summary>
    /// True if a non-expired access token (or a refreshable bundle) is
    /// already in memory. UI uses this to skip the LoginPage on warm
    /// starts.
    /// </summary>
    public bool HasValidSession
    {
        get
        {
            if (_tokens is null) return false;
            if (_tokens.AccessTokenExpiresAt - DateTimeOffset.UtcNow > TimeSpan.Zero)
                return true;
            // Access token expired but a refresh token is still around.
            return !string.IsNullOrEmpty(_tokens.RefreshToken);
        }
    }

    /// <summary>
    /// The current access token, or null if no session is active.
    /// Surfaced so the LoginPageViewModel can mirror it onto its own
    /// observable property (and so the OIDC id_token / claims can be
    /// shown in the UI).
    /// </summary>
    public string? CurrentAccessToken => _tokens?.AccessToken;

    /// <summary>The current OIDC id_token, or null.</summary>
    public string? CurrentIdToken => _tokens?.IdToken;

    /// <summary>Force a new interactive login (PKCE). Throws on failure.</summary>
    public async Task LoginInteractiveAsync(CancellationToken ct = default)
    {
        var browser = Platform.CreateBrowser?.Invoke();
        if (browser is null)
            throw new InvalidOperationException("No browser is available on this platform.");

        var client = new OidcClient(_settings.GetOidcClientOptions(browser));
        var result = await client.LoginAsync(new LoginRequest(), ct);
        if (result.IsError)
            throw new InvalidOperationException($"OIDC login failed: {result.Error}");

        if (string.IsNullOrEmpty(result.RefreshToken))
            throw new InvalidOperationException(
                "Missing refresh_token — vérifie le scope 'offline_access'.");

        _tokens = new RefreshTokenRecord(
            AccessToken: result.AccessToken,
            RefreshToken: result.RefreshToken,
            AccessTokenExpiresAt: ComputeExpiry(result.AccessTokenExpiration, result.AccessToken),
            IdToken: result.IdentityToken);

        _store.Save(_tokens);
    }

    /// <summary>Call a JSON endpoint, transparently refreshing the token if needed.</summary>
    public virtual async Task<T> CallAsync<T>(
        HttpMethod method,
        string path,
        object? body = null,
        CancellationToken ct = default)
    {
        using var response = await SendAsync(method, path, body, ct).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        var dto = await JsonSerializer.DeserializeAsync<T>(stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct).ConfigureAwait(false);
        return dto!;
    }

    /// <summary>Call an endpoint that returns no useful body (DELETE, etc.).</summary>
    public async Task CallAsync(
        HttpMethod method,
        string path,
        object? body = null,
        CancellationToken ct = default)
    {
        using var response = await SendAsync(method, path, body, ct).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method, string path, object? body, CancellationToken ct)
    {
        if (_tokens is null)
            throw new InvalidOperationException("Not logged in. Call LoginInteractiveAsync first.");

        await EnsureFreshTokenAsync(ct).ConfigureAwait(false);

        using var req = new HttpRequestMessage(method, path);
        if (body is not null)
            req.Content = JsonContent.Create(body);
        var response = await _http.SendAsync(req, ct).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Server rejected: could be revocation, clock skew, audience mismatch.
            // Force a refresh and retry exactly once.
            response.Dispose();
            await ForceRefreshAsync(ct).ConfigureAwait(false);

            using var retry = new HttpRequestMessage(method, path);
            if (body is not null)
                retry.Content = JsonContent.Create(body);
            response = await _http.SendAsync(retry, ct).ConfigureAwait(false);
        }

        return response;
    }

    /// <summary>
    /// Lock the refresh path so concurrent callers don't each rotate
    /// the refresh token (which Auth0 invalidates on first use).
    /// </summary>
    private async Task EnsureFreshTokenAsync(CancellationToken ct)
    {
        if (_tokens is null)
            throw new InvalidOperationException("Not logged in. Call LoginInteractiveAsync first.");

        if (_tokens.AccessTokenExpiresAt - DateTimeOffset.UtcNow > RefreshSkew)
            return;

        await _refreshGate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_tokens.AccessTokenExpiresAt - DateTimeOffset.UtcNow > RefreshSkew)
                return;

            await ForceRefreshAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            _refreshGate.Release();
        }
    }

    private async Task ForceRefreshAsync(CancellationToken ct)
    {
        if (_tokens is null || string.IsNullOrEmpty(_tokens.RefreshToken))
            throw new InvalidOperationException("No refresh token available.");

        var result = await _oidc.RefreshTokenAsync(_tokens.RefreshToken, cancellationToken: ct).ConfigureAwait(false);
        if (result.IsError)
        {
            // Refresh token dead: revoked, expired, or rotation-theft
            // detected. Purge and force an interactive re-login.
            _store.Clear();
            _tokens = null;
            throw new RefreshFailedException(result.Error ?? "refresh failed", isPermanent: true);
        }

        _tokens = new RefreshTokenRecord(
            AccessToken: result.AccessToken!,
            RefreshToken: result.RefreshToken ?? _tokens.RefreshToken,
            AccessTokenExpiresAt: ComputeExpiry(result.AccessTokenExpiration, result.AccessToken),
            IdToken: result.IdentityToken ?? _tokens.IdToken);

        _store.Save(_tokens);
    }

    public async Task LogoutAsync()
    {
        _store.Clear();
        _tokens = null;
        // Optional: clear the IdP session in the browser too.
        // await _oidc.LogoutAsync(new LogoutRequest());
        await Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        _http.Dispose();
        _refreshGate.Dispose();
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Compute the absolute expiry of an access token. Prefer the
    /// IdP-provided expiration when present (TimeSpan or DateTimeOffset
    /// depending on IdentityModel version), fall back to the JWT 'exp'
    /// claim, finally to a conservative 23h default.
    /// </summary>
    private static DateTimeOffset ComputeExpiry(object? provided, string? jwt)
    {
        // IdentityModel.OidcClient 5.x: AccessTokenExpiration is a TimeSpan.
        if (provided is TimeSpan ts) return DateTimeOffset.UtcNow.Add(ts);
        if (provided is DateTimeOffset dto) return dto;
        if (provided is int seconds) return DateTimeOffset.UtcNow.AddSeconds(seconds);

        var fromJwt = ParseJwtExpiry(jwt);
        return fromJwt ?? DateTimeOffset.UtcNow.AddHours(23);
    }

    /// <summary>
    /// Decode the 'exp' claim of a JWT without verifying the signature
    /// (the server verifies). Read-only fallback.
    /// </summary>
    private static DateTimeOffset? ParseJwtExpiry(string? jwt)
    {
        if (string.IsNullOrEmpty(jwt)) return null;
        var parts = jwt.Split('.');
        if (parts.Length != 3) return null;

        var payload = parts[1].Replace('-', '+').Replace('_', '/');
        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
        }

        try
        {
            using var doc = JsonDocument.Parse(Convert.FromBase64String(payload));
            if (!doc.RootElement.TryGetProperty("exp", out var expEl)) return null;
            return DateTimeOffset.FromUnixTimeSeconds(expEl.GetInt64());
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Internal <see cref="HttpMessageHandler"/> that injects the
    /// current Bearer token on every outbound request. Delegating
    /// handlers can't easily retry, so the 401 handling lives in
    /// <see cref="SendAsync"/> above; this handler only attaches the
    /// header.
    /// </summary>
    private sealed class BearerTokenHandler : DelegatingHandler
    {
        private readonly YavscApiClient _owner;
        public BearerTokenHandler(YavscApiClient owner) : base(new HttpClientHandler())
        {
            _owner = owner;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_owner._tokens is not null)
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer", _owner._tokens.AccessToken);
            return base.SendAsync(request, cancellationToken);
        }
    }
}

public sealed class RefreshFailedException : Exception
{
    public bool IsPermanent { get; }
    public RefreshFailedException(string message, bool isPermanent)
        : base(message) => IsPermanent = isPermanent;
}
