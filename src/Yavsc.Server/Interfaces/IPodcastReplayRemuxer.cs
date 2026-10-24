namespace Yavsc.Services;

public interface IPodcastReplayRemuxer
{
    Task RemuxAsync(string directory, int segmentCount, string outputPath, long maxBytes, CancellationToken ct);
}
