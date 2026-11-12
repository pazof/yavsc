#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
    Task<ResourceUsageSummary> EraseAsync(string userId, string actorUserId,
        EraseResourceUsageRequest request, CancellationToken cancellationToken);
}

public sealed class ResourceUsageAdjustmentException(int statusCode, string message) : InvalidOperationException(message)
{
    public int StatusCode { get; } = statusCode;
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

    public Task<ResourceUsageSummary> EraseAsync(string userId, string actorUserId,
        EraseResourceUsageRequest request, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(actorUserId);
        const decimal maximum = 99999999999999999999.99999999m;
        if (request.Points <= 0m || request.Points > maximum ||
            decimal.Round(request.Points, 8) != request.Points ||
            request.ExpectedCredits < 0m || request.ExpectedCredits > maximum ||
            decimal.Round(request.ExpectedCredits, 8) != request.ExpectedCredits)
            throw new ResourceUsageAdjustmentException(400, "Le montant doit être positif, avec au plus huit décimales.");

        return ResourceUsageTracker.WithUserLockAsync(dbContext, userId, async () =>
        {
            var user = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == userId, cancellationToken)
                ?? throw new ResourceUsageAdjustmentException(404, "Utilisateur introuvable.");
            if (user.WithdrawalRequestedAt is not null)
                throw new ResourceUsageAdjustmentException(409, "Ce compte est retiré ou en cours de retrait.");
            var record = await dbContext.ResourceUsageRecords.Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RecordedAtUtc).ThenByDescending(r => r.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (record is null)
                throw new ResourceUsageAdjustmentException(409, "Cet utilisateur n’a pas atteint sa limite.");
            await dbContext.Entry(record).ReloadAsync(cancellationToken);
            if (record.CreditPoints != request.ExpectedCredits)
                throw new ResourceUsageAdjustmentException(409, "Les crédits ont changé. Actualisez puis confirmez à nouveau.");
            var current = ToSummary(record, user);
            if (!current.IsQuotaReached)
                throw new ResourceUsageAdjustmentException(409, "Cet utilisateur n’a pas atteint sa limite.");
            if (request.Points > current.Consumption)
                throw new ResourceUsageAdjustmentException(409, "Le montant dépasse les points actuellement consommés.");
            if (request.Points > maximum - record.CreditPoints)
                throw new ResourceUsageAdjustmentException(409, "La capacité du compteur de crédits serait dépassée.");
            record.CreditPoints += request.Points;
            record.RecordedAtUtc = (clock ?? TimeProvider.System).GetUtcNow().UtcDateTime;
            record.DateModified = record.RecordedAtUtc;
            record.UserModified = actorUserId;
            await dbContext.SaveChangesAsync(actorUserId, cancellationToken);
            dbContext.GetService<ILoggerFactory>().CreateLogger<ResourceUsageService>()
                .LogInformation("Administrator {ActorUserId} erased {Points} consumption points for {UserId}; remaining consumption {Consumption}",
                    actorUserId, request.Points, userId, current.Consumption - request.Points);
            return ToSummary(record, user);
        }, cancellationToken);
    }

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
        services.AddSingleton<ResourceUsageNotificationInterceptor>();
        services.ConfigureDbContext<ApplicationDbContext>((provider, options) =>
            options.AddInterceptors(provider.GetRequiredService<ResourceUsageNotificationInterceptor>()));
        return services.AddScoped<IResourceUsageService, ResourceUsageService>();
    }
}
