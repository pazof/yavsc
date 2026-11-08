#nullable enable
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
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
    Task<PodcastRelayAuthorization?> AuthenticateAsync(PodcastRelayAuthentication request, CancellationToken ct);
}

public sealed class PodcastObsService(ApplicationDbContext db, IOptions<PodcastObsOptions> options,
    IOptions<ResourceUsageOptions> usageOptions, IPodcastObsRelay relay, TimeProvider clock,
    ILogger<PodcastObsService> logger) : IPodcastObsService
{
    private PodcastObsOptions Settings()
    {
        var value = options.Value;
        if (!value.Enabled) throw new PodcastException(503, "L'entree OBS n'est pas configuree sur cette instance.");
        value.Validate();
        return value;
    }

    public Task<PodcastObsCredentials> ActivateAsync(Guid id, string ownerId, CancellationToken ct)
        => ResourceUsageTracker.WithUserLockAsync(db, ownerId, async () =>
        {
            var settings = Settings();
            var flow = await OwnedAsync(id, ownerId, ct);
            if (flow.StartedAt is not null)
                throw new PodcastException(409, "Cette emission a deja demarre. Preparez une nouvelle emission.");
            if (flow.Owner.WithdrawalRequestedAt is not null)
                throw new PodcastException(403, "Le compte est ferme.");
            if (await QuotaReachedAsync(ownerId, ct))
                throw new PodcastException(429, "Le seuil de consommation est atteint.");
            if (flow.Owner.DiskUsage >= flow.Owner.GetEffectiveDiskQuota(clock.GetUtcNow()))
                throw new PodcastException(413, "Le quota disque est atteint.");
            var key = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
            flow.ObsKeyHash = Hash(key);
            flow.ObsEnabledUntil = clock.GetUtcNow().AddMinutes(settings.ActivationMinutes);
            flow.ObsConnectionId = null;
            flow.ObsLastError = null;
            await db.SaveChangesAsync(ct);
            return new PodcastObsCredentials(settings.ServerUrl.TrimEnd('/'),
                $"obs/{id:N}?key={key}", flow.ObsEnabledUntil.Value);
        }, ct);

    public async Task<PodcastObsStatus> StatusAsync(Guid id, string ownerId, CancellationToken ct)
    {
        var flow = await OwnedAsync(id, ownerId, ct, tracking: false);
        return new(flow.ObsKeyHash is not null &&
            (flow.StartedAt is not null || flow.ObsEnabledUntil > clock.GetUtcNow()),
            flow.ObsEnabledUntil, flow.ObsLastError);
    }

    public async Task DeactivateAsync(Guid id, string ownerId, CancellationToken ct)
    {
        var connectionId = await ResourceUsageTracker.WithUserLockAsync(db, ownerId, async () =>
        {
            var flow = await OwnedAsync(id, ownerId, ct);
            var connection = flow.ObsConnectionId;
            flow.ObsKeyHash = null;
            flow.ObsEnabledUntil = null;
            flow.ObsConnectionId = null;
            await db.SaveChangesAsync(ct);
            return connection;
        }, ct);
        if (connectionId.HasValue) await relay.KickAsync(id, ct, connectionId);
    }

    public async Task<PodcastRelayAuthorization?> AuthenticateAsync(PodcastRelayAuthentication request, CancellationToken ct)
    {
        var settings = Settings();
        if (request.User == settings.RelayUser && EqualSecret(request.Password, settings.RelayPassword))
        {
            if (request.Action == "api" || request.Action == "read" && request.Protocol == "rtmp" && TryId(request.Path, out _))
                return new(null);
        }
        if (request.Action == "publish" && request.Protocol == "rtmp" && TryId(request.Path, out var id))
        {
            var query = QueryHelpers.ParseQuery(request.Query ?? "");
            if (query.TryGetValue("key", out var keys) && keys.Count == 1 && keys[0] is { Length: 64 } key
                && Guid.TryParse(request.Id, out var connectionId))
            {
                var flow = await db.LiveFlow.AsNoTracking().Include(f => f.Owner)
                    .SingleOrDefaultAsync(f => f.PodcastId == id && !f.PodcastDeleted, ct);
                if (flow is not null)
                    return await ResourceUsageTracker.WithUserLockAsync<PodcastRelayAuthorization?>(db, flow.OwnerId, async () =>
                    {
                        var current = await db.LiveFlow.Include(f => f.Owner).SingleAsync(f => f.PodcastId == id, ct);
                        await db.Entry(current).ReloadAsync(ct);
                        await db.Entry(current.Owner).ReloadAsync(ct);
                        if (current is { StartedAt: null, ObsKeyHash: not null, PodcastDeleted: false }
                            && (current.ObsConnectionId is null || current.ObsConnectionId == connectionId)
                            && current.ObsEnabledUntil > clock.GetUtcNow() && current.Owner.WithdrawalRequestedAt is null
                            && EqualSecret(Hash(key), current.ObsKeyHash)
                            && !await QuotaReachedAsync(current.OwnerId, ct)
                            && current.Owner.DiskUsage < current.Owner.GetEffectiveDiskQuota(clock.GetUtcNow()))
                        {
                            current.ObsConnectionId = connectionId;
                            await db.SaveChangesAsync(ct);
                            return new(clock.GetUtcNow().AddMinutes(Math.Min(current.PlannedDurationMinutes ?? 1, settings.MaxDurationMinutes)));
                        }
                        logger.LogWarning("OBS publication refused for emission {PodcastId}", id);
                        return null;
                    }, ct);
            }

        }
        logger.LogWarning("OBS relay authentication refused for action {Action}", request.Action is "publish" or "read" or "api" ? request.Action : "other");
        return null;
    }

    private async Task<bool> QuotaReachedAsync(string ownerId, CancellationToken ct)
    {
        var record = await db.ResourceUsageRecords.AsNoTracking().Where(r => r.UserId == ownerId)
            .OrderByDescending(r => r.RecordedAtUtc).ThenByDescending(r => r.Id).FirstOrDefaultAsync(ct);
        var policy = usageOptions.Value;
        var gross = record is null ? 0m : policy.Calculate(record.ApiCalls, record.CpuSeconds, record.BandwidthMb, record.StorageMb);
        return ResourceUsageCredit.NetConsumption(gross, record?.CreditPoints ?? 0m) >= policy.GetLimit(ownerId);
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

    internal static bool TryId(string? path, out Guid id)
    {
        id = default;
        return path is not null && path.StartsWith("obs/", StringComparison.Ordinal) && Guid.TryParseExact(path[4..], "N", out id);
    }

    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    private static bool EqualSecret(string? actual, string expected)
        => actual is not null && CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(actual), Encoding.UTF8.GetBytes(expected));
}
