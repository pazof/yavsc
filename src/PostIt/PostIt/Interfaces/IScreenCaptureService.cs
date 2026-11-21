using System;
using System.Threading;
using System.Threading.Tasks;

namespace PostIt.Interfaces;

public enum ScreenCaptureSource
{
    Screen,
    Window,
    Prompt
}

public record CaptureOptions(
    string OutputFilePath,
    int Fps = 60,
    IntPtr? TargetWindowHandle = null, // Null = Écran principal complet
    bool CaptureCursor = true,
    string? RestoreToken = null, // Token sauvegardé de la session précédente
    ScreenCaptureSource Source = ScreenCaptureSource.Screen,
    bool CaptureAudio = true
);

public interface IScreenCaptureService : IDisposable
{
    bool IsRecording { get; }
    string? LatestRestoreToken { get; } // Nouveau token à sauvegarder après le démarrage

    event EventHandler<TimeSpan>? RecordingDurationUpdated;

    Task StartRecordingAsync(CaptureOptions options, CancellationToken cancellationToken = default);
    Task StopRecordingAsync();
}
