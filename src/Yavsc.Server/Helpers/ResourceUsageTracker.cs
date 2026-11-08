#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Yavsc.Models;
using Yavsc.Settings;

namespace Yavsc.Server.Helpers;

public sealed class ResourceUsageLimitException() : InvalidOperationException("Le seuil de consommation est atteint.")
{
}

public static class ResourceUsageTracker
{
    private static readonly SemaphoreSlim LocalGate = new(1, 1);

    public static Task RecordStorageAsync(ApplicationDbContext db, string userId, string userName,
        string path, long bytes, CancellationToken cancellationToken = default,
        Func<Task>? write = null, ResourceUsageOptions? policy = null, decimal bandwidthMb = 0m)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentOutOfRangeException.ThrowIfNegative(bytes);
        ArgumentOutOfRangeException.ThrowIfNegative(bandwidthMb);
        return WithUserLockAsync(db, userId, async () =>
        {
            if (await IsWithdrawingAsync(db, userId, cancellationToken))
                throw new InvalidOperationException("Storage accounting is closed for this withdrawn account.");
            var stored = await db.ResourceStorageRecords.FindAsync([userId, path], cancellationToken);
            if (stored is not null) await db.Entry(stored).ReloadAsync(cancellationToken);
            var delta = bytes - (stored?.Bytes ?? 0L);
            if (write is not null)
            {
                if (policy is null && (delta > 0 || bandwidthMb > 0m))
                    throw new ArgumentException("Storage admission requires a resource policy.", nameof(policy));
                if (policy is not null && await UpdateCoreAsync(db, userId, userName, 0m, 0, 0m, bandwidthMb, userId,
                    Constants.ResourceUsageDefaultCurrency, policy, Math.Max(0L, delta), cancellationToken) is null)
                {
                    db.GetService<ILoggerFactory>().CreateLogger("ResourceUsageTracker")
                        .LogWarning("Storage admission refused at resource threshold for {UserId}", userId);
                    throw new ResourceUsageLimitException();
                }
                await write();
            }

            if (stored is null && bytes > 0)
                db.ResourceStorageRecords.Add(new ResourceStorageRecord { UserId = userId, Path = path, Bytes = bytes });
            else if (stored is not null)
            {
                if (bytes == 0) db.ResourceStorageRecords.Remove(stored);
                else stored.Bytes = bytes;
            }
            await UpdateCoreAsync(db, userId, userName, 0m, delta, 0m, 0m,
                userId, Constants.ResourceUsageDefaultCurrency, null, 0, cancellationToken);
            await db.SaveChangesAsync(userId, cancellationToken);
            return true;
        }, cancellationToken);
    }

    public static Task MoveStorageAsync(ApplicationDbContext db, string userId, string from, string to,
        CancellationToken cancellationToken = default)
        => WithUserLockAsync(db, userId, async () =>
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(from);
            ArgumentException.ThrowIfNullOrWhiteSpace(to);
            if (from == to) return true;
            var records = await db.ResourceStorageRecords.Where(r => r.UserId == userId
                && (r.Path == from || r.Path.StartsWith(from + "/"))).ToListAsync(cancellationToken);
            foreach (var record in records)
            {
                db.ResourceStorageRecords.Remove(record);
                db.ResourceStorageRecords.Add(new ResourceStorageRecord
                {
                    UserId = userId,
                    Path = to + record.Path.Substring(from.Length),
                    Bytes = record.Bytes
                });
            }
            await db.SaveChangesAsync(userId, cancellationToken);
            return true;
        }, cancellationToken);

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
            await UpdateCoreAsync(dbContext, userId, userName, cpuSeconds, storageBytes, apiCalls,
                bandwidthMb, actorUserId, currency, policy, anticipatedStorageBytes, cancellationToken), cancellationToken);
    }

    private static async Task<ResourceUsageRecord?> UpdateCoreAsync(ApplicationDbContext dbContext,
        string userId, string userName, decimal cpuSeconds, long storageBytes, decimal apiCalls,
        decimal bandwidthMb, string? actorUserId, string currency, ResourceUsageOptions? policy,
        long anticipatedStorageBytes, CancellationToken cancellationToken)
    {
        if (await IsWithdrawingAsync(dbContext, userId, cancellationToken)) return null;
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
        record.StorageMb = Math.Max(0m, record.StorageMb + storageMb);
        record.Currency = string.IsNullOrWhiteSpace(currency) ? record.Currency : currency;
        record.DateModified = now;
        record.UserModified = effectiveActor;

        await dbContext.SaveChangesAsync(effectiveActor, cancellationToken);
        return record;
    }

    private static async Task<bool> IsWithdrawingAsync(ApplicationDbContext db, string userId, CancellationToken ct)
    {
        if (!await db.Users.AsNoTracking().AnyAsync(u => u.Id == userId && u.WithdrawalRequestedAt != null, ct))
            return false;
        db.GetService<ILoggerFactory>().CreateLogger("ResourceUsageTracker")
            .LogWarning("Refused resource accounting for withdrawn account {UserId}", userId);
        return true;
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
