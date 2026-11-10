#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Abstract.Resources;

/// <summary>Monthly resource offer configured by the instance.</summary>
public sealed record ResourceSubscriptionOfferResponse(string Pack, string Name, decimal PriceExcludingTax,
    decimal VatPercent, decimal PriceIncludingTax, decimal MonthlyPoints, int DiskQuotaMultiplier, bool Available);

/// <summary>Subscription status without merchant credentials or PayPal identifiers.</summary>
public sealed record ResourceSubscriptionResponse(Guid Id, string Pack, string State,
    decimal PriceIncludingTax, decimal MonthlyPoints, DateTimeOffset? NextBillingAt);

/// <summary>Current user's subscription catalogue, history and resource balance.</summary>
public sealed record ResourceSubscriptionsResponse(IReadOnlyList<ResourceSubscriptionOfferResponse> Offers,
    IReadOnlyList<ResourceSubscriptionResponse> Subscriptions, ResourceUsageSummary Usage);

/// <summary>Chooses a server-priced monthly pack; amounts are never supplied by the client.</summary>
public sealed record CreateResourceSubscriptionRequest
{
    [Required, RegularExpression("^(Discovery|Premium)$")]
    public required string Pack { get; init; }
}

/// <summary>Prepared subscription and optional PayPal approval link; not proof of payment.</summary>
public sealed record ResourceSubscriptionCheckoutResponse(ResourceSubscriptionResponse Subscription, string? ApprovalUrl);
