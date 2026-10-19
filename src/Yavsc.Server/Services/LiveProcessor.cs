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
    private readonly ILogger<LiveProcessor> _logger;
    private readonly PodcastOptions _options;
    private readonly TimeProvider _clock;
    private readonly SemaphoreSlim _gate = new(1);
    private readonly SemaphoreSlim _uploads = new(2);
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
            || _options.IdleTimeoutSeconds < 15 || _options.MaxOwnerPodcasts < 1)
            throw new InvalidOperationException("Invalid Podcast storage options.");
    }

    public async Task<PodcastResponse> CreateAsync(string ownerId, string ownerName, string title, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(ownerId) || string.IsNullOrWhiteSpace(title) || title.Trim().Length > 160)
            throw new PodcastException(400, "Un titre de 1 a 160 caracteres et un utilisateur sont requis.");
        await _gate.WaitAsync(ct);
        try
        {
            await LoadAsync(ct);
            await ExpireAsync(ct);
            if (_documents.Values.Any(d => d.Manifest.Podcast.OwnerId == ownerId && d.Manifest.Podcast.State == PodcastState.Live))
                throw new PodcastException(409, "Vous avez deja un direct en cours.");
            if (OwnerBytes(ownerId) >= _options.MaxOwnerBytes)
                throw new PodcastException(413, "Quota de stockage des podcasts atteint.");
            if (_documents.Values.Count(d => d.Manifest.Podcast.OwnerId == ownerId) >= _options.MaxOwnerPodcasts)
                throw new PodcastException(413, "Nombre maximal de podcasts atteint. Supprimez un ancien replay.");
            var now = _clock.GetUtcNow();
            var podcast = new PodcastResponse(Guid.NewGuid(), title.Trim(), ownerId, ownerName,
                now, now, PodcastState.Live, 0, 0, 0);
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
            var updated = podcast with { State = PodcastState.Completed, UpdatedAt = _clock.GetUtcNow() };
            await SaveAsync(document with { Manifest = document.Manifest with { Podcast = updated } }, ct);
            _logger.LogInformation("Podcast {PodcastId} completed with {Segments} segments", id, updated.SegmentCount);
            return updated;
        }
        finally { _gate.Release(); }
    }

    public async Task<Stream> OpenSegmentAsync(Guid id, int sequence, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            await LoadAsync(ct);
            var document = Find(id);
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
            Directory.Delete(DirectoryFor(id), recursive: true);
            _documents.Remove(id);
            _logger.LogInformation("Podcast {PodcastId} deleted by {OwnerId}", id, ownerId);
        }
        finally { _gate.Release(); }
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
            await using var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
            var buffer = new byte[Constants.WebSocketsMaxBufLen];
            while (handler.Socket.State == WebSocketState.Open)
            {
                var received = await handler.Socket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                if (received.MessageType == WebSocketMessageType.Close) break;
                if (received.MessageType != WebSocketMessageType.Binary || usage + received.Count > user.DiskQuota)
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
    }
}
