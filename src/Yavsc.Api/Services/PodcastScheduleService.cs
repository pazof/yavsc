#nullable enable

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Yavsc.Abstract.Streaming;
using Yavsc.Models;
using Yavsc.Models.Streaming;
using Yavsc.Services;

namespace Yavsc.Api.Services;

public interface IPodcastScheduleService
{
    Task<PodcastResponse> PrepareAsync(string ownerId, PreparePodcastRequest request, Guid? id, CancellationToken ct);
    Task<PodcastResponse> StartAsync(Guid id, string ownerId, CancellationToken ct, Guid? obsConnectionId = null);
    Task<IReadOnlyList<PodcastResponse>> ListAsync(string ownerId, CancellationToken ct);
    Task<PodcastManifestResponse> GetAsync(Guid id, string ownerId, CancellationToken ct);
    Task<IReadOnlyList<PodcastAnnouncementResponse>> AnnouncementsAsync(CancellationToken ct);
    Task DeleteAsync(Guid id, string ownerId, CancellationToken ct);
}

public sealed class PodcastScheduleService(ApplicationDbContext db, ILiveProcessor processor,
    IOptions<PodcastOptions> options) : IPodcastScheduleService
{
    // The media store is single-process; serialize database/media lifecycle changes on that same host.
    private static readonly SemaphoreSlim Gate = new(1);

    public async Task<PodcastResponse> PrepareAsync(string ownerId, PreparePodcastRequest request, Guid? id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length > 160
            || request.ScheduledStartAt == default || request.PlannedDurationMinutes < 1
            || request.PublicAnnouncement?.Length > 1023)
            throw new PodcastException(400, "Titre, debut et duree positive en minutes requis.");
        await Gate.WaitAsync(ct);
        try
        {
            var flow = id.HasValue ? await OwnedAsync(id.Value, ownerId, ct) : null;
            if (flow?.ObsKeyHash is not null)
                throw new PodcastException(409, "Desactivez l'entree OBS avant de modifier l'emission.");
            if (flow != null && (flow.StartedAt != null
                || (await processor.ListAsync(ct)).Any(p => p.Id == id)))
                throw new PodcastException(409, "Une emission demarree ne peut plus etre reprogrammee.");
            if (flow == null)
            {
                if (!await db.Users.AnyAsync(u => u.Id == ownerId, ct))
                    throw new PodcastException(404, "Compte introuvable.");
                if (await db.LiveFlow.CountAsync(f => f.OwnerId == ownerId && f.PodcastId != null && !f.PodcastDeleted, ct)
                    >= options.Value.MaxOwnerPodcasts)
                    throw new PodcastException(413, "Nombre maximal d'emissions atteint. Supprimez une ancienne emission.");
                flow = new LiveFlow
                {
                    PodcastId = Guid.NewGuid(), OwnerId = ownerId, PreparedAt = DateTimeOffset.UtcNow,
                    Title = "", Pitch = "", MediaType = "application/vnd.apple.mpegurl", DifferedFileName = ""
                };
                db.LiveFlow.Add(flow);
            }
            flow.Title = request.Title.Trim();
            flow.ScheduledStartAt = request.ScheduledStartAt.ToUniversalTime();
            flow.PlannedDurationMinutes = request.PlannedDurationMinutes;
            flow.PublicAnnouncement = request.PublicAnnouncement?.Trim() ?? "";
            await db.SaveChangesAsync(ct);
            await db.Entry(flow).Reference(f => f.Owner).LoadAsync(ct);
            return Planned(flow);
        }
        finally { Gate.Release(); }
    }

    public async Task<PodcastResponse> StartAsync(Guid id, string ownerId, CancellationToken ct, Guid? obsConnectionId = null)
    {
        await Gate.WaitAsync(ct);
        try
        {
            return await Yavsc.Server.Helpers.ResourceUsageTracker.WithUserLockAsync(db, ownerId, async () =>
            {
                var flow = await OwnedAsync(id, ownerId, ct);
                await db.Entry(flow).ReloadAsync(ct);
                await db.Entry(flow.Owner).ReloadAsync(ct);
                if (flow.Owner.WithdrawalRequestedAt is not null)
                    throw new PodcastException(403, "Le compte est ferme.");
                if (obsConnectionId.HasValue && (flow.ObsConnectionId != obsConnectionId
                    || flow.ObsKeyHash is null || flow.ObsEnabledUntil <= DateTimeOffset.UtcNow))
                    throw new OperationCanceledException("OBS connection revoked or activation expired.", ct);
                if (!obsConnectionId.HasValue && flow.ObsKeyHash is not null)
                    throw new PodcastException(409, "Desactivez OBS avant de demarrer une capture PostIt.");
                var existing = (await processor.ListAsync(ct)).SingleOrDefault(p => p.Id == id);
                if (existing != null && existing.State != PodcastState.Live)
                    throw new PodcastException(409, "Cette emission est terminee. Preparez une nouvelle emission.");
                if (flow.StartedAt != null && existing == null)
                    throw new PodcastException(404, "Les medias de cette emission sont introuvables.");
                var podcast = await processor.CreateAsync(ownerId, flow.Owner.UserName ?? ownerId, flow.Title, ct, id);
                flow.StartedAt ??= podcast.CreatedAt;
                await db.SaveChangesAsync(ct);
                return Enrich(podcast, flow);
            }, ct);
        }
        finally { Gate.Release(); }
    }

    public async Task<IReadOnlyList<PodcastResponse>> ListAsync(string ownerId, CancellationToken ct)
    {
        var media = await processor.ListAsync(ct);
        var flows = await db.LiveFlow.AsNoTracking().Include(f => f.Owner)
            .Where(f => f.PodcastId != null).ToListAsync(ct);
        var byId = flows.ToDictionary(f => f.PodcastId!.Value);
        var result = media.Where(p => !byId.TryGetValue(p.Id, out var f) || !f.PodcastDeleted)
            .Select(p => byId.TryGetValue(p.Id, out var f) ? Enrich(p, f) : p).ToList();
        var mediaIds = media.Select(p => p.Id).ToHashSet();
        result.AddRange(flows.Where(f => !f.PodcastDeleted && f.StartedAt == null && f.OwnerId == ownerId
            && !mediaIds.Contains(f.PodcastId!.Value)).Select(Planned));
        return result.OrderByDescending(p => p.CreatedAt).ToArray();
    }

    public async Task<PodcastManifestResponse> GetAsync(Guid id, string ownerId, CancellationToken ct)
    {
        var flow = await db.LiveFlow.AsNoTracking().Include(f => f.Owner).SingleOrDefaultAsync(f => f.PodcastId == id, ct);
        if (flow == null) return await processor.GetAsync(id, ct);
        if (flow.PodcastDeleted) throw new PodcastException(404, "Podcast introuvable.");
        var podcast = (await processor.ListAsync(ct)).SingleOrDefault(p => p.Id == id);
        if (podcast != null)
        {
            var manifest = await processor.GetAsync(id, ct);
            return manifest with { Podcast = Enrich(manifest.Podcast, flow) };
        }
        if (flow.OwnerId != ownerId || flow.StartedAt != null)
            throw new PodcastException(404, "Podcast introuvable.");
        return new(Planned(flow), []);
    }

    public async Task<IReadOnlyList<PodcastAnnouncementResponse>> AnnouncementsAsync(CancellationToken ct)
    {
        var flows = await db.LiveFlow.AsNoTracking().Include(f => f.Owner)
            .Where(f => f.PodcastId != null && !f.PodcastDeleted && f.ScheduledStartAt != null
                && f.PlannedDurationMinutes != null && f.PublicAnnouncement != "").ToListAsync(ct);
        return flows.OrderBy(f => f.ScheduledStartAt).Select(f => new PodcastAnnouncementResponse(
            f.PodcastId!.Value, f.Title, f.Owner.UserName ?? f.OwnerId, f.ScheduledStartAt!.Value,
            f.PlannedDurationMinutes!.Value, f.PublicAnnouncement)).ToArray();
    }

    public async Task DeleteAsync(Guid id, string ownerId, CancellationToken ct)
    {
        await Gate.WaitAsync(ct);
        try
        {
            var flow = await db.LiveFlow.SingleOrDefaultAsync(f => f.PodcastId == id, ct);
            if (flow == null) { await processor.DeleteAsync(id, ownerId, ct); return; }
            if (flow.OwnerId != ownerId) throw new PodcastException(403, "Seul l'auteur peut supprimer ce podcast.");
            if (flow.ObsKeyHash is not null)
                throw new PodcastException(409, "Desactivez l'entree OBS avant de supprimer l'emission.");
            var podcast = (await processor.ListAsync(ct)).SingleOrDefault(p => p.Id == id);
            if (podcast?.State == PodcastState.Live)
                throw new PodcastException(409, "Terminez le direct avant de le supprimer.");
            if (podcast != null) await processor.DeleteAsync(id, ownerId, ct);
            var storage = await db.ResourceStorageRecords.AsNoTracking()
                .SingleOrDefaultAsync(r => r.UserId == ownerId && r.Path == $"podcast/{id:N}", ct);
            if (storage is not null)
            {
                await Yavsc.Server.Helpers.ResourceUsageTracker.RecordStorageAsync(db, ownerId,
                    flow.OwnerId, storage.Path, 0, ct, write: async () =>
                    {
                        var owner = await db.Users.SingleAsync(u => u.Id == ownerId, ct);
                        await db.Entry(owner).ReloadAsync(ct);
                        owner.DiskUsage = Math.Max(0, owner.DiskUsage - storage.Bytes);
                    });
            }
            flow.PodcastDeleted = true;
            await db.SaveChangesAsync(ct);
        }
        finally { Gate.Release(); }
    }

    private async Task<LiveFlow> OwnedAsync(Guid id, string ownerId, CancellationToken ct)
    {
        var flow = await db.LiveFlow.Include(f => f.Owner).SingleOrDefaultAsync(f => f.PodcastId == id, ct);
        if (flow == null || flow.PodcastDeleted) throw new PodcastException(404, "Emission introuvable.");
        if (flow.OwnerId != ownerId) throw new PodcastException(403, "Seul l'auteur peut modifier cette emission.");
        return flow;
    }

    private static PodcastResponse Planned(LiveFlow flow) => Enrich(new(
        flow.PodcastId!.Value, flow.Title, flow.OwnerId, flow.Owner.UserName ?? flow.OwnerId,
        flow.PreparedAt!.Value, flow.PreparedAt.Value, PodcastState.Planned, 0, 0, 0), flow);

    private static PodcastResponse Enrich(PodcastResponse podcast, LiveFlow flow) => podcast with
    {
        FlowId = flow.Id, ScheduledStartAt = flow.ScheduledStartAt,
        PlannedDurationMinutes = flow.PlannedDurationMinutes,
        PublicAnnouncement = string.IsNullOrEmpty(flow.PublicAnnouncement) ? null : flow.PublicAnnouncement
    };
}
