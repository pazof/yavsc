using Yavsc.Services;

namespace Yavsc.Api.Services;

public sealed class PodcastReplayWorker(
    ILiveProcessor processor, IPodcastReplayRemuxer remuxer, IPodcastReplayEditor editor,
    IServiceScopeFactory scopes, ILogger<PodcastReplayWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (await processor.PrepareNextReplayAsync(remuxer, stoppingToken)) continue;
                if (await processor.PublishNextDraftAsync(editor, stoppingToken)) continue;
                await ApplyRetentionAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                logger.LogError(ex, "Podcast MP4 worker failed; retrying the scan");
            }
            try { await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }

    /// <summary>Purges expired refined replay originals and forgets the emissions that expired unpublished.</summary>
    private async Task ApplyRetentionAsync(CancellationToken ct)
    {
        var expired = await processor.PurgeExpiredOriginalsAsync(ct);
        if (expired.Count == 0) return;
        await using var scope = scopes.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IPodcastScheduleService>()
            .MarkExpiredAsync(expired.Select(p => p.Id).ToArray(), ct);
    }
}
