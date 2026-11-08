#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Yavsc.Abstract.Resources;
using Yavsc.Models;
using Yavsc.Settings;

namespace Yavsc.Server.Helpers;

public interface IResourceUsageService
{
    Task<ResourceUsageSummary> GetCurrentAsync(string userId, string userName, CancellationToken cancellationToken);
    Task<List<ResourceUsageSummary>> GetOverviewAsync(CancellationToken cancellationToken);
    Task<bool> TryAdmitAsync(string userId, string userName, decimal apiCalls, decimal bandwidthMb,
        long anticipatedStorageBytes, CancellationToken cancellationToken);
}

public sealed class ResourceUsageService(
    ApplicationDbContext dbContext,
    IOptions<ResourceUsageOptions> options,
    TimeProvider? clock = null) : IResourceUsageService
{
    private readonly ResourceUsageOptions _options = options.Value;

    public async Task<ResourceUsageSummary> GetCurrentAsync(
        string userId, string userName, CancellationToken cancellationToken)
    {
        var record = await ResourceUsageTracker.UpdateAsync(dbContext, userId, userName,
            cancellationToken: cancellationToken);
        var user = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);
        return ToSummary(record ?? throw new InvalidOperationException("Usage initialization failed."), user);
    }

    public async Task<List<ResourceUsageSummary>> GetOverviewAsync(CancellationToken cancellationToken)
    {
        var records = await dbContext.ResourceUsageRecords.AsNoTracking()
            .OrderByDescending(r => r.RecordedAtUtc).ThenByDescending(r => r.Id)
            .ToListAsync(cancellationToken);
        var ids = records.Select(r => r.UserId).Distinct().ToArray();
        var users = await dbContext.Users.AsNoTracking().Where(u => ids.Contains(u.Id)).ToDictionaryAsync(u => u.Id, cancellationToken);
        return records.GroupBy(r => r.UserId).Select(g => ToSummary(g.First(), users.GetValueOrDefault(g.Key))).ToList();
    }

    public async Task<bool> TryAdmitAsync(
        string userId, string userName, decimal apiCalls, decimal bandwidthMb,
        long anticipatedStorageBytes, CancellationToken cancellationToken)
        => await ResourceUsageTracker.UpdateAsync(dbContext, userId, userName,
            apiCalls: apiCalls, bandwidthMb: bandwidthMb,
            policy: _options, anticipatedStorageBytes: anticipatedStorageBytes,
            cancellationToken: cancellationToken) is not null;

    private ResourceUsageSummary ToSummary(ResourceUsageRecord record, ApplicationUser? user)
    {
        var gross = _options.Calculate(record.ApiCalls, record.CpuSeconds, record.BandwidthMb, record.StorageMb);
        var consumption = ResourceUsageCredit.NetConsumption(gross, record.CreditPoints);
        var limit = _options.GetLimit(record.UserId);
        return new ResourceUsageSummary
        {
            UserId = record.UserId,
            UserName = record.UserName,
            RecordedAtUtc = new DateTimeOffset(DateTime.SpecifyKind(record.RecordedAtUtc, DateTimeKind.Utc)),
            ApiCalls = record.ApiCalls,
            CpuSeconds = record.CpuSeconds,
            BandwidthMb = record.BandwidthMb,
            StorageMb = record.StorageMb,
            EstimatedCompensation = record.EstimatedCompensation,
            Currency = record.Currency,
            Consumption = consumption,
            GrossConsumption = gross,
            CreditBalance = ResourceUsageCredit.Balance(gross, record.CreditPoints),
            AppliedCredits = Math.Min(gross, record.CreditPoints),
            ConsumptionLimit = limit,
            RemainingConsumption = Math.Max(0m, limit - consumption) + ResourceUsageCredit.Balance(gross, record.CreditPoints),
            IsQuotaReached = consumption >= limit,
            DiskQuota = user?.GetEffectiveDiskQuota((clock ?? TimeProvider.System).GetUtcNow()),
            PremiumDiskQuotaUntil = user?.PremiumDiskQuotaUntil,
            DiscoveryDiskQuotaUntil = user?.DiscoveryDiskQuotaUntil
        };
    }
}

public static class ResourceUsageServiceCollectionExtensions
{
    public static IServiceCollection AddResourceUsage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ResourceUsageOptions>()
            .Bind(configuration.GetSection(ResourceUsageOptions.SectionName))
            .Validate(o => o.ApiCallWeight > 0m && o.CpuSecondWeight >= 0m
                && o.BandwidthMbWeight >= 0m && o.StorageMbWeight >= 0m,
                "ResourceUsage weights must be non-negative and ApiCallWeight must be positive.")
            .Validate(o => o.DefaultLimit > 0m && o.UserLimits.All(p => !string.IsNullOrWhiteSpace(p.Key) && p.Value > 0m),
                "ResourceUsage limits must be positive and keyed by user ID.")
            .ValidateOnStart();
        return services.AddScoped<IResourceUsageService, ResourceUsageService>();
    }
}
