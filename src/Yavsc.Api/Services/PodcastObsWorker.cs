#nullable enable
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using FFmpeg.NET;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Yavsc.Models;
using Yavsc.Server.Helpers;
using Yavsc.Services;
using Yavsc.Settings;

namespace Yavsc.Api.Services;

public sealed class PodcastObsWorker(IServiceScopeFactory scopes, IOptions<PodcastObsOptions> options,
    IOptions<PodcastOptions> mediaOptions, TimeProvider clock, ILogger<PodcastObsWorker> logger) : BackgroundService
{
    private sealed class Capture(Guid id, string ownerId, Guid connectionId, int port,
        DateTimeOffset expires, CancellationTokenSource cancellation)
    {
        public Guid Id { get; } = id;
        public string OwnerId { get; } = ownerId;
        public Guid ConnectionId { get; } = connectionId;
        public int Port { get; } = port;
        public DateTimeOffset Expires { get; } = expires;
        public CancellationTokenSource Cancellation { get; } = cancellation;
        public TaskCompletionSource Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public DateTimeOffset? Started { get; set; }
        public string? Error { get; set; }
    }

    internal SemaphoreSlim Lifecycle { get; } = new(1, 1);
    private readonly object gate = new();
    private readonly Dictionary<Guid, Capture> captures = new();
    private readonly TaskCompletionSource ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private CancellationToken stopping;

    internal Task WaitUntilReadyAsync(CancellationToken ct) => ready.Task.WaitAsync(ct);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stopping = stoppingToken;
        try
        {
            options.Value.Validate();
            using (var scope = scopes.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var stale = await db.LiveFlow.Where(f => f.ObsKeyHash != null).ToListAsync(stoppingToken);
                foreach (var flow in stale)
                    await ResourceUsageTracker.WithUserLockAsync(db, flow.OwnerId, async () =>
                    {
                        flow.ObsKeyHash = null;
                        flow.ObsConnectionId = null;
                        flow.ObsEnabledUntil = null;
                        flow.ObsLastError = "L'API a redemarre ; reactivez OBS pour obtenir une nouvelle URL SRT.";
                        await db.SaveChangesAsync(stoppingToken);
                        return true;
                    }, stoppingToken);
            }
            ready.TrySetResult();
            if (!options.Value.Enabled) return;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = scopes.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    Capture[] active;
                    lock (gate) active = captures.Values.ToArray();
                    foreach (var capture in active)
                    {
                        var flow = await db.LiveFlow.AsNoTracking().Include(f => f.Owner)
                            .SingleOrDefaultAsync(f => f.PodcastId == capture.Id, stoppingToken);
                        lock (gate)
                        {
                            if (!captures.ContainsKey(capture.Id)) continue;
                            if (capture.Completion.Task.IsCompleted)
                            {
                                captures.Remove(capture.Id);
                                capture.Cancellation.Dispose();
                            }
                            else if (flow is null || flow.PodcastDeleted || flow.ObsConnectionId != capture.ConnectionId
                                || flow.ObsKeyHash is null || flow.Owner.WithdrawalRequestedAt is not null)
                                capture.Cancellation.Cancel();
                        }
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
                catch (Exception ex) { logger.LogError(ex, "OBS listener state check failed; retrying"); }
                await Task.Delay(250, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        catch (Exception ex) { ready.TrySetException(ex); throw; }
        finally
        {
            Capture[] active;
            lock (gate)
            {
                active = captures.Values.ToArray();
                foreach (var capture in active) capture.Cancellation.Cancel();
            }
            await Task.WhenAll(active.Select(c => c.Completion.Task));
            lock (gate)
            {
                foreach (var capture in captures.Values) capture.Cancellation.Dispose();
                captures.Clear();
            }
        }
    }

    internal async Task<int> ArmAsync(Guid id, string ownerId, Guid connectionId, string key,
        DateTimeOffset expires, CancellationToken ct)
    {
        stopping.ThrowIfCancellationRequested();
        Capture capture;
        lock (gate)
        {
            foreach (var ended in captures.Values.Where(c => c.Completion.Task.IsCompleted).ToArray())
            {
                captures.Remove(ended.Id);
                ended.Cancellation.Dispose();
            }
            if (captures.Count >= options.Value.MaxConcurrentBroadcasts)
                throw new PodcastException(503, "Capacite OBS atteinte ; revoquez une activation inutilisee ou reessayez plus tard.");
            var port = Enumerable.Range(options.Value.FirstPort, options.Value.PortCount)
                .FirstOrDefault(p => captures.Values.All(c => c.Port != p) && !PortBound(p));
            if (port == 0) throw new PodcastException(503, "Aucun port UDP OBS disponible.");
            capture = new(id, ownerId, connectionId, port, expires,
                CancellationTokenSource.CreateLinkedTokenSource(stopping));
            captures.Add(id, capture);
        }
        _ = CaptureAsync(capture, key);
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct, stopping);
            timeout.CancelAfter(TimeSpan.FromSeconds(10));
            while (!PortBound(capture.Port))
            {
                if (capture.Error is not null || capture.Completion.Task.IsCompleted)
                    throw new PodcastException(503, capture.Error ?? "L'ecoute SRT n'a pas demarre.");
                await Task.Delay(50, timeout.Token);
            }
            return capture.Port;
        }
        catch
        {
            // Do not wait here: activation owns the accounting lock needed by capture cleanup.
            lock (gate) capture.Cancellation.Cancel();
            throw;
        }
    }

    internal async Task StopCaptureAsync(Guid id, CancellationToken ct)
    {
        Capture? capture;
        lock (gate)
        {
            if (!captures.TryGetValue(id, out capture)) return;
            capture.Cancellation.Cancel();
        }
        await capture.Completion.Task.WaitAsync(ct);
        if (PortBound(capture.Port))
            throw new PodcastException(503, "L'ecoute SRT ne s'est pas arretee ; intervention de l'administration requise.");
        lock (gate)
            if (captures.Remove(id)) capture.Cancellation.Dispose();
    }

    private bool PortBound(int port)
    {
        var address = IPAddress.Parse(options.Value.ListenAddress);
        using var socket = new Socket(address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        try { socket.Bind(new IPEndPoint(address, port)); return false; }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse) { return true; }
    }

    private async Task CaptureAsync(Capture capture, string key)
    {
        var media = mediaOptions.Value;
        var directory = Path.Combine(media.RootPath, $"obs-capture-{capture.Id:N}-{capture.ConnectionId:N}");
        string? error = null;
        try
        {
            Directory.CreateDirectory(directory);
            using var scope = scopes.CreateScope();
            using var receive = CancellationTokenSource.CreateLinkedTokenSource(capture.Cancellation.Token);
            var guard = GuardBufferAsync(directory, media.MaxSegmentBytes, receive.Token);
            var ingest = ReceiveAsync(capture, key, directory, scope.ServiceProvider, receive.Token);
            try
            {
                if (await Task.WhenAny(ingest, guard) == guard) await guard;
                await ingest;
            }
            finally
            {
                receive.Cancel();
                try { await ingest; }
                catch (OperationCanceledException) when (receive.IsCancellationRequested) { }
                try { await guard; }
                catch (OperationCanceledException) when (receive.IsCancellationRequested) { }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("OBS capture {PodcastId} stopped", capture.Id);
        }
        catch (Exception ex)
        {
            error = ex is PodcastException or ResourceUsageLimitException ? ex.Message
                : "Reception SRT impossible. Verifiez FFmpeg, la connexion et l'encodage H.264/AAC.";
            logger.LogError("OBS capture {PodcastId} failed ({ErrorType})", capture.Id, ex.GetType().Name);
        }
        finally
        {
            capture.Error = error;
            try
            {
                using var scope = scopes.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                await ResourceUsageTracker.WithUserLockAsync(db, capture.OwnerId, async () =>
                {
                    var flow = await db.LiveFlow.Include(f => f.Owner)
                        .SingleOrDefaultAsync(f => f.PodcastId == capture.Id, timeout.Token);
                    if (flow is not null)
                    {
                        if (capture.Started is not null && flow.Owner.WithdrawalRequestedAt is null)
                        {
                            try
                            {
                                await scope.ServiceProvider.GetRequiredService<ILiveProcessor>()
                                    .CompleteAsync(capture.Id, capture.OwnerId, timeout.Token);
                            }
                            catch (Exception ex)
                            {
                                error = "Finalisation OBS impossible ; intervention de l'administration requise.";
                                capture.Error = error;
                                logger.LogError("OBS replay finalization failed for {PodcastId} ({ErrorType})",
                                    capture.Id, ex.GetType().Name);
                            }
                        }
                        if (flow.ObsConnectionId == capture.ConnectionId)
                        {
                            flow.ObsKeyHash = null;
                            flow.ObsEnabledUntil = null;
                            flow.ObsConnectionId = null;
                            flow.ObsLastError = error;
                            await db.SaveChangesAsync(timeout.Token);
                        }
                    }
                    return true;
                }, timeout.Token);
            }
            catch (Exception ex)
            {
                logger.LogError("OBS capture {PodcastId} finalization failed ({ErrorType}); operator intervention required",
                    capture.Id, ex.GetType().Name);
                capture.Error = "Finalisation OBS impossible ; intervention de l'administration requise.";
            }
            try { if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true); }
            catch (Exception ex)
            {
                logger.LogError("OBS temporary files cleanup failed for {PodcastId} ({ErrorType})",
                    capture.Id, ex.GetType().Name);
            }
            capture.Completion.TrySetResult();
        }
    }

    private async Task ReceiveAsync(Capture capture, string key, string directory, IServiceProvider services, CancellationToken ct)
    {
        var input = new UriBuilder("srt", options.Value.ListenAddress, capture.Port)
        {
            Query = $"mode=listener&passphrase={key}&pbkeylen=32&enforced_encryption=1"
                + $"&listen_timeout={options.Value.ActivationMinutes * 60000000L}&timeout=20000000&latency=200000"
        };
        var engine = new Engine(mediaOptions.Value.FfmpegPath);
        var failed = false;
        engine.Error += (_, _) => Volatile.Write(ref failed, true);
        using var processCancellation = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var process = engine.ExecuteAsync(
            $"-nostdin -hide_banner -loglevel error -i \"{input.Uri.AbsoluteUri}\" -map 0:v:0 -map 0:a:0? -c copy"
            + " -f segment -segment_format mpegts -segment_time 2 -segment_list segments.csv"
            + " -segment_list_type csv segment-%08d.ts", directory, processCancellation.Token);
        var db = services.GetRequiredService<ApplicationDbContext>();
        var processor = services.GetRequiredService<ILiveProcessor>();
        var next = 0;
        var total = 0L;
        var lastSegment = clock.GetUtcNow();
        try
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                var now = clock.GetUtcNow();
                if (capture.Started is null && now >= capture.Expires)
                    throw new PodcastException(408, "Activation OBS expiree ; obtenez une nouvelle URL SRT.");
                if (capture.Started is not null)
                {
                    if (now - capture.Started >= TimeSpan.FromMinutes(options.Value.MaxDurationMinutes))
                        break;
                    if (now - lastSegment > TimeSpan.FromSeconds(20))
                        throw new PodcastException(408, "Aucun segment OBS recu depuis vingt secondes ; verifiez la connexion.");
                }
                var playlist = Path.Combine(directory, "segments.csv");
                var text = File.Exists(playlist) ? await File.ReadAllTextAsync(playlist, ct) : "";
                var lines = text.Split('\n');
                for (; next < lines.Length - 1; next++)
                {
                    if (next >= mediaOptions.Value.MaxSegments) throw new PodcastException(413, "Nombre maximal de segments atteint.");
                    var fields = lines[next].TrimEnd('\r').Split(',');
                    if (fields.Length != 3 || fields[0] != $"segment-{next:D8}.ts"
                        || !double.TryParse(fields[1], CultureInfo.InvariantCulture, out var begin)
                        || !double.TryParse(fields[2], CultureInfo.InvariantCulture, out var end)
                        || !double.IsFinite(end - begin) || end - begin <= 0 || end - begin > 15)
                        throw new PodcastException(400, "Segments OBS invalides ; configurez H.264/AAC et des images cles toutes les deux secondes.");
                    if (capture.Started is null)
                    {
                        var podcast = await services.GetRequiredService<IPodcastScheduleService>()
                            .StartAsync(capture.Id, capture.OwnerId, ct, capture.ConnectionId);
                        capture.Started = now;
                        processCancellation.CancelAfter(TimeSpan.FromMinutes(
                            Math.Min(podcast.PlannedDurationMinutes ?? 1, options.Value.MaxDurationMinutes)));
                    }
                    var path = Path.Combine(directory, fields[0]);
                    var bytes = new FileInfo(path).Length;
                    if (bytes == 0 || bytes > mediaOptions.Value.MaxSegmentBytes)
                        throw new PodcastException(413, "Segment OBS trop volumineux ; reduisez le debit.");
                    total = checked(total + bytes);
                    await ResourceUsageTracker.RecordStorageAsync(db, capture.OwnerId, capture.OwnerId,
                        $"podcast/{capture.Id:N}", total, ct, write: async () =>
                        {
                            var user = await db.Users.SingleAsync(u => u.Id == capture.OwnerId, ct);
                            await db.Entry(user).ReloadAsync(ct);
                            var flow = await db.LiveFlow.SingleAsync(f => f.PodcastId == capture.Id, ct);
                            await db.Entry(flow).ReloadAsync(ct);
                            if (user.WithdrawalRequestedAt is not null || flow.ObsKeyHash is null
                                || flow.ObsConnectionId != capture.ConnectionId || flow.PodcastDeleted)
                                throw new OperationCanceledException("OBS activation revoked or account closed.", ct);
                            if (bytes > user.GetEffectiveDiskQuota(now) - user.DiskUsage)
                                throw new PodcastException(413, "Quota disque atteint.");
                            await using var stream = File.OpenRead(path);
                            await processor.AppendAsync(capture.Id, capture.OwnerId, next, end - begin, stream, ct);
                            user.DiskUsage = checked(user.DiskUsage + bytes);
                        }, policy: services.GetRequiredService<IOptions<ResourceUsageOptions>>().Value,
                        bandwidthMb: (decimal)bytes / (1024m * 1024m));
                    File.Delete(path);
                    lastSegment = now;
                }
                if (process.IsCompleted)
                {
                    await process;
                    if (Volatile.Read(ref failed) || next == 0)
                        throw new IOException("FFmpeg SRT listener failed or produced no media.");
                    break;
                }
                await Task.Delay(100, ct);
            }
        }
        finally
        {
            processCancellation.Cancel();
            try { await process; }
            catch (OperationCanceledException) when (processCancellation.IsCancellationRequested) { }
            // FFmpeg.NET cancels/kills without waiting for OS socket teardown.
            using var released = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try
            {
                while (PortBound(capture.Port))
                    await Task.Delay(25, released.Token);
            }
            catch (OperationCanceledException) when (released.IsCancellationRequested)
            {
                throw new IOException("SRT listener did not release its UDP port after cancellation.");
            }
        }
    }

    private static async Task GuardBufferAsync(string directory, int maxSegmentBytes, CancellationToken ct)
    {
        while (true)
        {
            var pending = 0L;
            foreach (var path in Directory.EnumerateFiles(directory, "*.ts"))
            {
                var file = new FileInfo(path);
                if (!file.Exists) continue;
                var bytes = file.Length;
                if (bytes > maxSegmentBytes || (pending += bytes) > 64L * 1024 * 1024)
                    throw new PodcastException(413, "Capture OBS trop volumineuse ou stockage trop lent ; reduisez le debit.");
            }
            await Task.Delay(100, ct);
        }
    }
}
