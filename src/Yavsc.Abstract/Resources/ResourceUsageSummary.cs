namespace Yavsc.Abstract.Resources;

/// <summary>
/// Lightweight usage snapshot for PostIt / Yavsc resource accounting.
/// Counters are cumulative; consumption is expressed in points, not currency.
/// </summary>
public sealed class ResourceUsageSummary
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTimeOffset RecordedAtUtc { get; set; }

    public decimal ApiCalls { get; set; }
    public decimal CpuSeconds { get; set; }
    public decimal BandwidthMb { get; set; }
    public decimal StorageMb { get; set; }

    public decimal Consumption { get; set; }
    public decimal GrossConsumption { get; set; }
    public decimal CreditBalance { get; set; }
    public decimal AppliedCredits { get; set; }
    public decimal ConsumptionLimit { get; set; }
    public decimal RemainingConsumption { get; set; }
    public bool IsQuotaReached { get; set; }
    public long? DiskQuota { get; set; }
    public DateTimeOffset? PremiumDiskQuotaUntil { get; set; }
    public DateTimeOffset? DiscoveryDiskQuotaUntil { get; set; }

    public decimal EstimatedCompensation { get; set; }
    public string Currency { get; set; } = Constants.ResourceUsageDefaultCurrency;
}
