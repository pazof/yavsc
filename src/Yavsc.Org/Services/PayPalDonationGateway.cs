#nullable enable

using System.Globalization;
using System.Text;
using Microsoft.Extensions.Options;
using Yavsc.Models.Payment;

namespace Yavsc.Web.Services;

public sealed class DonationPaymentException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public sealed record DonationApproval(string PayerId);
public sealed record DonationReceipt(string TransactionId, bool Completed);

public interface IPayPalDonationGateway
{
    Task<string> CreateAsync(PerformerDonation donation, string returnUrl, string cancelUrl, string notifyUrl, CancellationToken ct);
    Task<DonationApproval> ApproveAsync(PerformerDonation donation, CancellationToken ct);
    Task<DonationReceipt> CaptureAsync(PerformerDonation donation, string payerId, CancellationToken ct);
    Task<IReadOnlyDictionary<string, string>> VerifyNotificationAsync(string body, CancellationToken ct);
    string CheckoutUrl(string token);
}

public sealed class PayPalDonationGateway(HttpClient http, IOptions<PayPalSettings> options) : IPayPalDonationGateway
{
    private PayPalSettings Settings
    {
        get
        {
            var settings = options.Value;
            if (settings.Mode is not ("production" or "sandbox"))
                throw new DonationPaymentException(503, "Le paiement PayPal n'est pas configure.");
            return settings;
        }
    }

    public string CheckoutUrl(string token)
        => $"https://{(Settings.Mode == "production" ? "www" : "www.sandbox")}.paypal.com/cgi-bin/webscr?cmd=_express-checkout&token={Uri.EscapeDataString(token)}";

    public async Task<string> CreateAsync(PerformerDonation donation, string returnUrl, string cancelUrl,
        string notifyUrl, CancellationToken ct)
    {
        var fields = PaymentFields(donation);
        fields["RETURNURL"] = returnUrl;
        fields["CANCELURL"] = cancelUrl;
        fields["PAYMENTREQUEST_0_NOTIFYURL"] = notifyUrl;
        fields["PAYMENTREQUEST_0_DESC"] = "Don au performer sur Yavsc";
        fields["NOSHIPPING"] = "1";
        var result = await CallAsync("SetExpressCheckout", fields, ct);
        var token = Required(result, "TOKEN");
        return token.Length <= 100 ? token : throw new DonationPaymentException(502, "Jeton PayPal invalide.");
    }

    public async Task<DonationApproval> ApproveAsync(PerformerDonation donation, CancellationToken ct)
    {
        var fields = await CallAsync("GetExpressCheckoutDetails", new()
        {
            ["TOKEN"] = donation.CheckoutToken ?? throw new InvalidOperationException("Missing donation checkout token.")
        }, ct);
        if (Required(fields, "TOKEN") != donation.CheckoutToken
            || Required(fields, "PAYMENTREQUEST_0_INVNUM") != donation.Id.ToString("N")
            || Required(fields, "PAYMENTREQUEST_0_CURRENCYCODE") != donation.Currency
            || !SameAmount(fields.GetValueOrDefault("PAYMENTREQUEST_0_AMT"), donation.Amount)
            || !string.Equals(Required(fields, "PAYMENTREQUEST_0_SELLERPAYPALACCOUNTID"),
                donation.RecipientEmail, StringComparison.OrdinalIgnoreCase))
            throw new DonationPaymentException(409, "La confirmation PayPal ne correspond pas a ce don.");
        return new DonationApproval(Required(fields, "PAYERID"));
    }

    public async Task<DonationReceipt> CaptureAsync(PerformerDonation donation, string payerId, CancellationToken ct)
    {
        var fields = PaymentFields(donation);
        fields["TOKEN"] = donation.CheckoutToken!;
        fields["PAYERID"] = payerId;
        fields["MSGSUBID"] = donation.Id.ToString("N");
        var result = await CallAsync("DoExpressCheckoutPayment", fields, ct);
        if (!SameAmount(result.GetValueOrDefault("PAYMENTINFO_0_AMT"), donation.Amount)
            || Required(result, "PAYMENTINFO_0_CURRENCYCODE") != donation.Currency
            || !string.Equals(Required(result, "PAYMENTINFO_0_SELLERPAYPALACCOUNTID"),
                donation.RecipientEmail, StringComparison.OrdinalIgnoreCase)
            || Required(result, "PAYMENTINFO_0_PAYMENTSTATUS") is not ("Completed" or "Pending"))
            throw new DonationPaymentException(409, "Le paiement PayPal n'est pas confirme pour ce don.");
        var transactionId = Required(result, "PAYMENTINFO_0_TRANSACTIONID");
        if (transactionId.Length > 32)
            throw new DonationPaymentException(502, "Reference de transaction PayPal invalide.");
        return new DonationReceipt(transactionId,
            result["PAYMENTINFO_0_PAYMENTSTATUS"] == "Completed");
    }

