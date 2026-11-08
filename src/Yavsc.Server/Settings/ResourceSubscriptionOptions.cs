#nullable enable
namespace Yavsc.Settings;

public sealed class ResourceSubscriptionOptions
{
    public const string SectionName = "ResourceSubscriptions";
    public string DiscoveryPlanId { get; set; } = "";
    public string PremiumPlanId { get; set; } = "";
    public string WebhookId { get; set; } = "";
    public decimal DiscoveryPriceExcludingTax { get; set; } = 1m;
    public decimal PremiumPriceExcludingTax { get; set; } = 2m;
    public decimal VatPercent { get; set; } = 20m;

    public decimal TotalPrice(decimal net) => decimal.Round(net * (1m + VatPercent / 100m), 2, MidpointRounding.AwayFromZero);
}
