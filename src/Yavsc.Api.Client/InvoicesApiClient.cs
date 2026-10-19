using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Yavsc.Billing;

namespace Yavsc.Api.Client;

public sealed class InvoicesApiClient
{
    private readonly IYavscApiClient _api;
    private readonly Func<string> _businessBaseAddress;

    public InvoicesApiClient(IYavscApiClient api, Func<string> businessBaseAddress)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
        _businessBaseAddress = businessBaseAddress ?? throw new ArgumentNullException(nameof(businessBaseAddress));
    }

    public Task<InvoiceResponse> GenerateAsync(long estimateId, CancellationToken ct = default)
        => CallAsync(HttpMethod.Post, $"from-estimate/{PositiveId(estimateId)}", null, ct);

    public Task<InvoiceResponse> GetAsync(long commandId, CancellationToken ct = default)
        => CallAsync(HttpMethod.Get, $"{PositiveId(commandId)}", null, ct);

    public Task<InvoiceResponse> RecordOfflineAsync(long commandId, RecordInvoicePaymentRequest request, CancellationToken ct = default)
        => CallAsync(HttpMethod.Post, $"{PositiveId(commandId)}/payments/offline",
            request ?? throw new ArgumentNullException(nameof(request)), ct);

    public Task<InvoiceResponse> VerifyPayPalAsync(long commandId, string transactionId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(transactionId);
        return CallAsync(HttpMethod.Post, $"{PositiveId(commandId)}/payments/paypal",
            new VerifyInvoicePayPalRequest { TransactionId = transactionId }, ct);
    }

    private Task<InvoiceResponse> CallAsync(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        var baseAddress = _businessBaseAddress();
        ArgumentException.ThrowIfNullOrWhiteSpace(baseAddress);
        return _api.CallAsync<InvoiceResponse>(method, new Uri(new Uri(baseAddress, UriKind.Absolute), $"invoices/{path}").ToString(),
            body: body, ct: ct);
    }

    private static long PositiveId(long id)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        return id;
    }
}
