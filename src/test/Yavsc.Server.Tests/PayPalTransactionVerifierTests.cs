using System.Globalization;
using System.Net;
using Microsoft.Extensions.Options;
using Yavsc.Services;

namespace Yavsc.Server.Tests;

public sealed class PayPalTransactionVerifierTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private static Dictionary<string, string> Completed() => new()
    {
        ["ACK"] = "Success", ["TRANSACTIONID"] = "TX-1", ["INVNUM"] = "RdvQuery/42",
        ["RECEIVERID"] = "merchant-id", ["CURRENCYCODE"] = "EUR", ["PAYMENTSTATUS"] = "Completed",
        ["AMT"] = "30.25", ["ORDERTIME"] = "2026-01-02T12:00:00Z"
    };

    private static PayPalSettings Settings(string mode = "sandbox") => new()
    {
        Mode = mode, MerchantAccountUserName = "test-api-user", MerchantAccountId = "merchant-id",
        Accounts =
        [
            new PayPalSettings.ClassicPayPalAccountApiCredential
            {
                ApiUsername = "test-api-user", ApiPassword = "test-password", Signature = "test-signature"
            }
        ]
    };

    private static string Encode(Dictionary<string, string> fields)
        => string.Join("&", fields.Select(field => $"{Uri.EscapeDataString(field.Key)}={Uri.EscapeDataString(field.Value)}"));

    [Theory]
    [InlineData("sandbox", "api-3t.sandbox.paypal.com")]
    [InlineData("production", "api-3t.paypal.com")]
    public async Task Completed_receipt_is_verified_with_server_credentials_and_invariant_amount(string mode, string host)
    {
        using var handler = new Handler(Encode(Completed()));
        using var http = new HttpClient(handler);
        var verifier = new PayPalTransactionVerifier(http, Options.Create(Settings(mode)));
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            var receipt = await verifier.VerifyAsync("TX-1", "RdvQuery/42", "EUR", Ct);
            Assert.Equal(30.25m, receipt.Amount);
            Assert.Equal("EUR", receipt.Currency);
            Assert.Equal("TX-1", receipt.TransactionId);
            Assert.Equal(TimeSpan.Zero, receipt.ReceivedAtUtc.Offset);
            Assert.Equal(host, handler.Uri!.Host);
            Assert.Equal("https", handler.Uri.Scheme);
            Assert.Contains("METHOD=GetTransactionDetails", handler.Body);
            Assert.Contains("TRANSACTIONID=TX-1", handler.Body);
            Assert.Contains("USER=test-api-user", handler.Body);
            Assert.Contains("PWD=test-password", handler.Body);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Theory]
    [InlineData("PAYMENTSTATUS", "Pending", 409)]
    [InlineData("PAYMENTSTATUS", "Refunded", 409)]
    [InlineData("PAYMENTSTATUS", "Reversed", 409)]
    [InlineData("TRANSACTIONID", "TX-2", 409)]
    [InlineData("INVNUM", "RdvQuery/43", 409)]
    [InlineData("RECEIVERID", "another-merchant", 409)]
    [InlineData("CURRENCYCODE", "USD", 409)]
    [InlineData("ACK", "Failure", 502)]
    [InlineData("AMT", "-1", 502)]
    [InlineData("AMT", "0", 502)]
    [InlineData("AMT", "30,25", 502)]
    [InlineData("AMT", "0.001", 502)]
    [InlineData("ORDERTIME", "not-a-date", 502)]
    [InlineData("ORDERTIME", "2999-01-01T12:00:00Z", 502)]
    public async Task Unverified_or_mismatched_transactions_are_rejected(string field, string value, int status)
    {
        var fields = Completed();
        fields[field] = value;
        using var handler = new Handler(Encode(fields));
        using var http = new HttpClient(handler);
        var verifier = new PayPalTransactionVerifier(http, Options.Create(Settings()));

        var exception = await Assert.ThrowsAsync<InvoiceOperationException>(
            () => verifier.VerifyAsync("TX-1", "RdvQuery/42", "EUR", Ct));
        Assert.Equal(status, exception.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ACK=Success&ACK=Success")]
    [InlineData("ACK=Success&malformed")]
    public async Task Malformed_responses_do_not_confirm_receipts(string body)
    {
        using var handler = new Handler(body);
        using var http = new HttpClient(handler);
        var verifier = new PayPalTransactionVerifier(http, Options.Create(Settings()));
        var exception = await Assert.ThrowsAsync<InvoiceOperationException>(
            () => verifier.VerifyAsync("TX-1", "RdvQuery/42", "EUR", Ct));
        Assert.Equal(502, exception.StatusCode);
    }

    [Fact]
    public async Task Missing_configuration_fails_before_any_network_call()
    {
        using var handler = new Handler(Encode(Completed()));
        using var http = new HttpClient(handler);
        var verifier = new PayPalTransactionVerifier(http, Options.Create(new PayPalSettings()));
        var exception = await Assert.ThrowsAsync<InvoiceOperationException>(
            () => verifier.VerifyAsync("TX-1", "RdvQuery/42", "EUR", Ct));
        Assert.Equal(503, exception.StatusCode);
        Assert.Null(handler.Uri);
    }

    [Fact]
    public async Task Http_failure_does_not_confirm_a_receipt()
    {
        using var handler = new Handler("unavailable", HttpStatusCode.ServiceUnavailable);
        using var http = new HttpClient(handler);
        var verifier = new PayPalTransactionVerifier(http, Options.Create(Settings()));
        var exception = await Assert.ThrowsAsync<InvoiceOperationException>(
            () => verifier.VerifyAsync("TX-1", "RdvQuery/42", "EUR", Ct));
        Assert.Equal(502, exception.StatusCode);
    }

    [Fact]
    public async Task Caller_cancellation_is_not_converted_into_a_payment_failure()
    {
        using var handler = new Handler(Encode(Completed()));
        using var http = new HttpClient(handler);
        var verifier = new PayPalTransactionVerifier(http, Options.Create(Settings()));
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(Ct);
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => verifier.VerifyAsync("TX-1", "RdvQuery/42", "EUR", cancellation.Token));
    }

    private sealed class Handler(string body, HttpStatusCode status = HttpStatusCode.OK) : HttpMessageHandler
    {
        public Uri? Uri { get; private set; }
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            Uri = request.RequestUri;
            Body = await request.Content!.ReadAsStringAsync(ct);
            return new HttpResponseMessage(status) { Content = new StringContent(body) };
        }
    }
}
