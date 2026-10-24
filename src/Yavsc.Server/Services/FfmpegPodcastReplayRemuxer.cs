using System.Diagnostics;
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
            var start = new ProcessStartInfo(options.Value.FfmpegPath)
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = directory
            };
            foreach (var argument in new[]
            {
                "-nostdin", "-hide_banner", "-loglevel", "error", "-y",
                "-protocol_whitelist", "file,concatf", "-f", "mpegts", "-i", $"concatf:{list}",
                "-map", "0:v:0", "-map", "0:a:0?", "-c", "copy", "-bsf:a", "aac_adtstoasc",
                "-movflags", "+faststart", "-f", "mp4", outputPath
            })
                start.ArgumentList.Add(argument);
            using var process = Process.Start(start)
                ?? throw new IOException("Impossible de demarrer FFmpeg.");
            var errors = DrainErrorsAsync(process.StandardError, CancellationToken.None);
            try
            {
                var exited = process.WaitForExitAsync(ct);
                // -fs can report success for a truncated replay. Abort instead of publishing partial media.
                while (!exited.IsCompleted)
                {
                    await Task.WhenAny(exited, Task.Delay(50, ct));
                    ct.ThrowIfCancellationRequested();
                    if (File.Exists(outputPath) && new FileInfo(outputPath).Length > maxBytes)
                        throw new IOException("Le MP4 depasse le quota de stockage disponible.");
                }
                await exited;
                var error = await errors;
                if (process.ExitCode != 0)
                    throw new InvalidDataException($"FFmpeg a echoue ({process.ExitCode}) : {error}");
                var length = new FileInfo(outputPath).Length;
                if (length == 0 || length > maxBytes)
                    throw new IOException("Le MP4 depasse le quota de stockage disponible ou est vide.");
            }
            catch
            {
                if (!process.HasExited) process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync(CancellationToken.None);
                await errors;
                throw;
            }
        }
        finally
        {
            File.Delete(list);
        }
    }

    private static async Task<string> DrainErrorsAsync(StreamReader reader, CancellationToken ct)
    {
        var text = new System.Text.StringBuilder();
        var buffer = new char[1024];
        int count;
        while ((count = await reader.ReadAsync(buffer.AsMemory(), ct)) != 0)
            if (text.Length < 4096) text.Append(buffer, 0, Math.Min(count, 4096 - text.Length));
        return text.ToString();
    }
}
