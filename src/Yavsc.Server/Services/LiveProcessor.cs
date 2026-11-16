#nullable enable

using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Yavsc.Abstract.Streaming;
using Yavsc.Models;
using Yavsc.ViewModels.Streaming;

namespace Yavsc.Services;

public sealed class LiveProcessor : ILiveProcessor, IDisposable
{
    private sealed record Document(PodcastManifestResponse Manifest, string[] Hashes);
    private const string ReplayFileName = "replay.mp4";
    private const string SourceFileName = "source.mp4";
    public const int MaxDraftMarks = 500;
    private readonly ILogger<LiveProcessor> _logger;
    private readonly PodcastOptions _options;
    private readonly TimeProvider _clock;
    private readonly SemaphoreSlim _gate = new(1);
    private readonly SemaphoreSlim _uploads = new(2);
    private readonly SemaphoreSlim _replays = new(1);
    private Guid? _preparingReplay;
    private readonly Dictionary<Guid, Document> _documents = new();
    private bool _loaded;

    public ConcurrentDictionary<string, LiveCastHandler> Casters { get; } = new();

    public LiveProcessor(ILoggerFactory loggerFactory, IOptions<PodcastOptions> options, TimeProvider? clock = null)
    {
        _logger = loggerFactory.CreateLogger<LiveProcessor>();
        _options = options.Value;
        _clock = clock ?? TimeProvider.System;
        if (!Path.IsPathFullyQualified(_options.RootPath) || _options.MaxSegmentBytes < 188
            || _options.MaxSegmentBytes > 8 * 1024 * 1024
            || _options.MaxOwnerBytes < _options.MaxSegmentBytes || _options.MaxSegments < 1
            || _options.IdleTimeoutSeconds < 15 || _options.MaxOwnerPodcasts < 1
            || _options.ReplayTimeoutSeconds < 1 || string.IsNullOrWhiteSpace(_options.FfmpegPath))
            throw new InvalidOperationException("Invalid Podcast storage options.");
    }

    public async Task<PodcastResponse> CreateAsync(string ownerId, string ownerName, string title, CancellationToken ct,
        Guid? id = null, bool refineReplay = false, int retentionDays = PodcastRetention.DefaultDays)
    {
        if (string.IsNullOrWhiteSpace(ownerId) || string.IsNullOrWhiteSpace(title) || title.Trim().Length > 160)
            throw new PodcastException(400, "Un titre de 1 a 160 caracteres et un utilisateur sont requis.");
        if (retentionDays is < 0 or > PodcastRetention.MaxDays)
            throw new PodcastException(400, $"La conservation des originaux va de 0 a {PodcastRetention.MaxDays} jours.");
        await _gate.WaitAsync(ct);
        try
        {
            await LoadAsync(ct);
            await ExpireAsync(ct);
            if (id.HasValue && _documents.TryGetValue(id.Value, out var existing))
            {
                if (existing.Manifest.Podcast.OwnerId != ownerId)
                    throw new PodcastException(403, "Seul l'auteur peut demarrer ce podcast.");
                return existing.Manifest.Podcast;
            }
            if (_documents.Values.Any(d => d.Manifest.Podcast.OwnerId == ownerId && d.Manifest.Podcast.State == PodcastState.Live))
                throw new PodcastException(409, "Vous avez deja un direct en cours.");
            if (OwnerBytes(ownerId) >= _options.MaxOwnerBytes)
                throw new PodcastException(413, "Quota de stockage des podcasts atteint.");
            if (_documents.Values.Count(d => d.Manifest.Podcast.OwnerId == ownerId) >= _options.MaxOwnerPodcasts)
                throw new PodcastException(413, "Nombre maximal de podcasts atteint. Supprimez un ancien replay.");
            var now = _clock.GetUtcNow();
            var podcast = new PodcastResponse(id ?? Guid.NewGuid(), title.Trim(), ownerId, ownerName,
                now, now, PodcastState.Live, 0, 0, 0) { RefineReplay = refineReplay, RetentionDays = retentionDays };
            var document = new Document(new(podcast, Array.Empty<PodcastSegmentResponse>()), Array.Empty<string>());
            Directory.CreateDirectory(DirectoryFor(podcast.Id));
            await SaveAsync(document, ct);
            _logger.LogInformation("Podcast {PodcastId} created by {OwnerId}", podcast.Id, ownerId);
            return podcast;
        }
        finally { _gate.Release(); }
    }

