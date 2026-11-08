#nullable enable
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Yavsc.Models.Payment;
using Yavsc.Settings;

namespace Yavsc.Web.Services;

public sealed class SubscriptionOperationException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public sealed record PayPalSubscriptionDetails(string Id, string CustomId, string PlanId, string State,
    DateTimeOffset? StartsAt, DateTimeOffset? NextBillingAt, string? ApprovalUrl);
public sealed record PayPalSubscriptionTransaction(string Id, string State, decimal Amount, string Currency, DateTimeOffset PaidAt);

public interface IPayPalSubscriptionGateway
{
    Task<PayPalSubscriptionDetails> CreateAsync(ResourceSubscription subscription, string returnUrl, string cancelUrl, CancellationToken ct);
    Task<PayPalSubscriptionDetails> GetAsync(string id, CancellationToken ct);
    Task<IReadOnlyList<PayPalSubscriptionTransaction>> TransactionsAsync(string id, DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
    Task CancelAsync(string id, CancellationToken ct);
    Task VerifyWebhookAsync(JsonElement notification, IReadOnlyDictionary<string, string> headers, CancellationToken ct);
}

public sealed class PayPalSubscriptionGateway(HttpClient http, IOptions<PayPalSettings> paypalOptions,
    IOptions<ResourceSubscriptionOptions> subscriptionOptions) : IPayPalSubscriptionGateway
{
    private static readonly Regex SubscriptionId = new("^I-[A-Z0-9]{1,62}$", RegexOptions.CultureInvariant);
    private string Origin => paypalOptions.Value.Mode switch
    {
        "production" => "https://api-m.paypal.com",
        "sandbox" => "https://api-m.sandbox.paypal.com",
        _ => throw new SubscriptionOperationException(503, "Le paiement PayPal REST n'est pas configure.")
    };

    public async Task<PayPalSubscriptionDetails> CreateAsync(ResourceSubscription subscription,
        string returnUrl, string cancelUrl, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(subscriptionOptions.Value.WebhookId))
            throw new SubscriptionOperationException(503, "Le webhook PayPal des abonnements n'est pas configure.");
        await ValidatePlanAsync(subscription, ct);
        using var result = await SendAsync(HttpMethod.Post, "/v1/billing/subscriptions", new
        {
            plan_id = subscription.PlanId,
            custom_id = subscription.Id.ToString("N"),
            application_context = new
            {
                brand_name = "Yavsc", user_action = "SUBSCRIBE_NOW", shipping_preference = "NO_SHIPPING",
                return_url = returnUrl, cancel_url = cancelUrl
            }
        }, subscription.Id.ToString("N"), ct);
        return ReadSubscription(result.RootElement);
    }

    private async Task ValidatePlanAsync(ResourceSubscription subscription, CancellationToken ct)
    {
        if (!Regex.IsMatch(subscription.PlanId, "^P-[A-Z0-9]{1,62}$", RegexOptions.CultureInvariant))
            throw new SubscriptionOperationException(503, "Le plan PayPal du pack n'est pas configure.");
        using var result = await SendAsync(HttpMethod.Get, "/v1/billing/plans/" + subscription.PlanId, null, null, ct);
        var plan = Object(result.RootElement);
        var cycles = Array(Property(plan, "billing_cycles")).ToArray();
        if (Text(plan, "id") != subscription.PlanId || Text(plan, "status") != "ACTIVE" || cycles.Length != 1)
            throw new SubscriptionOperationException(503, "Le plan PayPal doit etre actif, mensuel et sans periode d'essai.");
        var cycle = cycles[0];
        var frequency = Property(cycle, "frequency");
        var price = Property(Property(cycle, "pricing_scheme"), "fixed_price");
        var preferences = Object(Property(plan, "payment_preferences"));
        if (Text(cycle, "tenure_type") != "REGULAR" || Integer(Property(cycle, "total_cycles")) != 0
            || Text(frequency, "interval_unit") != "MONTH" || Integer(Property(frequency, "interval_count")) != 1
            || Text(price, "currency_code") != "EUR" || Amount(Property(price, "value")) != subscription.PriceIncludingTax
            || Boolean(Property(preferences, "auto_bill_outstanding"))
            || (preferences.TryGetProperty("setup_fee", out var fee) && Amount(Property(fee, "value")) != 0m)
            || (plan.TryGetProperty("taxes", out var taxes) && Amount(Property(taxes, "percentage")) != 0m)
            || (plan.TryGetProperty("quantity_supported", out var quantity) && Boolean(quantity)))
            throw new SubscriptionOperationException(503, "Le plan PayPal doit correspondre au tarif TTC du pack, sans frais supplementaires ni regroupement des impayes.");
    }

