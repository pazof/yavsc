using System.Net.Http;
using Yavsc.Api.Client;

namespace PostIt.Tests;

/// <summary>
/// Verifies <see cref="FrontOfficeApiClient"/> issues the right
/// <c>api/v1/front/query/accept</c> / <c>.../reject</c> requests with
/// the PostIt signature payload, using a recording fake
/// <see cref="IYavscApiClient"/> (no network).
/// </summary>
public class FrontOfficeApiClientTests
{
    private static QueryAcceptanceRequestDto SampleBody() => new()
    {
        Strokes = new[] { 2, 100, 200, 300, 400 },
        CoordinateMax = 10_000,
        CapturedAtUtc = new DateTime(2026, 9, 21, 12, 0, 0, DateTimeKind.Utc),
    };

    [Fact]
    public async Task Accept_posts_to_accept_endpoint_with_signature_body()
    {
        var api = new RecordingApi();
        var client = new FrontOfficeApiClient(api, "https://business.example/api/v1/");

        var body = SampleBody();
        await client.AcceptQueryAsync("Rdv", 42, body);

        Assert.Equal(HttpMethod.Post, api.LastMethod);
        Assert.Equal(
            "https://business.example/api/v1/front/query/accept?billingCode=Rdv&queryId=42",
            api.LastPath);
        Assert.Same(body, api.LastBody);
    }

    [Fact]
    public async Task Reject_posts_to_reject_endpoint_with_no_body()
    {
        var api = new RecordingApi();
        var client = new FrontOfficeApiClient(api, "https://business.example/api/v1/");

        await client.RejectQueryAsync("Rdv", 42);

        Assert.Equal(HttpMethod.Post, api.LastMethod);
        Assert.Equal(
            "https://business.example/api/v1/front/query/reject?billingCode=Rdv&queryId=42",
            api.LastPath);
        Assert.Null(api.LastBody);
    }

    [Fact]
    public async Task Accept_returns_deserialized_response()
    {
        var api = new RecordingApi
        {
            Response = new QueryAcceptanceResponseDto
            {
                QueryId = 42,
                Status = "Accepted",
                Signature = new QuerySignatureRefDto { Id = 9, EstimateId = 5, Type = "Pro" },
            },
        };
        var client = new FrontOfficeApiClient(api, "https://business.example/api/v1/");

        var response = await client.AcceptQueryAsync("Rdv", 42, SampleBody());

        Assert.Equal(42, response.QueryId);
        Assert.Equal("Accepted", response.Status);
        Assert.NotNull(response.Signature);
        Assert.Equal("Pro", response.Signature!.Type);
    }

    [Fact]
    public async Task Accept_rejects_invalid_arguments()
    {
        var api = new RecordingApi();
        var client = new FrontOfficeApiClient(api, "https://business.example/api/v1/");

        await Assert.ThrowsAsync<ArgumentException>(() => client.AcceptQueryAsync("", 42, SampleBody()));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.AcceptQueryAsync("Rdv", 0, SampleBody()));
        await Assert.ThrowsAsync<ArgumentNullException>(() => client.AcceptQueryAsync("Rdv", 42, null!));
        await Assert.ThrowsAsync<ArgumentException>(() => client.RejectQueryAsync("", 42));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.RejectQueryAsync("Rdv", 0));
    }

    [Fact]
    public async Task GetEstimatePdf_gets_estimate_pdf_endpoint()
    {
        var api = new RecordingApi();
        var client = new FrontOfficeApiClient(api, "https://business.example/api/v1/");

        await client.GetEstimatePdfAsync(42);

        Assert.Equal(HttpMethod.Get, api.LastMethod);
        Assert.Equal(
            "https://business.example/api/v1/front/query/42/estimate.pdf",
            api.LastPath);
    }

    [Fact]
    public async Task GetEstimateTex_gets_estimate_tex_endpoint()
    {
        var api = new RecordingApi();
        var client = new FrontOfficeApiClient(api, "https://business.example/api/v1/");

        await client.GetEstimateTexAsync(42);

        Assert.Equal(HttpMethod.Get, api.LastMethod);
        Assert.Equal(
            "https://business.example/api/v1/front/query/42/estimate.tex",
            api.LastPath);
    }

    private sealed class RecordingApi : IYavscApiClient
    {
        public HttpClient Http { get; } = new();
        public HttpMethod? LastMethod { get; private set; }
        public string? LastPath { get; private set; }
        public object? LastBody { get; private set; }
        public QueryAcceptanceResponseDto? Response { get; set; }

        public Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            LastMethod = method;
            LastPath = path;
            LastBody = body;

            var response = Response ?? new QueryAcceptanceResponseDto();
            return Task.FromResult((T)(object)response);
        }

        public Task CallAsync(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            LastMethod = method;
            LastPath = path;
            LastBody = body;
            return Task.CompletedTask;
        }

        public Task<T> CallAsync<T>(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default)
            => CallAsync<T>(method, path, (object?)null, ct);

        public Task CallAsync(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default)
            => CallAsync(method, path, (object?)null, ct);

        public Task<byte[]> DownloadAsync(HttpMethod method, string path, CancellationToken ct = default)
        {
            LastMethod = method;
            LastPath = path;
            LastBody = null;
            return Task.FromResult(Array.Empty<byte>());
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}