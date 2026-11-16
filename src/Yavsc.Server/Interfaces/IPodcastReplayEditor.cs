using Yavsc.Abstract.Streaming;

namespace Yavsc.Services;

public interface IPodcastReplayEditor
{
    /// <summary>
    /// Renders the kept ranges of <paramref name="sourcePath"/> into a new MP4, cut at frame
    /// boundaries with audio, if any, cut at the same times. A last range ending at
    /// <see cref="ReplayRange.OpenEnd"/> runs to the end of the source.
    /// </summary>
    Task RenderAsync(string sourcePath, IReadOnlyList<ReplayRange> kept,
        string outputPath, long maxBytes, CancellationToken ct);

    /// <summary>
    /// Copies <paramref name="inputPath"/> without re-encoding into a new MP4 carrying these chapters,
    /// given in its timeline and ordered. The last chapter ends at <paramref name="durationMs"/>.
    /// </summary>
    Task AddChaptersAsync(string inputPath, IReadOnlyList<PodcastChapter> chapters, long durationMs,
        string outputPath, long maxBytes, CancellationToken ct);
}