    public async Task<IReadOnlyDictionary<string, string>> VerifyNotificationAsync(string body, CancellationToken ct)
    {
        var endpoint = Settings.Mode == "production"
            ? "https://ipnpb.paypal.com/cgi-bin/webscr" : "https://ipnpb.sandbox.paypal.com/cgi-bin/webscr";
        using var content = new StringContent("cmd=_notify-validate&" + body, Encoding.UTF8, "application/x-www-form-urlencoded");
        using var response = await PostAsync(endpoint, content, ct);
        if ((await response.Content.ReadAsStringAsync(ct)).Trim() != "VERIFIED")
            throw new DonationPaymentException(400, "Notification PayPal non verifiee.");
        return Parse(body);
    }

    public static bool SameAmount(string? value, decimal amount)
        => decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var actual)
            && actual == amount;

    private static Dictionary<string, string> PaymentFields(PerformerDonation donation) => new()
    {
        ["PAYMENTREQUEST_0_PAYMENTACTION"] = "Sale",
        ["PAYMENTREQUEST_0_AMT"] = donation.Amount.ToString("F2", CultureInfo.InvariantCulture),
        ["PAYMENTREQUEST_0_CURRENCYCODE"] = donation.Currency,
        ["PAYMENTREQUEST_0_INVNUM"] = donation.Id.ToString("N"),
        ["PAYMENTREQUEST_0_PAYMENTREQUESTID"] = donation.Id.ToString("N"),
        ["PAYMENTREQUEST_0_SELLERPAYPALACCOUNTID"] = donation.RecipientEmail
    };

    private async Task<Dictionary<string, string>> CallAsync(string method, Dictionary<string, string> fields, CancellationToken ct)
    {
        var settings = Settings;
        var accounts = settings.Accounts?.Where(a => a.ApiUsername == settings.MerchantAccountUserName).ToArray();
        if (accounts is not { Length: 1 } || string.IsNullOrWhiteSpace(accounts[0].ApiUsername)
            || string.IsNullOrWhiteSpace(accounts[0].ApiPassword) || string.IsNullOrWhiteSpace(accounts[0].Signature))
            throw new DonationPaymentException(503, "Le compte marchand PayPal n'est pas configure.");
        fields["METHOD"] = method;
        fields["VERSION"] = "124.0";
        fields["USER"] = accounts[0].ApiUsername;
        fields["PWD"] = accounts[0].ApiPassword;
        fields["SIGNATURE"] = accounts[0].Signature;
        using var content = new FormUrlEncodedContent(fields);
        using var response = await PostAsync(settings.Mode == "production"
            ? "https://api-3t.paypal.com/nvp" : "https://api-3t.sandbox.paypal.com/nvp", content, ct);
        var result = Parse(await response.Content.ReadAsStringAsync(ct));
        if (result.GetValueOrDefault("ACK") is not ("Success" or "SuccessWithWarning"))
            throw new DonationPaymentException(502, "PayPal a refuse cette operation. Aucun don n'est marque comme paye.");
        return result;
    }

    private async Task<HttpResponseMessage> PostAsync(string endpoint, HttpContent content, CancellationToken ct)
    {
        try
        {
            var response = await http.PostAsync(endpoint, content, ct);
            if (response.IsSuccessStatusCode) return response;
            response.Dispose();
            throw new DonationPaymentException(502, "Le service PayPal est indisponible.");
        }
        catch (HttpRequestException ex)
        {
            throw new DonationPaymentException(502, $"La connexion PayPal a echoue ({ex.HttpRequestError}). Verifiez l'etat du don avant de recommencer.");
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new DonationPaymentException(504, "La reponse PayPal a expire. Verifiez l'etat du don avant de recommencer.");
        }
    }

    private static Dictionary<string, string> Parse(string body)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in body.Split('&'))
        {
            var parts = pair.Split('=', 2);
            if (parts.Length != 2 || !result.TryAdd(Decode(parts[0]), Decode(parts[1])))
                throw new DonationPaymentException(502, "Reponse PayPal invalide.");
        }
        return result;
    }

    private static string Decode(string value) => Uri.UnescapeDataString(value.Replace("+", " "));
    private static string Required(IReadOnlyDictionary<string, string> fields, string name)
        => fields.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value : throw new DonationPaymentException(502, "Reponse PayPal incomplete.");
}
