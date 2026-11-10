#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Yavsc.Abstract.Resources;
using Yavsc.Models.Payment;
using Yavsc.Server.Helpers;
using Yavsc.Settings;
using Yavsc.Web.Services;

namespace Yavsc.Controllers;

[ApiController]
[Route("api/v1/resource-subscriptions")]
[Authorize(AuthenticationSchemes = "Bearer")]
[ResourceUsageExempt]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class ResourceSubscriptionsApiController(IResourceSubscriptionService subscriptions,
    IResourceUsageService usage, IOptions<ResourceSubscriptionOptions> options,
    IOptions<ResourceUsageOptions> usageOptions, IOptions<SiteSettings> site,
    ILogger<ResourceSubscriptionsApiController> logger) : Controller
{
    [HttpGet]
    [ProducesResponseType<ResourceSubscriptionsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ResourceSubscriptionsResponse>> Get(CancellationToken ct)
    {
        var prices = options.Value;
        ResourceSubscriptionOfferResponse Offer(string pack, string name, decimal net, string plan, int points, int disk)
            => new(pack, name, net, prices.VatPercent, prices.TotalPrice(net),
                usageOptions.Value.DefaultLimit * points, disk,
                !string.IsNullOrWhiteSpace(plan) && !string.IsNullOrWhiteSpace(prices.WebhookId));
        return Ok(new ResourceSubscriptionsResponse(
            [Offer("Discovery", "Découverte", prices.DiscoveryPriceExcludingTax, prices.DiscoveryPlanId, 1, 2),
             Offer("Premium", "Premium", prices.PremiumPriceExcludingTax, prices.PremiumPlanId, 3, 5)],
            (await subscriptions.GetAllAsync(User.GetUserId(), ct)).Select(Response).ToArray(),
            await usage.GetCurrentAsync(User.GetUserId(), User.Identity?.Name ?? User.GetUserId(), ct)));
    }

    [HttpPost]
    [ProducesResponseType<ResourceSubscriptionCheckoutResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ResourceSubscriptionCheckoutResponse>> Create(
        CreateResourceSubscriptionRequest request, CancellationToken ct)
    {
        if (!Uri.TryCreate(site.Value.ExternalUrl, UriKind.Absolute, out var root)
            || root.Scheme != "https" || !string.IsNullOrEmpty(root.UserInfo))
            throw new SubscriptionOperationException(503, "L'adresse HTTPS publique du site n'est pas configuree.");
        var returnUrl = site.Value.ExternalUrl.TrimEnd('/') + "/api/v1/resource-subscriptions/return";
        var subscription = await subscriptions.PrepareAsync(User.GetUserId(), request.Pack, ct);
        var approval = await subscriptions.CheckoutAsync(subscription, returnUrl, returnUrl, ct);
        return CreatedAtAction(nameof(Get), new ResourceSubscriptionCheckoutResponse(Response(subscription), approval));
    }

    [HttpPost("refresh")]
    [ProducesResponseType<ResourceSubscriptionsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ResourceSubscriptionsResponse>> Refresh(CancellationToken ct)
    {
        foreach (var subscription in await subscriptions.GetAllAsync(User.GetUserId(), ct))
            await subscriptions.ReconcileAsync(subscription, ct);
        return await Get(ct);
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType<ResourceSubscriptionsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceSubscriptionsResponse>> Cancel(Guid id, CancellationToken ct)
    {
        var subscription = (await subscriptions.GetAllAsync(User.GetUserId(), ct)).SingleOrDefault(s => s.Id == id);
        if (subscription is null) return NotFound();
        await subscriptions.CancelAsync(subscription, ct);
        return await Get(ct);
    }

    // A browser return is deliberately anonymous and never grants credit.
    [AllowAnonymous, HttpGet("return")]
    public ContentResult Return() => Content(
        "<!doctype html><html lang=\"fr\"><meta charset=\"utf-8\"><title>PostIt - PayPal</title>" +
        "<h1>Retour dans PostIt</h1><p>Revenez dans PostIt et appuyez sur Actualiser pour verifier " +
        "votre abonnement et les paiements confirmes. Ce retour ne confirme aucun paiement.</p></html>", "text/html");

    private static ResourceSubscriptionResponse Response(ResourceSubscription s)
        => new(s.Id, s.Pack, s.State, s.PriceIncludingTax, s.MonthlyPoints, s.NextBillingAt);

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();
        if (executed.Exception is not SubscriptionOperationException error) return;
        logger.LogWarning(error, "Resource subscription API operation failed");
        executed.ExceptionHandled = true;
        executed.Result = new JsonResult(new ProblemDetails
        {
            Status = error.StatusCode,
            Title = error.Message,
            Instance = Request.Path
        })
        {
            StatusCode = error.StatusCode,
            ContentType = "application/problem+json"
        };
    }
}
