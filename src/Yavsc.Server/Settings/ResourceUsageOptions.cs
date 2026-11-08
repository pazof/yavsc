#nullable enable
namespace Yavsc.Settings;

public sealed class ResourceUsageOptions
{
    public const string SectionName = "ResourceUsage";

    public decimal ApiCallWeight { get; set; } = 0.2m;
    public decimal CpuSecondWeight { get; set; } = 0.1m;
    public decimal BandwidthMbWeight { get; set; } = 0.05m;
    public decimal StorageMbWeight { get; set; } = 0.5m;
    public decimal DefaultLimit { get; set; } = 1000m;
    public Dictionary<string, decimal> UserLimits { get; set; } = new();

    public decimal GetLimit(string userId)
        => UserLimits.TryGetValue(userId, out var limit) ? limit : DefaultLimit;

    public decimal Calculate(decimal apiCalls, decimal cpuSeconds, decimal bandwidthMb, decimal storageMb)
        => apiCalls * ApiCallWeight + cpuSeconds * CpuSecondWeight
            + bandwidthMb * BandwidthMbWeight + storageMb * StorageMbWeight;
}