    public async Task<PayPalSubscriptionDetails> GetAsync(string id, CancellationToken ct)
    {
        CheckId(id);
        using var result = await SendAsync(HttpMethod.Get, "/v1/billing/subscriptions/" + id, null, null, ct);
        return ReadSubscription(result.RootElement);
    }

    public async Task<IReadOnlyList<PayPalSubscriptionTransaction>> TransactionsAsync(
        string id, DateTimeOffset from, DateTimeOffset to, CancellationToken ct)
    {
        CheckId(id);
        var all = new List<PayPalSubscriptionTransaction>();
        // PayPal accepts bounded date ranges. Adjacent ranges can repeat an
        // endpoint transaction; reconciliation deduplicates by PayPal ID.
        while (from < to)
        {
            var end = from.AddDays(30) < to ? from.AddDays(30) : to;
            string Date(DateTimeOffset value) => Uri.EscapeDataString(value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture));
            using var result = await SendAsync(HttpMethod.Get,
                $"/v1/billing/subscriptions/{id}/transactions?start_time={Date(from)}&end_time={Date(end)}", null, null, ct);
            foreach (var item in Array(Property(result.RootElement, "transactions")))
            {
                var amount = Property(Property(item, "amount_with_breakdown"), "gross_amount");
                all.Add(new(Text(item, "id"), Text(item, "status"), Amount(Property(amount, "value")),
                    Text(amount, "currency_code"), ReadDate(Text(item, "time"))));
            }
            from = end;
        }
        return all;
    }

    public async Task CancelAsync(string id, CancellationToken ct)
    {
        CheckId(id);
        using var result = await SendAsync(HttpMethod.Post, $"/v1/billing/subscriptions/{id}/cancel",
            new { reason = "Resiliation demandee par l'abonne Yavsc" }, null, ct);
    }

    public async Task VerifyWebhookAsync(JsonElement notification, IReadOnlyDictionary<string, string> headers, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(subscriptionOptions.Value.WebhookId))
            throw new SubscriptionOperationException(503, "Le webhook PayPal des abonnements n'est pas configure.");
        string Header(string name) => headers.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value : throw new SubscriptionOperationException(400, "Signature PayPal manquante.");
        using var result = await SendAsync(HttpMethod.Post, "/v1/notifications/verify-webhook-signature", new
        {
            auth_algo = Header("PAYPAL-AUTH-ALGO"), cert_url = Header("PAYPAL-CERT-URL"),
            transmission_id = Header("PAYPAL-TRANSMISSION-ID"), transmission_sig = Header("PAYPAL-TRANSMISSION-SIG"),
            transmission_time = Header("PAYPAL-TRANSMISSION-TIME"),
            webhook_id = subscriptionOptions.Value.WebhookId, webhook_event = notification
        }, null, ct);
        if (Text(result.RootElement, "verification_status") != "SUCCESS")
            throw new SubscriptionOperationException(400, "Notification PayPal non verifiee.");
    }

    private async Task<JsonDocument> SendAsync(HttpMethod method, string path, object? payload, string? requestId, CancellationToken ct)
    {
        var settings = paypalOptions.Value;
        if (string.IsNullOrWhiteSpace(settings.ClientId) || string.IsNullOrWhiteSpace(settings.ClientSecret))
            throw new SubscriptionOperationException(503, "Les identifiants REST PayPal ne sont pas configures.");
        try
        {
            using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, Origin + "/v1/oauth2/token");
            tokenRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes(settings.ClientId + ":" + settings.ClientSecret)));
            tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string> { ["grant_type"] = "client_credentials" });
            using var tokenResponse = await http.SendAsync(tokenRequest, ct);
            if (!tokenResponse.IsSuccessStatusCode)
                throw new SubscriptionOperationException(502, "L'authentification PayPal a echoue.");
            using var token = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync(ct));
            using var request = new HttpRequestMessage(method, Origin + path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Text(token.RootElement, "access_token"));
            if (requestId is not null) request.Headers.Add("PayPal-Request-Id", requestId);
            if (payload is not null) request.Content = JsonContent.Create(payload);
            using var response = await http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
                throw new SubscriptionOperationException(502, "PayPal a refuse l'operation. Verifiez l'etat de l'abonnement avant de recommencer.");
            return response.StatusCode == System.Net.HttpStatusCode.NoContent
                ? JsonDocument.Parse("{}") : JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        }
        catch (HttpRequestException)
        {
            throw new SubscriptionOperationException(502, "PayPal est indisponible. Aucun paiement n'est presume reussi.");
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new SubscriptionOperationException(504, "PayPal n'a pas repondu a temps. Verifiez l'abonnement avant de recommencer.");
        }
        catch (JsonException)
        {
            throw new SubscriptionOperationException(502, "PayPal a retourne une reponse invalide.");
        }
    }

    private static PayPalSubscriptionDetails ReadSubscription(JsonElement value)
    {
        value = Object(value);
        var id = Text(value, "id");
        CheckId(id);
        if (value.TryGetProperty("plan_overridden", out var overridden) && Boolean(overridden))
            throw new SubscriptionOperationException(409, "Le plan de cet abonnement a ete modifie.");
        if ((value.TryGetProperty("quantity", out var quantity) && Amount(quantity) != 1m)
            || (value.TryGetProperty("shipping_amount", out var shipping) && Amount(Property(shipping, "value")) != 0m))
            throw new SubscriptionOperationException(409, "L'abonnement comporte une quantite ou des frais non autorises.");
        string? approval = null;
        if (value.TryGetProperty("links", out var links))
        {
            var approvals = Array(links).Where(link => Text(link, "rel") == "approve")
                .Select(link => Text(link, "href")).ToArray();
            if (approvals.Length > 1)
                throw new SubscriptionOperationException(502, "Plusieurs liens d'approbation PayPal ont ete retournes.");
            approval = approvals.SingleOrDefault();
        }
        if (approval is not null && (!Uri.TryCreate(approval, UriKind.Absolute, out var uri)
            || uri.Scheme != "https" || !string.IsNullOrEmpty(uri.UserInfo)
            || uri.Host is not ("www.paypal.com" or "www.sandbox.paypal.com")))
            throw new SubscriptionOperationException(502, "Lien d'approbation PayPal invalide.");
        DateTimeOffset? Date(JsonElement element, string key) => element.TryGetProperty(key, out _)
            ? ReadDate(Text(element, key)) : null;
        var next = value.TryGetProperty("billing_info", out var info) ? Date(Object(info), "next_billing_time") : null;
        return new(id, Text(value, "custom_id"), Text(value, "plan_id"), Text(value, "status"),
            Date(value, "start_time"), next, approval);
    }

    private static void CheckId(string id)
    {
        if (!SubscriptionId.IsMatch(id)) throw new SubscriptionOperationException(400, "Reference d'abonnement PayPal invalide.");
    }

    private static JsonElement Property(JsonElement value, string property)
        => value.ValueKind == JsonValueKind.Object && value.TryGetProperty(property, out var found)
            ? found : throw new SubscriptionOperationException(502, "Reponse PayPal incomplete.");

    private static JsonElement Object(JsonElement value)
        => value.ValueKind == JsonValueKind.Object
            ? value : throw new SubscriptionOperationException(502, "Objet PayPal invalide.");

    private static JsonElement.ArrayEnumerator Array(JsonElement value)
        => value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray() : throw new SubscriptionOperationException(502, "Liste PayPal invalide.");

    private static int Integer(JsonElement value)
        => value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number)
            ? number : throw new SubscriptionOperationException(502, "Entier PayPal invalide.");

    private static bool Boolean(JsonElement value)
        => value.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? value.GetBoolean() : throw new SubscriptionOperationException(502, "Booleen PayPal invalide.");

    private static string Text(JsonElement value, string property)
    {
        var found = Property(value, property);
        return found.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(found.GetString())
            ? found.GetString()! : throw new SubscriptionOperationException(502, "Reponse PayPal invalide.");
    }

    private static decimal Amount(JsonElement value)
        => value.ValueKind == JsonValueKind.String
            && decimal.TryParse(value.GetString(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var amount)
            ? amount : throw new SubscriptionOperationException(502, "Montant PayPal invalide.");

    private static DateTimeOffset ReadDate(string value)
        => DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date)
            ? date.ToUniversalTime() : throw new SubscriptionOperationException(502, "Date PayPal invalide.");
}
