namespace Yavsc.Services;

public interface IPodcastReplayEditor
{
    /// <summary>
    /// Renders the kept ranges of <paramref name="sourcePath"/> into a new MP4, cut at frame
    /// boundaries with audio cut at the same times.
    /// </summary>
    Task RenderAsync(string sourcePath, IReadOnlyList<ReplayRange> kept, bool hasAudio,
        string outputPath, long maxBytes, CancellationToken ct);
}
