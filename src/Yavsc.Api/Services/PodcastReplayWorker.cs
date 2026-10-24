using Yavsc.Services;

namespace Yavsc.Api.Services;

public sealed class PodcastReplayWorker(
    ILiveProcessor processor, IPodcastReplayRemuxer remuxer, ILogger<PodcastReplayWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (await processor.PrepareNextReplayAsync(remuxer, stoppingToken)) continue;
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
}
