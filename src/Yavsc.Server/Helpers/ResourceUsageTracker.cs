#nullable enable
using Microsoft.EntityFrameworkCore;
using Yavsc.Models;

namespace Yavsc.Server.Helpers;

public static class ResourceUsageTracker
{
    public static async Task RecordAsync(
        ApplicationDbContext dbContext,
        string userId,
        string userName,
        decimal cpuSeconds = 0m,
        long storageBytes = 0,
        decimal apiCalls = 0m,
        decimal bandwidthMb = 0m,
        string? actorUserId = null,
        string currency = Constants.ResourceUsageDefaultCurrency)
    {
        if (dbContext is null || string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        var effectiveActor = string.IsNullOrWhiteSpace(actorUserId) ? userId : actorUserId;
        var now = DateTime.UtcNow;
        var storageMb = storageBytes <= 0 ? 0m : (decimal)storageBytes / (1024m * 1024m);

        var record = await dbContext.ResourceUsageRecords
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.RecordedAtUtc)
            .FirstOrDefaultAsync();

        if (record is null)
        {
            record = new ResourceUsageRecord
            {
                UserId = userId,
                UserName = string.IsNullOrWhiteSpace(userName) ? userId : userName,
                RecordedAtUtc = now,
                ApiCalls = apiCalls,
                CpuSeconds = cpuSeconds,
                BandwidthMb = bandwidthMb,
                StorageMb = storageMb,
                EstimatedCompensation = 0m,
                Currency = currency,
                UserCreated = effectiveActor,
                UserModified = effectiveActor,
                DateCreated = now,
                DateModified = now,
            };
            dbContext.ResourceUsageRecords.Add(record);
            await dbContext.SaveChangesAsync(effectiveActor);
            return;
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

        await dbContext.SaveChangesAsync(effectiveActor);
    }
}