    public async Task<IReadOnlyList<PodcastResponse>> ListAsync(CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            await LoadAsync(ct);
            await ExpireAsync(ct);
            return _documents.Values.Select(d => d.Manifest.Podcast)
                .OrderByDescending(p => p.CreatedAt).ToArray();
        }
        finally { _gate.Release(); }
    }

    public async Task<PodcastManifestResponse> GetAsync(Guid id, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            await LoadAsync(ct);
            await ExpireAsync(ct);
            var manifest = Find(id).Manifest;
            return manifest with { Segments = manifest.Segments.ToArray() };
        }
        finally { _gate.Release(); }
    }

    public async Task<PodcastResponse> AppendAsync(Guid id, string ownerId, int sequence,
        double durationSeconds, Stream input, CancellationToken ct)
    {
        if (sequence < 0 || !double.IsFinite(durationSeconds) || durationSeconds <= 0 || durationSeconds > 15)
            throw new PodcastException(400, "Sequence ou duree du segment invalide (maximum 15 secondes).");

        await _gate.WaitAsync(ct);
        try
        {
            await LoadAsync(ct);
            await ExpireAsync(ct);
            _ = Owned(id, ownerId);
        }
        finally { _gate.Release(); }

        // Bound concurrent body buffering without holding the metadata lock during network reads.
        await _uploads.WaitAsync(ct);
        try
        {
            var bytes = await ReadSegmentAsync(input, ct);
            await _gate.WaitAsync(ct);
            try
            {
                var document = Owned(id, ownerId);
                var podcast = document.Manifest.Podcast;
                var segments = document.Manifest.Segments;
                if (sequence > segments.Count)
                    throw new PodcastException(409, $"Le prochain segment attendu est {segments.Count}.");
                if (sequence == segments.Count && podcast.State != PodcastState.Live)
                    throw new PodcastException(409, "Ce direct est termine.");
                var hash = Convert.ToHexString(SHA256.HashData(bytes));
                if (sequence < segments.Count)
                {
                    if (document.Hashes[sequence] != hash || segments[sequence].DurationSeconds != durationSeconds)
                        throw new PodcastException(409, "Un autre contenu existe deja pour cette sequence.");
                    return podcast;
                }
                if (segments.Count >= _options.MaxSegments || OwnerBytes(ownerId) + bytes.Length > _options.MaxOwnerBytes)
                    throw new PodcastException(413, "Quota de stockage ou duree maximale du podcast atteint.");

                var path = SegmentPath(id, sequence);
                await File.WriteAllBytesAsync(path + ".tmp", bytes, ct);
                File.Move(path + ".tmp", path, true);
                var updated = podcast with
                {
                    UpdatedAt = _clock.GetUtcNow(), SegmentCount = segments.Count + 1,
                    Bytes = podcast.Bytes + bytes.Length, DurationSeconds = podcast.DurationSeconds + durationSeconds
                };
                await SaveAsync(new(new(updated,
                    segments.Append(new PodcastSegmentResponse(sequence, durationSeconds, bytes.Length)).ToArray()),
                    document.Hashes.Append(hash).ToArray()), ct);
                return updated;
            }
            finally { _gate.Release(); }
        }
        finally { _uploads.Release(); }
    }

    private async Task<byte[]> ReadSegmentAsync(Stream input, CancellationToken ct)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(20));
        try
        {
            using var buffer = new MemoryStream();
            var chunk = new byte[64 * 1024];
            int count;
            while ((count = await input.ReadAsync(chunk, timeout.Token)) != 0)
            {
                if (buffer.Length + count > _options.MaxSegmentBytes)
                    throw new PodcastException(413, "Segment trop volumineux.");
                await buffer.WriteAsync(chunk.AsMemory(0, count), timeout.Token);
            }
            var bytes = buffer.ToArray();
            ValidateTransportStream(bytes);
            return bytes;
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new PodcastException(408, "Delai de reception du segment depasse.");
        }
    }

    public async Task<PodcastResponse> CompleteAsync(Guid id, string ownerId, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            await LoadAsync(ct);
            var document = Owned(id, ownerId);
            var podcast = document.Manifest.Podcast;
            if (podcast.State == PodcastState.Completed)
                return podcast;
            var now = _clock.GetUtcNow();
            var updated = podcast with
            {
                State = PodcastState.Completed, UpdatedAt = now,
                Replay = podcast.Replay ?? new(PodcastReplayState.Pending),
                Draft = podcast.RefineReplay ? podcast.Draft ?? NewDraft() : null,
                OriginalsExpireAt = podcast.OriginalsExpireAt ?? EndExpiry(podcast, now)
            };
            await SaveAsync(document with { Manifest = document.Manifest with { Podcast = updated } }, ct);
            _logger.LogInformation("Podcast {PodcastId} completed with {Segments} segments", id, updated.SegmentCount);
            return updated;
        }
        finally { _gate.Release(); }
    }

    public async Task<Stream> OpenReplayAsync(Guid id, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            await LoadAsync(ct);
            var podcast = Find(id).Manifest.Podcast;
            if (podcast.State != PodcastState.Completed || podcast.Replay?.State != PodcastReplayState.Ready)
                throw new PodcastException(409, "Le replay MP4 n'est pas disponible.");
            return new FileStream(Path.Combine(DirectoryFor(id), ReplayFileName), FileMode.Open, FileAccess.Read,
                FileShare.Read | FileShare.Delete, 64 * 1024, FileOptions.Asynchronous | FileOptions.RandomAccess);
        }
        finally { _gate.Release(); }
    }

    public async Task RetryReplayAsync(Guid id, string ownerId, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            await LoadAsync(ct);
            var document = Owned(id, ownerId);
            var podcast = document.Manifest.Podcast;
            if (podcast.RefineReplay)
            {
                if (podcast.Draft?.State != PodcastDraftState.Failed)
                    throw new PodcastException(409, "Seule une source de brouillon en echec peut etre relancee.");
                await SetDraftAsync(document, podcast.Draft with { State = PodcastDraftState.Preparing, Error = null }, ct);
                return;
            }
            if (podcast.Replay?.State != PodcastReplayState.Failed)
                throw new PodcastException(409, "Seul un replay en echec peut etre relance.");
            await SetReplayAsync(document, new(PodcastReplayState.Pending), ct);
        }
        finally { _gate.Release(); }
    }

    public async Task<bool> PrepareNextReplayAsync(IPodcastReplayRemuxer remuxer, CancellationToken ct)
    {
        await _replays.WaitAsync(ct);
        try { return await PrepareReplayAsync(remuxer, ct); }
        finally { _replays.Release(); }
    }

    private async Task<bool> PrepareReplayAsync(IPodcastReplayRemuxer remuxer, CancellationToken ct)
    {
        Document document;
        long allowance;
        await _gate.WaitAsync(ct);
        try
        {
            await LoadAsync(ct);
            var next = _documents.Values.Where(d => IsRemuxQueued(d.Manifest.Podcast))
                .OrderBy(d => d.Manifest.Podcast.CreatedAt).FirstOrDefault();
            if (next is null) return false;
            document = next;
            allowance = _options.MaxOwnerBytes - OwnerBytes(document.Manifest.Podcast.OwnerId);
            if (!document.Manifest.Podcast.RefineReplay)
                await SetReplayAsync(document, new(PodcastReplayState.Processing), ct);
            _preparingReplay = document.Manifest.Podcast.Id;
        }
        finally { _gate.Release(); }

        var id = document.Manifest.Podcast.Id;
        var directory = DirectoryFor(id);
        // A refined replay is first remuxed into a private, seekable editing source.
        var refine = document.Manifest.Podcast.RefineReplay;
        var target = Path.Combine(directory, refine ? SourceFileName : ReplayFileName);
        var temporary = Path.Combine(directory, refine ? "source.tmp.mp4" : "replay.tmp.mp4");
        try
        {
            if (allowance <= 1) throw new IOException("Quota de stockage des podcasts atteint.");
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(_options.ReplayTimeoutSeconds));
            await remuxer.RemuxAsync(directory, document.Manifest.Segments.Count, temporary, allowance, timeout.Token);
            var bytes = new FileInfo(temporary).Length;
            if (bytes <= 0 || bytes > allowance) throw new IOException("Taille du MP4 invalide ou quota atteint.");
            await _gate.WaitAsync(ct);
            try
            {
                var current = Find(id);
                var podcast = current.Manifest.Podcast;
                if (OwnerBytes(podcast.OwnerId) + bytes > _options.MaxOwnerBytes)
                    throw new IOException("Quota de stockage des podcasts atteint.");
                File.Move(temporary, target, overwrite: true);
                var updated = refine
                    ? podcast with
                    {
                        Bytes = podcast.Bytes + bytes,
                        Draft = (podcast.Draft ?? NewDraft()) with
                        {
                            State = PodcastDraftState.Ready, SourceBytes = bytes, Error = null
                        }
                    }
                    : podcast with { Bytes = podcast.Bytes + bytes, Replay = new(PodcastReplayState.Ready, bytes) };
                await SaveAsync(current with { Manifest = current.Manifest with { Podcast = updated } }, ct);
                _logger.LogInformation("Podcast {PodcastId} {Kind} ready ({Bytes} bytes)", id,
                    refine ? "draft source" : "MP4", bytes);
            }
            finally { _gate.Release(); }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Podcast {PodcastId} MP4 preparation failed", id);
            File.Delete(temporary);
            File.Delete(target);
            await _gate.WaitAsync(CancellationToken.None);
            try
            {
                var current = Find(id);
                if (refine)
                {
                    var draft = current.Manifest.Podcast.Draft ?? NewDraft();
                    if (!ct.IsCancellationRequested)
                        await SetDraftAsync(current, draft with
                        {
                            State = PodcastDraftState.Failed, Error =
                                "Preparation de la source impossible. L'auteur peut relancer la preparation."
                        }, CancellationToken.None);
                    if (ct.IsCancellationRequested) throw;
                    return true;
                }
                var replay = ct.IsCancellationRequested
                    ? new PodcastReplayResponse(PodcastReplayState.Pending)
                    : new PodcastReplayResponse(PodcastReplayState.Failed, Error:
                        "Preparation du MP4 impossible. L'auteur peut relancer la preparation.");
                await SetReplayAsync(current, replay, CancellationToken.None);
            }
            finally { _gate.Release(); }
            if (ct.IsCancellationRequested) throw;
        }
        finally
        {
            await _gate.WaitAsync(CancellationToken.None);
            try { _preparingReplay = null; }
            finally { _gate.Release(); }
        }
        return true;
    }

    private Task SetReplayAsync(Document document, PodcastReplayResponse replay, CancellationToken ct)
        => SaveAsync(document with
        {
            Manifest = document.Manifest with { Podcast = document.Manifest.Podcast with { Replay = replay } }
        }, ct);

    private Task SetDraftAsync(Document document, PodcastDraftResponse draft, CancellationToken ct)
        => SaveAsync(document with
        {
            Manifest = document.Manifest with { Podcast = document.Manifest.Podcast with { Draft = draft } }
        }, ct);

    private static PodcastDraftResponse NewDraft() => new(PodcastDraftState.Preparing, 0, [], []);

    /// <summary>The retention delay runs from the end of the live; with 0 days it starts on publication.</summary>
    private static DateTimeOffset? EndExpiry(PodcastResponse podcast, DateTimeOffset endedAt)
        => podcast.RefineReplay && podcast.RetentionDays > 0 ? endedAt.AddDays(podcast.RetentionDays) : null;

    private static bool IsRemuxQueued(PodcastResponse podcast) => podcast.RefineReplay
        ? podcast.Draft?.State == PodcastDraftState.Preparing
        : podcast.State == PodcastState.Completed && podcast.Replay?.State == PodcastReplayState.Pending;

    private static long SourceDurationMs(PodcastResponse podcast)
        => (long)Math.Ceiling(podcast.DurationSeconds * 1000);

    private Document EditableDraft(Guid id, string ownerId, int revision)
    {
        var document = Owned(id, ownerId);
        var draft = document.Manifest.Podcast.Draft;
        if (document.Manifest.Podcast.OriginalsDeleted)
            throw new PodcastException(409, "Les originaux ont ete supprimes : ce replay ne peut plus etre reedite.");
        if (!document.Manifest.Podcast.RefineReplay || draft is null)
            throw new PodcastException(409, "Ce replay n'est pas en cours de raffinage.");
        if (draft.State != PodcastDraftState.Ready)
            throw new PodcastException(409, "Le brouillon n'est pas modifiable pendant la preparation ou la publication.");
        if (draft.Revision != revision)
            throw new PodcastException(409, "Le brouillon a ete modifie ailleurs. Rechargez-le avant de continuer.");
        return document;
    }

    public async Task<PodcastResponse> SaveDraftAsync(Guid id, string ownerId, SavePodcastDraftRequest request, CancellationToken ct)
    {
        if (request.Marks.Count > MaxDraftMarks)
            throw new PodcastException(400, $"Au plus {MaxDraftMarks} marques sont autorisees.");
        await _gate.WaitAsync(ct);
        try
        {
            await LoadAsync(ct);
            var document = EditableDraft(id, ownerId, request.Revision);
            var podcast = document.Manifest.Podcast;
            var duration = SourceDurationMs(podcast);
            var removed = request.RemovedRanges.Select(r => new ReplayRange(r.StartMs, r.EndMs)).ToArray();
            try { _ = ReplayEditPlan.KeptRanges(removed, duration); }
            catch (ArgumentException ex) { throw new PodcastException(400, ex.Message.Split(" (Parameter")[0]); }
            if (request.Marks.Any(m => m < 0 || m > duration))
                throw new PodcastException(400, "Une marque est en dehors de la video.");
            // A mark inside a removed passage disappears with it.
            var marks = request.Marks.Distinct().Order()
                .Where(m => !removed.Any(r => m >= r.StartMs && m < r.EndMs)).ToArray();
            var ranges = request.RemovedRanges.OrderBy(r => r.StartMs).ThenBy(r => r.EndMs).ToArray();
            var draft = podcast.Draft! with
            {
                Revision = podcast.Draft!.Revision + 1, Marks = marks, RemovedRanges = ranges, Error = null
            };
            await SetDraftAsync(document, draft, ct);
            return _documents[id].Manifest.Podcast;
        }
        finally { _gate.Release(); }
    }

    public async Task<PodcastResponse> PublishDraftAsync(Guid id, string ownerId, int revision, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            await LoadAsync(ct);
            var current = Owned(id, ownerId).Manifest.Podcast;
            if (current.Draft is { State: PodcastDraftState.Publishing } publishing && publishing.Revision == revision)
                return current;
            var document = EditableDraft(id, ownerId, revision);
            var podcast = document.Manifest.Podcast;
            if (podcast.Draft!.PublishedRevision == revision && podcast.Replay?.State == PodcastReplayState.Ready)
                return podcast;
            await SetDraftAsync(document, podcast.Draft with { State = PodcastDraftState.Publishing, Error = null }, ct);
            _logger.LogInformation("Podcast {PodcastId} draft revision {Revision} queued for publication", id, revision);
            return _documents[id].Manifest.Podcast;
        }
        finally { _gate.Release(); }
    }

    public async Task<Stream> OpenDraftSourceAsync(Guid id, string ownerId, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            await LoadAsync(ct);
            var draft = Owned(id, ownerId).Manifest.Podcast.Draft;
            if (draft is null || draft.SourceBytes <= 0)
                throw new PodcastException(409, "La source du brouillon n'est pas disponible.");
            return new FileStream(Path.Combine(DirectoryFor(id), SourceFileName), FileMode.Open, FileAccess.Read,
                FileShare.Read | FileShare.Delete, 64 * 1024, FileOptions.Asynchronous | FileOptions.RandomAccess);
        }
        finally { _gate.Release(); }
    }

    public async Task<bool> PublishNextDraftAsync(IPodcastReplayEditor editor, CancellationToken ct)
    {
        await _replays.WaitAsync(ct);
        try { return await PublishDraftAsync(editor, ct); }
        finally { _replays.Release(); }
    }

    private async Task<bool> PublishDraftAsync(IPodcastReplayEditor editor, CancellationToken ct)
    {
        PodcastResponse podcast;
        long allowance;
        await _gate.WaitAsync(ct);
        try
        {
            await LoadAsync(ct);
            var next = _documents.Values.Where(d => d.Manifest.Podcast.Draft?.State == PodcastDraftState.Publishing)
                .OrderBy(d => d.Manifest.Podcast.CreatedAt).FirstOrDefault();
            if (next is null) return false;
            podcast = next.Manifest.Podcast;
            // The published MP4 is replaced, so its bytes are available for the new version.
            allowance = _options.MaxOwnerBytes - OwnerBytes(podcast.OwnerId) + PublishedBytes(podcast);
            _preparingReplay = podcast.Id;
        }
        finally { _gate.Release(); }

        var id = podcast.Id;
        var draft = podcast.Draft!;
        var directory = DirectoryFor(id);
        var source = Path.Combine(directory, SourceFileName);
        var temporary = Path.Combine(directory, "replay.tmp.mp4");
        try
        {
            if (allowance <= 1) throw new IOException("Quota de stockage des podcasts atteint.");
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(_options.ReplayTimeoutSeconds));
            if (draft.RemovedRanges.Count == 0)
            {
                if (draft.SourceBytes > allowance) throw new IOException("Quota de stockage des podcasts atteint.");
                await using var input = File.OpenRead(source);
                await using var output = File.Create(temporary);
                await input.CopyToAsync(output, timeout.Token);
            }
            else
            {
                var duration = SourceDurationMs(podcast);
                var kept = ReplayEditPlan.KeptRanges(
                    draft.RemovedRanges.Select(r => new ReplayRange(r.StartMs, r.EndMs)), duration).ToArray();
                // The declared segment durations may be slightly shorter than the media: keep its tail.
                if (kept[^1].EndMs == duration) kept[^1] = kept[^1] with { EndMs = ReplayRange.OpenEnd };
                await editor.RenderAsync(source, kept, temporary, allowance, timeout.Token);
            }
            var bytes = new FileInfo(temporary).Length;
            if (bytes <= 0 || bytes > allowance) throw new IOException("Taille du MP4 invalide ou quota atteint.");
            await _gate.WaitAsync(ct);
            try
            {
                var current = Find(id);
                var latest = current.Manifest.Podcast;
                var previous = PublishedBytes(latest);
                if (OwnerBytes(latest.OwnerId) - previous + bytes > _options.MaxOwnerBytes)
                    throw new IOException("Quota de stockage des podcasts atteint.");
                // Readers of the previous version keep their open file; new requests get the new one.
                File.Move(temporary, Path.Combine(directory, ReplayFileName), overwrite: true);
                var updated = latest with
                {
                    Bytes = latest.Bytes - previous + bytes,
                    Replay = new(PodcastReplayState.Ready, bytes),
                    Draft = latest.Draft! with
                    {
                        State = PodcastDraftState.Ready, PublishedRevision = draft.Revision, Error = null
                    },
                    OriginalsExpireAt = latest.OriginalsExpireAt
                        ?? (latest.RetentionDays == 0 ? _clock.GetUtcNow() : null)
                };
                await SaveAsync(current with { Manifest = current.Manifest with { Podcast = updated } }, ct);
                _logger.LogInformation("Podcast {PodcastId} draft revision {Revision} published ({Bytes} bytes)",
                    id, draft.Revision, bytes);
            }
            finally { _gate.Release(); }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Podcast {PodcastId} draft publication failed", id);
            File.Delete(temporary);
            if (!ct.IsCancellationRequested)
            {
                await _gate.WaitAsync(CancellationToken.None);
                try
                {
                    var current = Find(id);
                    await SetDraftAsync(current, current.Manifest.Podcast.Draft! with
                    {
                        State = PodcastDraftState.Ready, Error =
                            "Publication impossible. Le brouillon est conserve ; corrigez-le ou relancez la publication."
                    }, CancellationToken.None);
                }
                finally { _gate.Release(); }
            }
            if (ct.IsCancellationRequested) throw;
        }
        finally
        {
            await _gate.WaitAsync(CancellationToken.None);
            try { _preparingReplay = null; }
            finally { _gate.Release(); }
        }
        return true;
    }

    private static long PublishedBytes(PodcastResponse podcast)
        => podcast.Replay?.State == PodcastReplayState.Ready ? podcast.Replay.Bytes : 0;

    public async Task<Stream> OpenSegmentAsync(Guid id, int sequence, string? viewerId, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            await LoadAsync(ct);
            var document = Find(id);
            PodcastVisibility.EnsureOriginalsVisible(document.Manifest.Podcast, viewerId);
            if (sequence < 0 || sequence >= document.Manifest.Segments.Count)
                throw new PodcastException(404, "Segment introuvable.");
            return new FileStream(SegmentPath(id, sequence), FileMode.Open, FileAccess.Read,
                FileShare.Read | FileShare.Delete, 64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        }
        finally { _gate.Release(); }
    }

    public async Task DeleteAsync(Guid id, string ownerId, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            await LoadAsync(ct);
            var document = Owned(id, ownerId);
            if (document.Manifest.Podcast.State == PodcastState.Live)
                throw new PodcastException(409, "Terminez le direct avant de le supprimer.");
            if (_preparingReplay == id)
                throw new PodcastException(409, "Preparation du MP4 en cours. Reessayez apres sa finalisation.");
            Directory.Delete(DirectoryFor(id), recursive: true);
            _documents.Remove(id);
            _logger.LogInformation("Podcast {PodcastId} deleted by {OwnerId}", id, ownerId);
        }
        finally { _gate.Release(); }
    }

    public async Task<PodcastResponse> DeleteOriginalsAsync(Guid id, string ownerId, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            await LoadAsync(ct);
            var document = Owned(id, ownerId);
            var podcast = document.Manifest.Podcast;
            if (podcast.OriginalsDeleted) return podcast;
            if (!podcast.RefineReplay || PublishedBytes(podcast) == 0)
                throw new PodcastException(409, "Les originaux ne peuvent etre supprimes qu'apres la publication du replay.");
            if (!CanPurge(document))
                throw new PodcastException(409, "Publication en cours. Reessayez apres sa finalisation.");
            return await PurgeOriginalsAsync(document, ct);
        }
        finally { _gate.Release(); }
    }

    public async Task<IReadOnlyList<PodcastResponse>> PurgeExpiredOriginalsAsync(CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            await LoadAsync(ct);
            var now = _clock.GetUtcNow();
            var removed = new List<PodcastResponse>();
            foreach (var document in _documents.Values.Where(d => d.Manifest.Podcast is
                { RefineReplay: true, OriginalsDeleted: false, OriginalsExpireAt: { } expiry } && expiry <= now
                && CanPurge(d)).ToArray())
            {
                var podcast = document.Manifest.Podcast;
                if (PublishedBytes(podcast) > 0)
                {
                    await PurgeOriginalsAsync(document, ct);
                    continue;
                }
                // Nothing was published before the deadline: the whole emission expires.
                Directory.Delete(DirectoryFor(podcast.Id), recursive: true);
                _documents.Remove(podcast.Id);
                removed.Add(podcast);
                _logger.LogInformation("Unpublished refined podcast {PodcastId} expired and was deleted", podcast.Id);
            }
            return removed;
        }
        finally { _gate.Release(); }
    }

    private bool CanPurge(Document document) => _preparingReplay != document.Manifest.Podcast.Id
        && document.Manifest.Podcast.Draft?.State is not (PodcastDraftState.Preparing or PodcastDraftState.Publishing);

    private async Task<PodcastResponse> PurgeOriginalsAsync(Document document, CancellationToken ct)
    {
        var podcast = document.Manifest.Podcast;
        var directory = DirectoryFor(podcast.Id);
        foreach (var segment in document.Manifest.Segments) File.Delete(SegmentPath(podcast.Id, segment.Sequence));
        File.Delete(Path.Combine(directory, SourceFileName));
        File.Delete(Path.Combine(directory, "source.tmp.mp4"));
        var updated = podcast with
        {
            SegmentCount = 0, Bytes = PublishedBytes(podcast), Draft = null,
            OriginalsExpireAt = null, OriginalsDeleted = true
        };
        await SaveAsync(new(new(updated, []), []), ct);
        _logger.LogInformation("Podcast {PodcastId} originals and draft deleted; the published MP4 remains", podcast.Id);
        return updated;
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        if (_loaded) return;
        Directory.CreateDirectory(_options.RootPath);
        foreach (var directory in Directory.EnumerateDirectories(_options.RootPath))
        {
            if (!Guid.TryParseExact(Path.GetFileName(directory), "N", out var id)) continue;
            var path = Path.Combine(directory, "index.json");
            if (!File.Exists(path)) continue; // An uncommitted create may leave an empty directory.
            await using var file = File.OpenRead(path);
            var document = await JsonSerializer.DeserializeAsync<Document>(file, cancellationToken: ct)
                ?? throw new InvalidDataException($"Invalid podcast index {id}.");
            if (document.Manifest.Podcast.Id != id || document.Hashes.Length != document.Manifest.Segments.Count)
                throw new InvalidDataException($"Inconsistent podcast index {id}.");
            _documents[id] = document;
        }
        // A restarted process cannot know whether its old encoder is still running.
        foreach (var document in _documents.Values.Where(d => d.Manifest.Podcast.State == PodcastState.Live).ToArray())
            await InterruptAsync(document, ct);
        foreach (var document in _documents.Values.Where(d => d.Manifest.Podcast.State == PodcastState.Completed
            && (d.Manifest.Podcast.Replay is null || d.Manifest.Podcast.Replay.State == PodcastReplayState.Processing)).ToArray())
            await SetReplayAsync(document, new(PodcastReplayState.Pending), ct);
        _loaded = true;
    }

    private async Task ExpireAsync(CancellationToken ct)
    {
        var deadline = _clock.GetUtcNow().AddSeconds(-_options.IdleTimeoutSeconds);
        foreach (var document in _documents.Values.Where(d =>
            d.Manifest.Podcast.State == PodcastState.Live && d.Manifest.Podcast.UpdatedAt < deadline).ToArray())
            await InterruptAsync(document, ct);
    }

    private async Task InterruptAsync(Document document, CancellationToken ct)
    {
        var podcast = document.Manifest.Podcast with { State = PodcastState.Interrupted, UpdatedAt = _clock.GetUtcNow() };
        // A refined emission stays recoverable by its owner: prepare its private draft as after a normal end.
        if (podcast.RefineReplay)
            podcast = podcast with
            {
                Replay = podcast.Replay ?? new(PodcastReplayState.Pending), Draft = podcast.Draft ?? NewDraft(),
                OriginalsExpireAt = podcast.OriginalsExpireAt ?? EndExpiry(podcast, podcast.UpdatedAt)
            };
        await SaveAsync(document with { Manifest = document.Manifest with { Podcast = podcast } }, ct);
        _logger.LogWarning("Podcast {PodcastId} interrupted; committed segments remain playable", podcast.Id);
    }

    private async Task SaveAsync(Document document, CancellationToken ct)
    {
        var path = Path.Combine(DirectoryFor(document.Manifest.Podcast.Id), "index.json");
        await using (var file = new FileStream(path + ".tmp", FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await JsonSerializer.SerializeAsync(file, document, cancellationToken: ct);
            await file.FlushAsync(ct);
        }
        File.Move(path + ".tmp", path, true);
        _documents[document.Manifest.Podcast.Id] = document;
    }

    private Document Find(Guid id) => _documents.TryGetValue(id, out var document)
        ? document : throw new PodcastException(404, "Podcast introuvable.");

    private Document Owned(Guid id, string ownerId)
    {
        var document = Find(id);
        if (document.Manifest.Podcast.OwnerId != ownerId)
            throw new PodcastException(403, "Seul l'auteur peut modifier ce podcast.");
        return document;
    }

    private long OwnerBytes(string ownerId) => _documents.Values
        .Where(d => d.Manifest.Podcast.OwnerId == ownerId).Sum(d => d.Manifest.Podcast.Bytes);
    private string DirectoryFor(Guid id) => Path.Combine(_options.RootPath, id.ToString("N"));
    private string SegmentPath(Guid id, int sequence) => Path.Combine(DirectoryFor(id), $"{sequence:D8}.ts");

    private static void ValidateTransportStream(byte[] bytes)
    {
        if (bytes.Length == 0 || bytes.Length % 188 != 0)
            throw new PodcastException(400, "Un segment MPEG-TS complet est requis.");
        for (var i = 0; i < bytes.Length; i += 188)
            if (bytes[i] != 0x47)
                throw new PodcastException(400, "Synchronisation MPEG-TS invalide.");
    }

    public async Task<bool> AcceptStream(HttpContext context, ApplicationUser user, string destDir, string fileName)
    {
        var userName = user.UserName ?? throw new InvalidOperationException("A stream owner must have a user name.");
        var handler = new LiveCastHandler();
        if (!Casters.TryAdd(userName, handler))
        {
            handler.Dispose();
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            return false;
        }
        try
        {
            handler.Socket = await context.WebSockets.AcceptWebSocketAsync();
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(
                context.RequestAborted, handler.TokenSource.Token);
            var ct = cancellation.Token;
            var path = Path.Combine(destDir, Path.GetFileName(fileName));
            var oldLength = File.Exists(path) ? new FileInfo(path).Length : 0;
            var usage = user.DiskUsage - oldLength;
            if (usage >= user.GetEffectiveDiskQuota(_clock.GetUtcNow()))
            {
                await handler.Socket.CloseOutputAsync(WebSocketCloseStatus.PolicyViolation, "Available quota required.", ct);
                return false;
            }
            await using var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
            var buffer = new byte[Constants.WebSocketsMaxBufLen];
            while (handler.Socket.State == WebSocketState.Open)
            {
                var received = await handler.Socket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                if (received.MessageType == WebSocketMessageType.Close) break;
                if (received.MessageType != WebSocketMessageType.Binary || usage + received.Count > user.GetEffectiveDiskQuota(_clock.GetUtcNow()))
                {
                    await handler.Socket.CloseOutputAsync(WebSocketCloseStatus.PolicyViolation, "Binary data and available quota required.", ct);
                    return false;
                }
                await file.WriteAsync(buffer.AsMemory(0, received.Count), ct);
                usage += received.Count;
                user.DiskUsage = usage;
                foreach (var listener in handler.Listeners.Values)
                    if (listener.State == WebSocketState.Open)
                        await listener.SendAsync(new ArraySegment<byte>(buffer, 0, received.Count),
                            WebSocketMessageType.Binary, received.EndOfMessage, ct);
            }
            if (handler.Socket.State == WebSocketState.CloseReceived)
                await handler.Socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Stream complete.", ct);
            return true;
        }
        catch (Exception ex) when (ex is IOException or WebSocketException or OperationCanceledException)
        {
            _logger.LogWarning(ex, "Legacy stream interrupted for {UserName}", user.UserName);
            throw;
        }
        finally
        {
            Casters.TryRemove(userName, out _);
            handler.Dispose();
        }
    }

    public void Dispose()
    {
        foreach (var handler in Casters.Values) handler.Dispose();
        _gate.Dispose();
        _uploads.Dispose();
        _replays.Dispose();
    }
}
