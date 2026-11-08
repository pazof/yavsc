#nullable enable
using System.Diagnostics;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Yavsc.Models;
using Yavsc.Server.Helpers;
using Yavsc.Services;
using Yavsc.Settings;

namespace Yavsc.Api.Services;

public sealed class PodcastObsWorker(IServiceScopeFactory scopes, IOptions<PodcastObsOptions> options,
    ILogger<PodcastObsWorker> logger) : BackgroundService
{
    private sealed record Capture(CancellationTokenSource Cancellation, Task Task);
    private readonly Dictionary<Guid, Capture> captures = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled) return;
        options.Value.Validate();
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = scopes.CreateScope();
                    var relay = scope.ServiceProvider.GetRequiredService<IPodcastObsRelay>();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    foreach (var pair in captures.Where(c => c.Value.Task.IsCompleted).ToArray())
                    {
                        captures.Remove(pair.Key);
                        pair.Value.Cancellation.Dispose();
                        await pair.Value.Task;
                    }
                    foreach (var publisher in await relay.PublishersAsync(stoppingToken))
                    {
                        var flow = await db.LiveFlow.AsNoTracking().Include(f => f.Owner)
                            .SingleOrDefaultAsync(f => f.PodcastId == publisher.PodcastId, stoppingToken);
                        var allowed = flow is { PodcastDeleted: false, ObsKeyHash: not null }
                            && flow.ObsConnectionId == publisher.ConnectionId
                            && flow.Owner.WithdrawalRequestedAt is null;
                        if (!allowed)
                        {
                            if (captures.TryGetValue(publisher.PodcastId, out var running)) running.Cancellation.Cancel();
                            await relay.KickAsync(publisher.PodcastId, stoppingToken, publisher.ConnectionId);
                            continue;
                        }
                        if (captures.ContainsKey(publisher.PodcastId)) continue;
                        if (flow!.StartedAt is not null || flow.ObsEnabledUntil <= DateTimeOffset.UtcNow
                            || captures.Count >= options.Value.MaxConcurrentBroadcasts)
                        {
                            await relay.KickAsync(publisher.PodcastId, stoppingToken, publisher.ConnectionId);
                            await FailActivationAsync(publisher.PodcastId, publisher.ConnectionId, "Emission deja demarree, activation expiree ou capacite OBS atteinte.", stoppingToken);
                            continue;
                        }
                        var cancellation = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                        captures.Add(publisher.PodcastId, new(cancellation,
                            CaptureAsync(publisher.PodcastId, flow.OwnerId, publisher.ConnectionId, cancellation.Token)));
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
                catch (Exception ex)
                {
                    logger.LogError(ex, "OBS relay scan failed; retrying");
                }
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        finally
        {
            foreach (var capture in captures.Values) capture.Cancellation.Cancel();
            await Task.WhenAll(captures.Values.Select(c => c.Task));
            foreach (var capture in captures.Values) capture.Cancellation.Dispose();
        }
    }

    private async Task CaptureAsync(Guid id, string ownerId, Guid connectionId, CancellationToken ct)
    {
        using var scope = scopes.CreateScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<ApplicationDbContext>();
        var processor = services.GetRequiredService<ILiveProcessor>();
        var relay = services.GetRequiredService<IPodcastObsRelay>();
        var media = services.GetRequiredService<IOptions<PodcastOptions>>().Value;
        var directory = Path.Combine(media.RootPath, "obs-capture-" + id.ToString("N"));
        string? error = null;
        var started = false;
        try
        {
            Directory.CreateDirectory(directory);
            var podcast = await services.GetRequiredService<IPodcastScheduleService>().StartAsync(id, ownerId, ct, connectionId);
            started = true;
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
            deadline.CancelAfter(TimeSpan.FromMinutes(Math.Min(podcast.PlannedDurationMinutes ?? 1, options.Value.MaxDurationMinutes)));
            await ReceiveAsync(id, ownerId, connectionId, directory, db, processor, media, services, deadline.Token);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("OBS capture {PodcastId} stopped or reached its duration limit", id);
        }
        catch (Exception ex)
        {
            // FFmpeg diagnostics and request URLs can contain credentials: never persist or log them.
            error = ex is PodcastException or ResourceUsageLimitException ? ex.Message : "Reception OBS impossible. Verifiez le relais, FFmpeg et l'encodage H.264/AAC.";
            logger.LogError("OBS capture {PodcastId} failed ({ErrorType})", id, ex.GetType().Name);
        }
        finally
        {
            try
            {
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                await relay.KickAsync(id, timeout.Token, connectionId);
                db.ChangeTracker.Clear();
                var flow = await db.LiveFlow.Include(f => f.Owner).SingleOrDefaultAsync(f => f.PodcastId == id, timeout.Token);
                if (flow is not null)
                {
                    if (flow.ObsKeyHash is null && flow.ObsConnectionId is null) error = null;
                    if (started && flow.Owner.WithdrawalRequestedAt is null)
                        await processor.CompleteAsync(id, ownerId, timeout.Token);
                    if (flow.ObsConnectionId == connectionId || flow.ObsKeyHash is null)
                    {
                        flow.ObsKeyHash = null;
                        flow.ObsEnabledUntil = null;
                        flow.ObsConnectionId = null;
                        flow.ObsLastError = error;
                    }
                    await db.SaveChangesAsync(timeout.Token);
                }
            }
            catch (Exception ex)
            {
                logger.LogError("OBS capture {PodcastId} cleanup failed ({ErrorType}); operator intervention required", id, ex.GetType().Name);
                await FailActivationAsync(id, connectionId, "Finalisation OBS impossible ; intervention de l'administration requise.", CancellationToken.None);
            }
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    private async Task ReceiveAsync(Guid id, string ownerId, Guid connectionId, string directory, ApplicationDbContext db,
        ILiveProcessor processor, PodcastOptions media, IServiceProvider services, CancellationToken ct)
    {
        var settings = options.Value;
        var input = new UriBuilder(new Uri(new Uri(settings.RelayReadUrl.TrimEnd('/') + "/"), $"obs/{id:N}"))
        { Query = $"user={Uri.EscapeDataString(settings.RelayUser)}&pass={Uri.EscapeDataString(settings.RelayPassword)}" };
        var start = new ProcessStartInfo(media.FfmpegPath)
        {
            UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true,
            CreateNoWindow = true, WorkingDirectory = directory
        };
        foreach (var argument in new[]
        {
            "-nostdin", "-hide_banner", "-loglevel", "error", "-rw_timeout", "15000000",
            "-i", input.Uri.AbsoluteUri, "-map", "0:v:0", "-map", "0:a:0?", "-c", "copy",
            "-f", "segment", "-segment_format", "mpegts", "-segment_time", "2",
            "-segment_list", "pipe:1", "-segment_list_type", "csv", "segment-%08d.ts"
        }) start.ArgumentList.Add(argument);
        using var process = Process.Start(start) ?? throw new IOException("FFmpeg could not start.");
        var diagnostics = DrainAsync(process.StandardError);
        using var monitoring = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var guard = GuardBufferAsync(directory, media.MaxSegmentBytes, monitoring.Token);
        var next = 0;
        var total = 0L;
        var revoked = false;
        try
        {
            while (true)
            {
                using var idle = CancellationTokenSource.CreateLinkedTokenSource(ct);
                idle.CancelAfter(TimeSpan.FromSeconds(20));
                var read = process.StandardOutput.ReadLineAsync(idle.Token).AsTask();
                if (await Task.WhenAny(read, guard) == guard) await guard;
                var line = await read;
                if (line is null) break;
                var fields = line.Split(',');
                if (fields.Length != 3 || fields[0] != $"segment-{next:D8}.ts"
                    || !double.TryParse(fields[1], CultureInfo.InvariantCulture, out var begin)
                    || !double.TryParse(fields[2], CultureInfo.InvariantCulture, out var end)
                    || !double.IsFinite(end - begin) || end - begin <= 0 || end - begin > 15)
                    throw new PodcastException(400, "Segments OBS invalides ; configurez H.264/AAC et des images cles toutes les deux secondes.");
                var path = Path.Combine(directory, fields[0]);
                var bytes = new FileInfo(path).Length;
                if (bytes == 0 || bytes > media.MaxSegmentBytes)
                    throw new PodcastException(413, "Segment OBS trop volumineux ; reduisez le debit et l'intervalle des images cles.");
                total = checked(total + bytes);
                await ResourceUsageTracker.RecordStorageAsync(db, ownerId, ownerId, $"podcast/{id:N}", total, ct,
                    write: async () =>
                    {
                        var user = await db.Users.SingleAsync(u => u.Id == ownerId, ct);
                        await db.Entry(user).ReloadAsync(ct);
                        var flow = await db.LiveFlow.SingleAsync(f => f.PodcastId == id, ct);
                        await db.Entry(flow).ReloadAsync(ct);
                        if (user.WithdrawalRequestedAt is not null || flow.ObsKeyHash is null
                            || flow.ObsConnectionId != connectionId || flow.PodcastDeleted)
                        {
                            revoked = true;
                            throw new OperationCanceledException("OBS activation revoked or account closed.", ct);
                        }
                        if (bytes > user.GetEffectiveDiskQuota(DateTimeOffset.UtcNow) - user.DiskUsage)
                            throw new PodcastException(413, "Quota disque atteint.");
                        await using var stream = File.OpenRead(path);
                        await processor.AppendAsync(id, ownerId, next, end - begin, stream, ct);
                        user.DiskUsage = checked(user.DiskUsage + bytes);
                    },
                    policy: services.GetRequiredService<IOptions<ResourceUsageOptions>>().Value,
                    bandwidthMb: (decimal)bytes / (1024m * 1024m));
                File.Delete(path);
                next++;
            }
            await process.WaitForExitAsync(ct);
            if (process.ExitCode != 0 || next == 0) throw new IOException("FFmpeg ingest failed or produced no media.");
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested && !revoked)
        {
            throw new PodcastException(408, "Aucun segment OBS recu depuis vingt secondes ; verifiez l'encodeur et la connexion.");
        }
        finally
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync(CancellationToken.None);
            await diagnostics;
            monitoring.Cancel();
            try { await guard; }
            catch (OperationCanceledException) when (monitoring.IsCancellationRequested) { }
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

    private static async Task DrainAsync(StreamReader reader)
    {
        var buffer = new char[1024];
        while (await reader.ReadAsync(buffer) != 0) { }
    }

    private async Task FailActivationAsync(Guid id, Guid connectionId, string message, CancellationToken ct)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var flow = await db.LiveFlow.SingleOrDefaultAsync(f => f.PodcastId == id, ct);
        if (flow is null || flow.ObsKeyHash is not null && flow.ObsConnectionId != connectionId) return;
        flow.ObsKeyHash = null;
        flow.ObsEnabledUntil = null;
        flow.ObsConnectionId = null;
        flow.ObsLastError = message;
        await db.SaveChangesAsync(ct);
    }
}
