using System.Net.Http;
using Yavsc.Api.Client;
using Yavsc.Billing;

namespace PostIt.Tests;

public sealed class InvoicesApiClientTests
{
    [Fact]
    public async Task Client_maps_invoice_operations_and_forwards_cancellation()
    {
        var api = new RecordingApi();
        var client = new InvoicesApiClient(api, () => "https://business.example/api/v1/");
        var ct = TestContext.Current.CancellationToken;

        await client.GenerateAsync(7, ct);
        Assert.Equal(HttpMethod.Post, api.Method);
        Assert.Equal("https://business.example/api/v1/invoices/from-estimate/7", api.Path);
        Assert.Null(api.Body);
        Assert.Equal(ct, api.Cancellation);

        await client.GetAsync(42, ct);
        Assert.Equal(HttpMethod.Get, api.Method);
        Assert.Equal("https://business.example/api/v1/invoices/42", api.Path);

        var receipt = new RecordInvoicePaymentRequest
        {
            Reference = "bank-1", Amount = 30m, Currency = "EUR",
            ReceivedAtUtc = DateTimeOffset.UtcNow.AddDays(-1)
        };
        await client.RecordOfflineAsync(42, receipt, ct);
        Assert.Equal(HttpMethod.Post, api.Method);
        Assert.Equal("https://business.example/api/v1/invoices/42/payments/offline", api.Path);
        Assert.Same(receipt, api.Body);

        await client.VerifyPayPalAsync(42, "TX-1", ct);
        Assert.Equal(HttpMethod.Post, api.Method);
        Assert.Equal("https://business.example/api/v1/invoices/42/payments/paypal", api.Path);
        Assert.Equal("TX-1", Assert.IsType<VerifyInvoicePayPalRequest>(api.Body).TransactionId);
        Assert.Equal(ct, api.Cancellation);
    }

    [Fact]
    public async Task Invalid_arguments_fail_before_transport()
    {
        var api = new RecordingApi();
        var client = new InvoicesApiClient(api, () => "https://business.example/api/v1/");
        var ct = TestContext.Current.CancellationToken;
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GenerateAsync(0, ct));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => client.GetAsync(-1, ct));
        await Assert.ThrowsAsync<ArgumentNullException>(() => client.RecordOfflineAsync(42, null!, ct));
        await Assert.ThrowsAsync<ArgumentException>(() => client.VerifyPayPalAsync(42, "", ct));
        Assert.Null(api.Path);
    }

    private sealed class RecordingApi : IYavscApiClient
    {
        public HttpClient Http { get; } = new();
        public HttpMethod? Method { get; private set; }
        public string? Path { get; private set; }
        public object? Body { get; private set; }
        public CancellationToken Cancellation { get; private set; }

        public Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            Method = method;
            Path = path;
            Body = body;
            Cancellation = ct;
            return Task.FromResult(default(T)!);
        }

        public Task CallAsync(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
            => throw new NotSupportedException();

        public Task<T> CallAsync<T>(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default)
            => throw new NotSupportedException();

        public Task CallAsync(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default)
            => throw new NotSupportedException();

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
