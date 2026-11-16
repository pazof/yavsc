using System.Diagnostics;

namespace Yavsc.Services;

internal static class FfmpegProcess
{
    /// <summary>
    /// Runs FFmpeg with arguments passed without a shell and fails instead of leaving a partial,
    /// empty or oversized output.
    /// </summary>
    public static async Task RunAsync(string ffmpegPath, IEnumerable<string> arguments, string? workingDirectory,
        string outputPath, long maxBytes, CancellationToken ct)
    {
        var start = new ProcessStartInfo(ffmpegPath)
        {
            UseShellExecute = false,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        if (workingDirectory is not null) start.WorkingDirectory = workingDirectory;
        foreach (var argument in arguments) start.ArgumentList.Add(argument);
        using var process = Process.Start(start)
            ?? throw new IOException("Impossible de demarrer FFmpeg.");
        var errors = DrainErrorsAsync(process.StandardError, CancellationToken.None);
        try
        {
            var exited = process.WaitForExitAsync(ct);
            // -fs can report success for a truncated output. Abort instead of publishing partial media.
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

    /// <summary>Reports whether a local media file has an audio stream, by decoding one audio frame.</summary>
    public static async Task<bool> HasAudioAsync(string ffmpegPath, string sourcePath, CancellationToken ct)
    {
        var start = new ProcessStartInfo(ffmpegPath)
        {
            UseShellExecute = false,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        foreach (var argument in new[] { "-nostdin", "-hide_banner", "-loglevel", "error", "-i", sourcePath,
            "-map", "0:a:0", "-frames:a", "1", "-f", "null", "-" })
            start.ArgumentList.Add(argument);
        using var process = Process.Start(start)
            ?? throw new IOException("Impossible de demarrer FFmpeg.");
        var errors = DrainErrorsAsync(process.StandardError, CancellationToken.None);
        try
        {
            await process.WaitForExitAsync(ct);
            await errors;
            return process.ExitCode == 0;
        }
        catch
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync(CancellationToken.None);
            await errors;
            throw;
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
