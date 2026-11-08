#nullable enable
using Microsoft.EntityFrameworkCore;
using Yavsc.Models;
using Yavsc.Settings;

namespace Yavsc.Server.Helpers;

public static class ResourceUsageTracker
{
    private static readonly SemaphoreSlim LocalGate = new(1, 1);

    public static async Task RecordAsync(
        ApplicationDbContext dbContext,
        string userId,
        string userName,
        decimal cpuSeconds = 0m,
        long storageBytes = 0,
        decimal apiCalls = 0m,
        decimal bandwidthMb = 0m,
        string? actorUserId = null,
        string currency = Constants.ResourceUsageDefaultCurrency,
        CancellationToken cancellationToken = default)
    {
        await UpdateAsync(dbContext, userId, userName, cpuSeconds, storageBytes,
            apiCalls, bandwidthMb, actorUserId, currency, cancellationToken: cancellationToken);
    }

    internal static async Task<ResourceUsageRecord?> UpdateAsync(
        ApplicationDbContext dbContext, string userId, string userName,
        decimal cpuSeconds = 0m, long storageBytes = 0, decimal apiCalls = 0m,
        decimal bandwidthMb = 0m, string? actorUserId = null,
        string currency = Constants.ResourceUsageDefaultCurrency,
        ResourceUsageOptions? policy = null, long anticipatedStorageBytes = 0,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentOutOfRangeException.ThrowIfNegative(cpuSeconds);
        ArgumentOutOfRangeException.ThrowIfNegative(storageBytes);
        ArgumentOutOfRangeException.ThrowIfNegative(apiCalls);
        ArgumentOutOfRangeException.ThrowIfNegative(bandwidthMb);
        ArgumentOutOfRangeException.ThrowIfNegative(anticipatedStorageBytes);

        return await WithUserLockAsync(dbContext, userId, async () =>
        {
            var effectiveActor = string.IsNullOrWhiteSpace(actorUserId) ? userId : actorUserId;
            var now = DateTime.UtcNow;
            var storageMb = (decimal)storageBytes / (1024m * 1024m);
            var record = await dbContext.ResourceUsageRecords
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RecordedAtUtc).ThenByDescending(r => r.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (record is not null)
                await dbContext.Entry(record).ReloadAsync(cancellationToken);

            if (policy is not null)
            {
                var gross = record is null ? 0m :
                    policy.Calculate(record.ApiCalls, record.CpuSeconds, record.BandwidthMb, record.StorageMb);
                var current = ResourceUsageCredit.NetConsumption(gross, record?.CreditPoints ?? 0m);
                var projected = ResourceUsageCredit.NetConsumption(gross + policy.Calculate(apiCalls, cpuSeconds, bandwidthMb,
                    storageMb + (decimal)anticipatedStorageBytes / (1024m * 1024m)), record?.CreditPoints ?? 0m);
                var limit = policy.GetLimit(userId);
                if (current >= limit || projected > limit) return null;
            }

            if (record is not null && apiCalls == 0m && cpuSeconds == 0m && bandwidthMb == 0m && storageBytes == 0)
                return record;

            if (record is null)
            {
                record = new ResourceUsageRecord
                {
                    UserId = userId,
                    UserName = string.IsNullOrWhiteSpace(userName) ? userId : userName,
                    Currency = currency,
                    UserCreated = effectiveActor,
                    DateCreated = now
                };
                dbContext.ResourceUsageRecords.Add(record);
            }

            record.UserName = string.IsNullOrWhiteSpace(userName) ? record.UserName : userName;
            record.RecordedAtUtc = now;
            record.ApiCalls += apiCalls;
            record.CpuSeconds += cpuSeconds;
            record.BandwidthMb += bandwidthMb;
            record.StorageMb += storageMb;
            record.Currency = string.IsNullOrWhiteSpace(currency) ? record.Currency : currency;
            record.DateModified = now;
            record.UserModified = effectiveActor;

            await dbContext.SaveChangesAsync(effectiveActor, cancellationToken);
            return record;
        }, cancellationToken);
    }

    public static async Task<T> WithUserLockAsync<T>(ApplicationDbContext dbContext, string userId,
        Func<Task<T>> action, CancellationToken cancellationToken)
    {
        // This lock is shared by accounting and subscription credits on all hosts.
        var postgres = dbContext.Database.IsNpgsql();
        if (!postgres) await LocalGate.WaitAsync(cancellationToken);
        try
        {
            await using var transaction = postgres && dbContext.Database.CurrentTransaction is null
                ? await dbContext.Database.BeginTransactionAsync(cancellationToken) : null;
            if (postgres)
                await dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT pg_advisory_xact_lock(hashtextextended({userId}, 781932))", cancellationToken);
            var result = await action();
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            return result;
        }
        finally
        {
            if (!postgres) LocalGate.Release();
        }
    }
}
