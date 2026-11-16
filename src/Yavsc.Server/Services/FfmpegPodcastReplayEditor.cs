using System.Globalization;
using System.Text;
using Microsoft.Extensions.Options;
using Yavsc.Abstract.Streaming;

namespace Yavsc.Services;

public sealed class FfmpegPodcastReplayEditor(IOptions<PodcastOptions> options) : IPodcastReplayEditor
{
    public async Task RenderAsync(string sourcePath, IReadOnlyList<ReplayRange> kept,
        string outputPath, long maxBytes, CancellationToken ct)
    {
        if (kept.Count == 0) throw new ArgumentException("Aucune plage conservée.", nameof(kept));
        if (kept.Count > ReplayEditPlan.MaxRemovedRanges + 1)
            throw new ArgumentException("Trop de plages conservées.", nameof(kept));
        for (var i = 0; i < kept.Count; i++)
            if (kept[i].StartMs < 0 || kept[i].DurationMs <= 0 || (i > 0 && kept[i].StartMs < kept[i - 1].EndMs)
                || (kept[i].EndMs == ReplayRange.OpenEnd && i != kept.Count - 1))
                throw new ArgumentException("Les plages conservées doivent être ordonnées et disjointes.", nameof(kept));

        var hasAudio = await FfmpegProcess.HasAudioAsync(options.Value.FfmpegPath, sourcePath, ct);

        // Each kept range is a separate input with an accurate seek: FFmpeg decodes from the previous
        // keyframe, drops frames before the start and stops at the end, with timestamps reset to zero
        // and frame durations preserved (trim + setpts loses the last frame duration). concat then
        // rebuilds continuous audio and video timestamps. Only formatted numbers and the
        // server-generated source path are passed to FFmpeg, never user text.
        var arguments = new List<string> { "-nostdin", "-hide_banner", "-loglevel", "error", "-y" };
        var graph = new StringBuilder();
        for (var i = 0; i < kept.Count; i++)
        {
            arguments.AddRange(["-ss", Seconds(kept[i].StartMs)]);
            if (kept[i].EndMs != ReplayRange.OpenEnd) arguments.AddRange(["-to", Seconds(kept[i].EndMs)]);
            arguments.AddRange(["-i", sourcePath]);
            graph.Append(hasAudio ? $"[{i}:v:0][{i}:a:0]" : $"[{i}:v:0]");
        }
        graph.Append($"concat=n={kept.Count}:v=1:a={(hasAudio ? 1 : 0)}[v]");
        if (hasAudio) graph.Append("[a]");

        arguments.AddRange(["-filter_complex", graph.ToString(), "-map", "[v]"]);
        if (hasAudio) arguments.AddRange(["-map", "[a]", "-c:a", "aac"]);
        arguments.AddRange(["-c:v", "libx264", "-pix_fmt", "yuv420p", "-fps_mode", "vfr",
            "-movflags", "+faststart", "-f", "mp4", outputPath]);
        await FfmpegProcess.RunAsync(options.Value.FfmpegPath, arguments, null, outputPath, maxBytes, ct);
    }

    public async Task AddChaptersAsync(string inputPath, IReadOnlyList<PodcastChapter> chapters, long durationMs,
        string outputPath, long maxBytes, CancellationToken ct)
    {
        if (chapters.Count == 0) throw new ArgumentException("Aucun chapitre.", nameof(chapters));
        var metadata = new StringBuilder(";FFMETADATA1\n");
        for (var i = 0; i < chapters.Count; i++)
        {
            var start = chapters[i].TimeMs;
            var end = i + 1 < chapters.Count ? chapters[i + 1].TimeMs : Math.Max(durationMs, start + 1);
            if (start < 0 || end <= start)
                throw new ArgumentException("Les chapitres doivent etre ordonnes et distincts.", nameof(chapters));
            metadata.Append(CultureInfo.InvariantCulture,
                $"[CHAPTER]\nTIMEBASE=1/1000\nSTART={start}\nEND={end}\ntitle={MetadataValue(chapters[i].Title)}\n");
        }
        // Titles are user text: they only reach FFmpeg through this escaped metadata file.
        var metadataPath = outputPath + ".chapters.txt";
        await File.WriteAllTextAsync(metadataPath, metadata.ToString(), new UTF8Encoding(false), ct);
        try
        {
            await FfmpegProcess.RunAsync(options.Value.FfmpegPath,
            [
                "-nostdin", "-hide_banner", "-loglevel", "error", "-y", "-i", inputPath,
                "-f", "ffmetadata", "-i", metadataPath, "-map", "0:v", "-map", "0:a?",
                "-map_metadata", "0", "-map_chapters", "1", "-c", "copy",
                "-movflags", "+faststart", "-f", "mp4", outputPath
            ], null, outputPath, maxBytes, ct);
        }
        finally { File.Delete(metadataPath); }
    }

    private static string MetadataValue(string value)
    {
        var escaped = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            if (char.IsControl(c)) { escaped.Append(' '); continue; }
            if (c is '=' or ';' or '#' or '\\') escaped.Append('\\');
            escaped.Append(c);
        }
        return escaped.ToString();
    }

    private static string Seconds(long milliseconds)
        => (milliseconds / 1000m).ToString("0.###", CultureInfo.InvariantCulture);
}
