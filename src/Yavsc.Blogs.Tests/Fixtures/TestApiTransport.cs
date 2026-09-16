using System.Net.Http.Json;
using System.Text.Json;
using Yavsc.Api.Client;

namespace Yavsc.Blogs.Tests.Fixtures;

/// <summary>
/// Minimal <see cref="IYavscApiClient"/> transport for integration
/// tests: it forwards <see cref="BlogApiClient"/> calls over a
/// test-owned <see cref="HttpClient"/> (already authenticated by
/// the caller) and applies the contract documented on the
/// interface — JSON bodies serialised with web defaults
/// (camelCase, case-insensitive reads), and an
/// <see cref="HttpRequestException"/> carrying the response body
/// on non-2xx statuses so assertion failures stay actionable.
///
/// <para>Unlike PostIt's <c>YavscApiClient</c>, there is no
/// silent-refresh-on-401: the test token never expires mid-test,
/// and a 401 is a failure we want surfaced, not retried.</para>
///
/// The <see cref="HttpClient"/> is owned by the test, not by this
/// transport, so <see cref="DisposeAsync"/> is a no-op.
/// </summary>
public sealed class TestApiTransport : IYavscApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public TestApiTransport(HttpClient http)
    {
        Http = http ?? throw new ArgumentNullException(nameof(http));
    }

    public HttpClient Http { get; }

    public Task<T> CallAsync<T>(
        HttpMethod method,
        string path,
        object? body = null,
        CancellationToken ct = default)
        => SendAsync<T>(method, path,
            body is null ? null : () => JsonContent.Create(body, options: JsonOptions),
            ct);

    public Task<T> CallAsync<T>(
        HttpMethod method,
        string path,
        Func<HttpContent> contentFactory,
        CancellationToken ct = default)
        => SendAsync<T>(method, path, contentFactory, ct);

    public async Task CallAsync(
        HttpMethod method,
        string path,
        object? body = null,
        CancellationToken ct = default)
        => await SendAsync<object>(method, path,
            body is null ? null : () => JsonContent.Create(body, options: JsonOptions),
            ct);

    public async Task CallAsync(
        HttpMethod method,
        string path,
        Func<HttpContent> contentFactory,
        CancellationToken ct = default)
        => await SendAsync<object>(method, path, contentFactory, ct);

    private async Task<T> SendAsync<T>(
        HttpMethod method,
        string path,
        Func<HttpContent>? contentFactory,
        CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, path);
        if (contentFactory is not null)
            request.Content = contentFactory();

        using var response = await Http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode} {response.StatusCode} on {method} {path}. Body: {error}");
        }

        var payload = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(payload))
            return default!; // 201/204 with an empty body (PUT returns 204).

        return JsonSerializer.Deserialize<T>(payload, JsonOptions)!;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
