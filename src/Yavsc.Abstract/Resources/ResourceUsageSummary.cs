namespace Yavsc.Abstract.Resources;

/// <summary>
/// Lightweight usage snapshot for PostIt / Yavsc resource accounting.
/// The initial implementation is intentionally intentionally small and
/// serialisable so it can be exposed through the API and consumed by the
/// PostIt client without requiring a full database model yet.
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

    public decimal EstimatedCompensation { get; set; }
    public string Currency { get; set; } = Constants.ResourceUsageDefaultCurrency;
}
