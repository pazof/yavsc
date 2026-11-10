#nullable enable
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Yavsc.Models.Payment;
using Yavsc.Server.Helpers;
using Yavsc.Settings;
using Yavsc.ViewModels;
using Yavsc.Web.Services;

namespace Yavsc.Controllers;

[Authorize(Policy = "WebUser")]
[ResourceUsageExempt]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class ResourceSubscriptionController(IResourceSubscriptionService subscriptions,
    IPayPalSubscriptionGateway paypal, IResourceUsageService usage,
    IOptions<ResourceSubscriptionOptions> options, IOptions<ResourceUsageOptions> usageOptions,
    IOptions<SiteSettings> site, ILogger<ResourceSubscriptionController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
        => View(new ResourceSubscriptionPage(options.Value, usageOptions.Value.DefaultLimit,
            await usage.GetCurrentAsync(User.GetUserId(), User.Identity?.Name ?? User.GetUserId(), ct),
            await subscriptions.GetAllAsync(User.GetUserId(), ct)));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string pack, CancellationToken ct)
    {
        var subscription = await subscriptions.PrepareAsync(User.GetUserId(), pack, ct);
        var root = subscription.PayPalId is null ? SiteRoot() : "";
        var approvalUrl = await subscriptions.CheckoutAsync(subscription,
            root + "ResourceSubscription/Return?id=" + subscription.Id,
            root + "ResourceSubscription", ct);
        if (approvalUrl is not null)
            return Redirect(approvalUrl);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Return(Guid id, CancellationToken ct)
    {
        var subscription = await OwnedAsync(id, ct);
        if (subscription is null) return NotFound();
        await subscriptions.ReconcileAsync(subscription, ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        foreach (var subscription in await subscriptions.GetAllAsync(User.GetUserId(), ct))
            await subscriptions.ReconcileAsync(subscription, ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var subscription = await OwnedAsync(id, ct);
        if (subscription is null || subscription.PayPalId is null) return NotFound();
        await subscriptions.CancelAsync(subscription, ct);
        return RedirectToAction(nameof(Index));
    }

    [AllowAnonymous, HttpPost, IgnoreAntiforgeryToken, RawPayPalNotification, RequestSizeLimit(64 * 1024)]
    public async Task<IActionResult> Notify(CancellationToken ct)
    {
        JsonDocument notification;
        try
        {
            notification = await JsonDocument.ParseAsync(Request.Body, cancellationToken: ct);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Malformed PayPal subscription notification");
            return BadRequest();
        }
        using (notification)
        {
            var headers = Request.Headers.ToDictionary(pair => pair.Key, pair => pair.Value.ToString(), StringComparer.OrdinalIgnoreCase);
            await paypal.VerifyWebhookAsync(notification.RootElement, headers, ct);
            var value = notification.RootElement;
            if (value.ValueKind != JsonValueKind.Object
                || !value.TryGetProperty("event_type", out var type) || type.ValueKind != JsonValueKind.String
                || !value.TryGetProperty("resource", out var resource) || resource.ValueKind != JsonValueKind.Object)
                throw new SubscriptionOperationException(400, "Notification PayPal incomplete.");
            var eventType = type.GetString()!;
            if (!eventType.StartsWith("BILLING.SUBSCRIPTION.", StringComparison.Ordinal)
                && eventType is not ("PAYMENT.SALE.COMPLETED" or "PAYMENT.SALE.REFUNDED" or "PAYMENT.SALE.REVERSED"))
                return NoContent();
            ResourceSubscription? subscription;
            var key = eventType.StartsWith("BILLING.", StringComparison.Ordinal) ? "id" : "billing_agreement_id";
            var paypalId = NotificationText(resource, key);
            if (paypalId is not null)
                subscription = await subscriptions.FindPayPalAsync(paypalId, ct);
            else
            {
                var transactionId = NotificationText(resource, "sale_id") ?? NotificationText(resource, "id");
                subscription = transactionId is null ? null : await subscriptions.FindPaymentAsync(transactionId, ct);
                if (subscription is null && resource.TryGetProperty("links", out var links))
                {
                    if (links.ValueKind != JsonValueKind.Array)
                        throw new SubscriptionOperationException(400, "Liens de notification PayPal invalides.");
                    foreach (var link in links.EnumerateArray())
                    {
                        if (link.ValueKind != JsonValueKind.Object)
                            throw new SubscriptionOperationException(400, "Lien de notification PayPal invalide.");
                        if (!Uri.TryCreate(NotificationText(link, "href"), UriKind.Absolute, out var uri)
                            || uri.Scheme != "https" || uri.Host is not ("api.paypal.com" or "api.sandbox.paypal.com" or "api-m.paypal.com" or "api-m.sandbox.paypal.com")
                            || !uri.AbsolutePath.StartsWith("/v1/payments/sale/", StringComparison.Ordinal)) continue;
                        subscription = await subscriptions.FindPaymentAsync(uri.Segments[^1], ct);
                        if (subscription is not null) break;
                    }
                }
            }
            if (subscription is null) return NotFound();
            // The signed notification is a trigger, not the payment authority.
            // Re-fetch the bound subscription and all transactions from PayPal.
            await subscriptions.ReconcileAsync(subscription, ct);
            return NoContent();
        }
    }

    private async Task<ResourceSubscription?> OwnedAsync(Guid id, CancellationToken ct)
        => (await subscriptions.GetAllAsync(User.GetUserId(), ct)).SingleOrDefault(s => s.Id == id);

    private static string? NotificationText(JsonElement value, string property)
    {
        if (!value.TryGetProperty(property, out var found)) return null;
        return found.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(found.GetString())
            ? found.GetString() : throw new SubscriptionOperationException(400, "Reference de notification PayPal invalide.");
    }

    private string SiteRoot()
    {
        if (!Uri.TryCreate(site.Value.ExternalUrl, UriKind.Absolute, out var uri)
            || uri.Scheme != "https" || !string.IsNullOrEmpty(uri.UserInfo))
            throw new SubscriptionOperationException(503, "L'adresse HTTPS publique du site n'est pas configuree.");
        return site.Value.ExternalUrl.TrimEnd('/') + "/";
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();
        if (executed.Exception is not SubscriptionOperationException error || Response.HasStarted) return;
        logger.LogError(error, "Resource subscription operation failed for {Action}", context.ActionDescriptor.DisplayName);
        executed.ExceptionHandled = true;
        executed.Result = Problem(statusCode: error.StatusCode, title: "Abonnement indisponible", detail: error.Message);
    }
}
