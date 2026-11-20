using System;
using System.IO;
using System.Threading.Tasks;
using PostIt.Interfaces;
using PostIt.Services;

namespace PostIt.ViewModels.Capture;

public class ScreenCaptureViewModel : ViewModelBase
{
    IScreenCaptureService _captureService;

    public ScreenCaptureViewModel()
    {
        if (Platform.IsScreenCaptureSupported)
            _captureService = Platform.CreateScreenCaptureService!();
        else
            throw new NotSupportedException("Screen capture is not supported on this platform.");
    }
     string screencastNName = "screencast";

    private string GetTokenFilePath() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        App.AppName,
        screencastNName+"_token.txt"
    );

    public bool IsRecording => _captureService?.IsRecording ?? false;
    public async Task ToggleRecordingAsync()
    {
        if (_captureService.IsRecording)
        {
            await _captureService.StopRecordingAsync();
            return;
        }

        // 1. Lire l'ancien token s'il existe
        string tokenFilePath = GetTokenFilePath();
        string? savedToken = File.Exists(tokenFilePath)
            ? await File.ReadAllTextAsync(tokenFilePath)
            : null;

        var options = new CaptureOptions(
            OutputFilePath: Path.Combine(Path.GetTempPath(), screencastNName + ".mp4"),
            Fps: 60,
            CaptureCursor: true,
            RestoreToken: savedToken
        );

        // 2. Lancer la capture (silencieuse si le token est valide)
        await _captureService.StartRecordingAsync(options);

        // 3. Sauvegarder le nouveau token généré pour la prochaine fois
        if (!string.IsNullOrEmpty(_captureService.LatestRestoreToken))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(tokenFilePath)!);
            await File.WriteAllTextAsync(tokenFilePath, _captureService.LatestRestoreToken);
        }
    }

    public void StopRecordingAsync()
    {
        if (_captureService.IsRecording)
        {
            _captureService.StopRecordingAsync().GetAwaiter().GetResult();
        }
    }

    public override bool CanNavigateNext { get => true; protected set => throw new System.NotImplementedException(); }
    public override bool CanNavigatePrevious { get => true; protected set => throw new System.NotImplementedException(); }
}
