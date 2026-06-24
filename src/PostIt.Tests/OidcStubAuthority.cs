using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PostIt.Tests;

/// <summary>
/// Minimal in-process OIDC authority used by LoginPageViewModelTests.
/// It serves the discovery document, jwks, and a token endpoint that
/// accepts any authorization code and returns a signed RS256 JWT.
///
/// Designed to be used together with <see cref="FakeAuthorizingBrowser"/>:
/// the browser intercepts the authorize redirect, the server completes
/// the token exchange.
/// </summary>
public sealed class OidcStubAuthority : IAsyncDisposable, IDisposable
{
    private readonly HttpListener _listener;
    private readonly RSA _rsa;
    private readonly string _kid;
    private readonly CancellationTokenSource _cts = new();

    public string Issuer { get; }
    public string LoopbackRedirectUri { get; }

    private OidcStubAuthority(HttpListener listener, RSA rsa, string kid, string issuer, string loopback)
    {
        _listener = listener;
        _rsa = rsa;
        _kid = kid;
        Issuer = issuer;
        LoopbackRedirectUri = loopback;
    }

    public static async Task<OidcStubAuthority> StartAsync()
    {
        // Pick a free loopback port.
        var port = GetFreePort();
        var prefix = $"http://127.0.0.1:{port}/";
        var loopback = "postit://callback"; // matches PostIt.Settings.DefaultLoopbackRedirectUri

        var listener = new HttpListener();
        listener.Prefixes.Add(prefix);
        listener.Start();

        var rsa = RSA.Create(2048);
        var kid = "test-key-1";

        var authority = new OidcStubAuthority(listener, rsa, kid, prefix.TrimEnd('/'), loopback);
        _ = Task.Run(() => authority.AcceptLoopAsync(authority._cts.Token));
        return authority;
    }

    private async Task AcceptLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            HttpListenerContext ctx;
            try { ctx = await _listener.GetContextAsync().WaitAsync(ct); }
            catch (OperationCanceledException) { return; }
            catch (HttpListenerException) { return; }

            try { await DispatchAsync(ctx); }
            catch { /* swallow per-request */ }
        }
    }

    private async Task DispatchAsync(HttpListenerContext ctx)
    {
        var path = ctx.Request.Url?.AbsolutePath ?? "/";
        switch (path)
        {
            case "/.well-known/openid-configuration":
                await WriteJsonAsync(ctx.Response, BuildDiscovery());
                break;
            case "/.well-known/jwks":
                await WriteJsonAsync(ctx.Response, BuildJwks());
                break;
            case "/connect/token":
                await HandleTokenAsync(ctx);
                break;
            case "/connect/userinfo":
                await WriteJsonAsync(ctx.Response, new { sub = "test-user" });
                break;
            default:
                ctx.Response.StatusCode = 404;
                ctx.Response.Close();
                break;
        }
    }
    private Dictionary<string, object> BuildDiscovery() => new()
    {
        ["issuer"] = Issuer,
        ["authorization_endpoint"] = $"{Issuer}/connect/authorize",
        ["token_endpoint"] = $"{Issuer}/connect/token",
        ["userinfo_endpoint"] = $"{Issuer}/connect/userinfo",
        ["jwks_uri"] = $"{Issuer}/.well-known/jwks",
        ["response_types_supported"] = new[] { "code" },
        ["subject_types_supported"] = new[] { "public" },
        ["id_token_signing_alg_values_supported"] = new[] { "RS256" },
        ["grant_types_supported"] = new[] { "authorization_code" },
        ["code_challenge_methods_supported"] = new[] { "S256" },
    };

    private Dictionary<string, object> BuildJwks()
    {
        var p = _rsa.ExportParameters(false);
        return new Dictionary<string, object>
        {
            ["keys"] = new[]
            {
                new Dictionary<string, object>
                {
                    ["kty"] = "RSA",
                    ["use"] = "sig",
                    ["alg"] = "RS256",
                    ["kid"] = _kid,
                    ["n"] = Base64UrlEncoder.Encode(p.Modulus!),
                    ["e"] = Base64UrlEncoder.Encode(p.Exponent!),
                }
            }
        };
    }

    private async Task HandleTokenAsync(HttpListenerContext ctx)
    {
        // Read form-encoded body.
        string body;
        using (var reader = new StreamReader(ctx.Request.InputStream, Encoding.UTF8))
            body = await reader.ReadToEndAsync();

        var form = ParseForm(body);
        // We accept any code and don't validate PKCE on the stub side;
        // the OidcClient itself validates the redirect_uri match.
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var claims = new Dictionary<string, object>
        {
            ["iss"] = Issuer,
            ["sub"] = "test-user",
            ["aud"] = form.TryGetValue("client_id", out var cid) ? cid : "postit-tests",
            ["exp"] = now + 600,
            ["iat"] = now,
        };

        var accessToken = SignJwt(claims);
        var refreshToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var response = new
        {
            access_token = accessToken,
            id_token = accessToken,
            refresh_token = refreshToken,
            token_type = "Bearer",
            expires_in = 600,
            scope = form.TryGetValue("scope", out var s) ? s : "openid",
        };
        await WriteJsonAsync(ctx.Response, response);
    }

    private string SignJwt(Dictionary<string, object> claims)
    {
        var header = new Dictionary<string, object>
        {
            ["alg"] = "RS256",
            ["typ"] = "JWT",
            ["kid"] = _kid,
        };
        var headerJson = JsonSerializer.Serialize(header);
        var payloadJson = JsonSerializer.Serialize(claims);
        var headerB64 = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(headerJson));
        var payloadB64 = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(payloadJson));
        var signingInput = $"{headerB64}.{payloadB64}";
        var signature = _rsa.SignData(
            Encoding.UTF8.GetBytes(signingInput),
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        return $"{signingInput}.{Base64UrlEncoder.Encode(signature)}";
    }

    private static Dictionary<string, string> ParseForm(string body)
    {
        var dict = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in body.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var eq = pair.IndexOf('=');
            if (eq < 0) continue;
            var key = Uri.UnescapeDataString(pair[..eq]);
            var val = Uri.UnescapeDataString(pair[(eq + 1)..]);
            dict[key] = val;
        }
        return dict;
    }

    private static async Task WriteJsonAsync(HttpListenerResponse response, object payload)
    {
        response.ContentType = "application/json";
        response.StatusCode = 200;
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
        await response.OutputStream.WriteAsync(bytes);
        response.Close();
    }

    private static int GetFreePort()
    {
        var l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        var port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return port;
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        try { _listener.Stop(); } catch { }
        _listener.Close();
        _rsa.Dispose();
        _cts.Dispose();
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        // Synchronous dispose: cancels the accept loop and tears down
        // resources. The accept task will exit on its own once the
        // listener is closed.
        try { _cts.Cancel(); } catch { }
        try { _listener.Stop(); } catch { }
        try { _listener.Close(); } catch { }
        try { _rsa.Dispose(); } catch { }
        try { _cts.Dispose(); } catch { }
    }
}

/// <summary>
/// Minimal base64url encoder (no padding). RFC 7515 §2.
/// </summary>
internal static class Base64UrlEncoder
{
    public static string Encode(byte[] data)
    {
        return Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
