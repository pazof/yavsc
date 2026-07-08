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
using PostIt.ViewModels;

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

    public  Settings Settings {  get; }
    private readonly OidcClient _oidc;
    private readonly TokenStore _store;
    public HttpClient Http { get; }
    private readonly BearerTokenHandler _bearer;
    private readonly SemaphoreSlim _refreshGate = new(1, 1);

    private RefreshTokenRecord? _tokens;

    public YavscApiClient(Settings settings, TokenStore store, OidcClient? oidc = null)
    {
        Settings = settings;
        _store = store;
        _oidc = oidc ?? new OidcClient(settings.GetOidcClientOptions());

        _bearer = new BearerTokenHandler(this);
        Http = new HttpClient(_bearer, disposeHandler: true);

        _tokens = store.Load();
    }

    /// <summary>
    /// True if a non-expired access token (or a refreshable bundle) is
    /// already in memory. UI uses this to skip the login flow on warm
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
    /// Surfaced so consumers (e.g. <c>HomePage</c>) can mirror it onto
    /// their own observable properties and so the OIDC id_token / claims
    /// can be shown in the UI.
    /// </summary>
    public string? CurrentAccessToken => _tokens?.AccessToken;

    /// <summary>The current OIDC id_token, or null.</summary>
    public string? CurrentIdToken => _tokens?.IdToken;

    /// <summary>Force a new interactive login (PKCE). Throws on failure.</summary>
    /// <param name="progress">Optional sink for the discrete phases of
    /// the flow; the UI uses this to render a debug-friendly status
    /// (Discovering → OpeningBrowser → AwaitingCallback → ExchangingCode
    /// → Success / Error).</param>
    public async Task LoginInteractiveAsync(
        IProgress<OIDCLoginPhase>? progress = null,
        CancellationToken ct = default)
    {
        progress?.Report(OIDCLoginPhase.Discovering);

        var browser = Platform.CreateBrowser?.Invoke();
        if (browser is null)
        {
            progress?.Report(OIDCLoginPhase.Error);
            throw new InvalidOperationException("No browser is available on this platform.");
        }

        var client = new OidcClient(Settings.GetOidcClientOptions(browser));

        // OidcClient.LoginAsync builds the authorize URL, calls
        // IBrowser.InvokeAsync (which on desktop hands the user off
        // to the system browser and waits on the named pipe), then
        // posts the code at the token endpoint. We can't hook each
        // milestone individually without subclassing OidcClient, so
        // we bracket the call with the two phases the UI cares about:
        // the moment we ask the browser to open (covers the entire
        // user-driven window including the AwaitingCallback wait), and
        // the moment we trade the code for tokens.
        progress?.Report(OIDCLoginPhase.OpeningBrowser);
        var result = await client.LoginAsync(new LoginRequest(), ct).ConfigureAwait(false);

        if (result.IsError)
        {
            progress?.Report(OIDCLoginPhase.Error);
            throw new InvalidOperationException($"OIDC login failed: {result.Error}");
        }

        progress?.Report(OIDCLoginPhase.ExchangingCode);

        if (string.IsNullOrEmpty(result.RefreshToken))
        {
            progress?.Report(OIDCLoginPhase.Error);
            throw new InvalidOperationException(
                "Missing refresh_token — vérifie le scope 'offline_access'.");
        }

        _tokens = new RefreshTokenRecord(
            AccessToken: result.AccessToken,
            RefreshToken: result.RefreshToken,
            AccessTokenExpiresAt: ComputeExpiry(result.AccessTokenExpiration, result.AccessToken),
            IdToken: result.IdentityToken);

        _store.Save(_tokens);
        progress?.Report(OIDCLoginPhase.Success);
    }

    /// <summary>
    /// Best-effort silent refresh used at boot when a token bundle is
    /// already on disk. Returns <c>true</c> when the access token is
    /// usable (either because it was still valid, or because the
    /// refresh succeeded); <c>false</c> when the refresh token is
    /// gone / rejected and the caller should route to the login page.
    /// Never throws on refresh failure — it logs via the returned
    /// phase and returns false so the UI can keep going.
    /// </summary>
    public async Task<bool> TrySilentLoginAsync(
        IProgress<OIDCLoginPhase>? progress = null,
        CancellationToken ct = default)
    {
        if (!HasValidSession) return false;
        if (_tokens is null) return false;

        // Access token still has plenty of life — nothing to do.
        if (_tokens.AccessTokenExpiresAt - DateTimeOffset.UtcNow > RefreshSkew)
        {
            progress?.Report(OIDCLoginPhase.Success);
            return true;
        }

        // Access token expired but we have a refresh token: try the
        // silent refresh once. If the OP rejects (revoked, rotation
        // theft, network down), the refresh path already purges the
        // store and throws RefreshFailedException; we catch and route
        // the user back to the login page.
        try
        {
            progress?.Report(OIDCLoginPhase.ExchangingCode);
            await ForceRefreshAsync(ct).ConfigureAwait(false);
            progress?.Report(OIDCLoginPhase.Success);
            return true;
        }
        catch (RefreshFailedException)
        {
            progress?.Report(OIDCLoginPhase.Idle);
            return false;
        }
        catch
        {
            progress?.Report(OIDCLoginPhase.Idle);
            return false;
        }
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

    /// <summary>
    /// Call a JSON endpoint with no request body while still allowing a
    /// positional cancellation token argument.
    /// </summary>
    public Task<T> CallAsync<T>(
        HttpMethod method,
        string path,
        CancellationToken ct)
        => CallAsync<T>(method, path, body: null, ct);

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

    /// <summary>
    /// Call an endpoint with no request body while still allowing a
    /// positional cancellation token argument.
    /// </summary>
    public Task CallAsync(
        HttpMethod method,
        string path,
        CancellationToken ct)
        => CallAsync(method, path, body: null, ct);

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method, string path, object? body, CancellationToken ct)
    {
        if (_tokens is null)
            throw new InvalidOperationException("Not logged in. Call LoginInteractiveAsync first.");

        await EnsureFreshTokenAsync(ct).ConfigureAwait(false);

        using var req = new HttpRequestMessage(method, path);
        if (body is not null)
            req.Content = JsonContent.Create(body);
        var response = await Http.SendAsync(req, ct).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Server rejected: could be revocation, clock skew, audience mismatch.
            // Force a refresh and retry exactly once.
            response.Dispose();
            await ForceRefreshAsync(ct).ConfigureAwait(false);

            using var retry = new HttpRequestMessage(method, path);
            if (body is not null)
                retry.Content = JsonContent.Create(body);
            response = await Http.SendAsync(retry, ct).ConfigureAwait(false);
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
        Http.Dispose();
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
