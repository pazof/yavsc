#nullable enable

using System.Globalization;
using Microsoft.Extensions.Options;

namespace Yavsc.Services;

public sealed record VerifiedPayPalReceipt(string TransactionId, decimal Amount, string Currency, DateTimeOffset ReceivedAtUtc);

public interface IPayPalTransactionVerifier
{
    Task<VerifiedPayPalReceipt> VerifyAsync(string transactionId, string invoiceReference, string currency, CancellationToken ct);
}

public sealed class PayPalTransactionVerifier(HttpClient http, IOptions<PayPalSettings> options) : IPayPalTransactionVerifier
{
    public async Task<VerifiedPayPalReceipt> VerifyAsync(string transactionId, string invoiceReference, string currency, CancellationToken ct)
    {
        var settings = options.Value;
        var accounts = settings.Accounts?.Where(account => account.ApiUsername == settings.MerchantAccountUserName).ToArray();
        if (accounts is not { Length: 1 })
            throw new InvoiceOperationException(503, "PayPal transaction verification requires one matching merchant account.");
        var credentials = accounts[0];
        if (string.IsNullOrWhiteSpace(credentials.ApiUsername)
            || string.IsNullOrWhiteSpace(credentials.ApiPassword) || string.IsNullOrWhiteSpace(credentials.Signature)
            || string.IsNullOrWhiteSpace(settings.MerchantAccountId) || settings.Mode is not ("production" or "sandbox"))
            throw new InvoiceOperationException(503, "PayPal transaction verification is not configured.");
        var endpoint = settings.Mode == "production"
            ? "https://api-3t.paypal.com/nvp" : "https://api-3t.sandbox.paypal.com/nvp";
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["METHOD"] = "GetTransactionDetails", ["VERSION"] = "124.0",
            ["USER"] = credentials.ApiUsername, ["PWD"] = credentials.ApiPassword,
            ["SIGNATURE"] = credentials.Signature, ["TRANSACTIONID"] = transactionId
        });

        string body;
        try
        {
            using var response = await http.PostAsync(endpoint, content, ct);
            if (!response.IsSuccessStatusCode)
                throw new InvoiceOperationException(502, "PayPal transaction verification failed.");
            body = await response.Content.ReadAsStringAsync(ct);
        }
        catch (HttpRequestException)
        {
            throw new InvoiceOperationException(502, "PayPal is unavailable. No receipt was recorded.");
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new InvoiceOperationException(504, "PayPal verification timed out. No receipt was recorded.");
        }

        var fields = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in body.Split('&'))
        {
            var parts = pair.Split('=', 2);
            if (parts.Length != 2 || !fields.TryAdd(Decode(parts[0]), Decode(parts[1])))
                throw new InvoiceOperationException(502, "PayPal returned an invalid verification response.");
        }
        string? Field(string name) => fields.GetValueOrDefault(name);
        if (Field("ACK") is not ("Success" or "SuccessWithWarning"))
            throw new InvoiceOperationException(502, "PayPal did not verify the transaction.");
        if (Field("TRANSACTIONID") != transactionId || Field("INVNUM") != invoiceReference
            || Field("RECEIVERID") != settings.MerchantAccountId || Field("CURRENCYCODE") != currency
            || Field("PAYMENTSTATUS") != "Completed")
            throw new InvoiceOperationException(409, "The PayPal receipt is not a completed payment for this invoice, currency and merchant.");
        if (!decimal.TryParse(Field("AMT"), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var amount)
            || amount <= 0m || decimal.Round(amount, 2) != amount
            || !DateTimeOffset.TryParse(Field("ORDERTIME"), CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal, out var receivedAt) || receivedAt == default || receivedAt > DateTimeOffset.UtcNow)
            throw new InvoiceOperationException(502, "PayPal returned an invalid receipt amount or date.");
        return new VerifiedPayPalReceipt(transactionId, amount, currency, receivedAt.ToUniversalTime());
    }

    private static string Decode(string value) => Uri.UnescapeDataString(value.Replace("+", " "));
}
