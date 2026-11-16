using Microsoft.Extensions.Options;

namespace Yavsc.Services;

public sealed class FfmpegPodcastReplayRemuxer(IOptions<PodcastOptions> options) : IPodcastReplayRemuxer
{
    public async Task RemuxAsync(string directory, int segmentCount, string outputPath, long maxBytes, CancellationToken ct)
    {
        if (segmentCount < 1) throw new InvalidDataException("Le podcast ne contient aucun segment.");
        var list = Path.Combine(directory, "replay-concat.txt");
        // Concatenate TS bytes without resetting timestamps or guessing durations between segments.
        // Only generated local basenames enter the protocol, never uploaded metadata.
        try
        {
            await File.WriteAllLinesAsync(list,
                Enumerable.Range(0, segmentCount).Select(i => $"{i:D8}.ts"), ct);
            await FfmpegProcess.RunAsync(options.Value.FfmpegPath,
            [
                "-nostdin", "-hide_banner", "-loglevel", "error", "-y",
                "-protocol_whitelist", "file,concatf", "-f", "mpegts", "-i", $"concatf:{list}",
                "-map", "0:v:0", "-map", "0:a:0?", "-c", "copy", "-bsf:a", "aac_adtstoasc",
                "-movflags", "+faststart", "-f", "mp4", outputPath
            ], directory, outputPath, maxBytes, ct);
        }
        finally
        {
            File.Delete(list);
        }
    }
}
