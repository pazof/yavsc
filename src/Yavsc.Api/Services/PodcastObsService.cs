#nullable enable
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Yavsc.Abstract.Streaming;
using Yavsc.Models;
using Yavsc.Models.Streaming;
using Yavsc.Server.Helpers;
using Yavsc.Services;
using Yavsc.Settings;

namespace Yavsc.Api.Services;

public interface IPodcastObsService
{
    Task<PodcastObsCredentials> ActivateAsync(Guid id, string ownerId, CancellationToken ct);
    Task<PodcastObsStatus> StatusAsync(Guid id, string ownerId, CancellationToken ct);
    Task DeactivateAsync(Guid id, string ownerId, CancellationToken ct);
}

public sealed class PodcastObsService(ApplicationDbContext db, IOptions<PodcastObsOptions> options,
    IOptions<ResourceUsageOptions> usageOptions, PodcastObsWorker worker, TimeProvider clock,
    ILogger<PodcastObsService> logger) : IPodcastObsService
{
    public async Task<PodcastObsCredentials> ActivateAsync(Guid id, string ownerId, CancellationToken ct)
    {
        var settings = options.Value;
        if (!settings.Enabled) throw new PodcastException(503, "L'entree OBS n'est pas configuree sur cette instance.");
        settings.Validate();
        await worker.WaitUntilReadyAsync(ct);
        await worker.Lifecycle.WaitAsync(ct);
        try
        {
            await ResourceUsageTracker.WithUserLockAsync(db, ownerId, async () =>
            {
                var flow = await EligibleAsync(id, ownerId, ct);
                flow.ObsKeyHash = null;
                flow.ObsEnabledUntil = null;
                flow.ObsConnectionId = null;
                await db.SaveChangesAsync(ct);
                return true;
            }, ct);
            await worker.StopCaptureAsync(id, ct);
            return await ResourceUsageTracker.WithUserLockAsync(db, ownerId, async () =>
            {
                var flow = await EligibleAsync(id, ownerId, ct);
                var key = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
                var connectionId = Guid.NewGuid();
                var expires = clock.GetUtcNow().AddMinutes(settings.ActivationMinutes);
                flow.ObsKeyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)));
                flow.ObsEnabledUntil = expires;
                flow.ObsConnectionId = connectionId;
                flow.ObsLastError = null;
                await db.SaveChangesAsync(ct);
                try
                {
                    var port = await worker.ArmAsync(id, ownerId, connectionId, key, expires, ct);
                    var url = new UriBuilder(settings.ServerUrl)
                    {
                        Port = port,
                        Query = $"mode=caller&passphrase={key}&pbkeylen=32&enforced_encryption=1&latency=200000"
                    };
                    return new PodcastObsCredentials(url.Uri.AbsoluteUri, "", expires);
                }
                catch (Exception ex)
                {
                    flow.ObsKeyHash = null;
                    flow.ObsEnabledUntil = null;
                    flow.ObsConnectionId = null;
                    flow.ObsLastError = "Activation SRT impossible. Verifiez FFmpeg, les ports UDP et la capacite disponible.";
                    await db.SaveChangesAsync(CancellationToken.None);
                    logger.LogError("OBS listener activation failed ({ErrorType})", ex.GetType().Name);
                    if (ex is OperationCanceledException or PodcastException) throw;
                    throw new PodcastException(503, flow.ObsLastError);
                }
            }, ct);
        }
        finally { worker.Lifecycle.Release(); }
    }

    public async Task<PodcastObsStatus> StatusAsync(Guid id, string ownerId, CancellationToken ct)
    {
        var flow = await OwnedAsync(id, ownerId, ct, tracking: false);
        return new(flow.ObsKeyHash is not null &&
            (flow.StartedAt is not null || flow.ObsEnabledUntil > clock.GetUtcNow()),
            flow.ObsEnabledUntil, flow.ObsLastError);
    }

    public async Task DeactivateAsync(Guid id, string ownerId, CancellationToken ct)
    {
        await worker.Lifecycle.WaitAsync(ct);
        try
        {
            await ResourceUsageTracker.WithUserLockAsync(db, ownerId, async () =>
            {
                var flow = await OwnedAsync(id, ownerId, ct);
                flow.ObsKeyHash = null;
                flow.ObsEnabledUntil = null;
                flow.ObsConnectionId = null;
                await db.SaveChangesAsync(ct);
                return true;
            }, ct);
            await worker.StopCaptureAsync(id, ct);
        }
        finally { worker.Lifecycle.Release(); }
    }

    private async Task<LiveFlow> EligibleAsync(Guid id, string ownerId, CancellationToken ct)
    {
        var flow = await OwnedAsync(id, ownerId, ct);
        if (flow.StartedAt is not null)
            throw new PodcastException(409, "Cette emission a deja demarre. Preparez une nouvelle emission.");
        if (flow.Owner.WithdrawalRequestedAt is not null)
            throw new PodcastException(403, "Le compte est ferme.");
        var record = await db.ResourceUsageRecords.AsNoTracking().Where(r => r.UserId == ownerId)
            .OrderByDescending(r => r.RecordedAtUtc).ThenByDescending(r => r.Id).FirstOrDefaultAsync(ct);
        var policy = usageOptions.Value;
        var gross = record is null ? 0m : policy.Calculate(record.ApiCalls, record.CpuSeconds, record.BandwidthMb, record.StorageMb);
        if (ResourceUsageCredit.NetConsumption(gross, record?.CreditPoints ?? 0m) >= policy.GetLimit(ownerId))
            throw new PodcastException(429, "Le seuil de consommation est atteint.");
        if (flow.Owner.DiskUsage >= flow.Owner.GetEffectiveDiskQuota(clock.GetUtcNow()))
            throw new PodcastException(413, "Le quota disque est atteint.");
        return flow;
    }

    private async Task<LiveFlow> OwnedAsync(Guid id, string ownerId, CancellationToken ct, bool tracking = true)
    {
        var query = db.LiveFlow.Include(f => f.Owner).AsQueryable();
        if (!tracking) query = query.AsNoTracking();
        var flow = await query.SingleOrDefaultAsync(f => f.PodcastId == id && !f.PodcastDeleted, ct)
            ?? throw new PodcastException(404, "Emission introuvable.");
        if (tracking)
        {
            await db.Entry(flow).ReloadAsync(ct);
            await db.Entry(flow.Owner).ReloadAsync(ct);
        }
        if (flow.OwnerId != ownerId) throw new PodcastException(403, "Seul l'auteur peut activer OBS.");
        return flow;
    }
}
