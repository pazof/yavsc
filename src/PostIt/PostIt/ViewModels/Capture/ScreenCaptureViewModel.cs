using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Interfaces;
using PostIt.Models;
using PostIt.Services;
using Yavsc.Api.Client;

namespace PostIt.ViewModels.Capture;

public record CaptureSourceOption(ScreenCaptureSource Source, string Label);

public partial class ScreenCaptureViewModel : ViewModelBase, IActionStatusViewModel
{
    IScreenCaptureService _captureService;
    private readonly UserFilesApiClient? _userFiles;
    private string? _outputFilePath;

    public CaptureSourceOption[] CaptureSources { get; } =
    [
        new(ScreenCaptureSource.Screen, "Écran entier"),
        new(ScreenCaptureSource.Window, "Fenêtre"),
        new(ScreenCaptureSource.Prompt, "Choisir au lancement")
    ];

    private CaptureSourceOption _selectedCaptureSource = null!;
    public CaptureSourceOption SelectedCaptureSource
    {
        get => _selectedCaptureSource;
        set => SetProperty(ref _selectedCaptureSource, value);
    }

    private bool _captureAudio = true;
    public bool CaptureAudio
    {
        get => _captureAudio;
        set => SetProperty(ref _captureAudio, value);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
                OnPropertyChanged(nameof(CanConfigure));
        }
    }

    public bool CanConfigure => !IsRecording && !IsBusy;

    private string _statusMessage = "Prêt.";
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private StatusNotice _actionStatus = StatusNotice.Info("Prêt.");
    public StatusNotice ActionStatus
    {
        get => _actionStatus;
        set => SetProperty(ref _actionStatus, value);
    }

    public ScreenCaptureViewModel(UserFilesApiClient userFiles)
    {
        if (Platform.IsScreenCaptureSupported)
            _captureService = Platform.CreateScreenCaptureService!();
        else
            throw new NotSupportedException("Screen capture is not supported on this platform.");

        _userFiles = userFiles;
        SelectedCaptureSource = CaptureSources[0];
    }

    public ScreenCaptureViewModel() : this(null!) { }

    [ObservableProperty]
    public partial string ScreencastNName { get; set; } = "screencast";

    private string GetTokenFilePath() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        App.AppName,
        $"{SanitizeFileName(ScreencastNName)}_{SelectedCaptureSource.Source.ToString().ToLowerInvariant()}_token.txt"
    );


    public bool IsRecording { get => _captureService?.IsRecording ?? false; }

    [RelayCommand]
    public async Task ToggleRecordingAsync()
    {
        if (_captureService.IsRecording)
        {
            IsBusy = true;
            bool finalized = false;
            try
            {
                await _captureService.StopRecordingAsync();
                finalized = true;
                this.OnPropertyChanged(nameof(IsRecording));
                this.OnPropertyChanged(nameof(CanConfigure));
                await UploadRecordingAsync();
            }
            catch (Exception ex)
            {
                this.SetErrorStatus(finalized
                    ? $"La capture est finalisée localement, mais son envoi a échoué : {ex.Message}"
                    : $"La finalisation de la capture a échoué : {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
            return;
        }

        // 1. Lire l'ancien token s'il existe
        string tokenFilePath = GetTokenFilePath();
        string? savedToken = File.Exists(tokenFilePath)
            ? await File.ReadAllTextAsync(tokenFilePath)
            : null;

        string fileName = EnsureMp4Extension(SanitizeFileName(ScreencastNName));
        _outputFilePath = Path.Combine(Path.GetTempPath(), fileName);
        if (File.Exists(_outputFilePath))
            File.Delete(_outputFilePath);

        var options = new CaptureOptions(
            OutputFilePath: _outputFilePath,
            Fps: 60,
            CaptureCursor: true,
            RestoreToken: savedToken,
            Source: SelectedCaptureSource.Source,
            CaptureAudio: CaptureAudio
        );

        // 2. Lancer la capture (silencieuse si le token est valide)
        await _captureService.StartRecordingAsync(options);

        // 3. Sauvegarder le nouveau token généré pour la prochaine fois
        if (!string.IsNullOrEmpty(_captureService.LatestRestoreToken))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(tokenFilePath)!);
            await File.WriteAllTextAsync(tokenFilePath, _captureService.LatestRestoreToken);
        }
        this.OnPropertyChanged(nameof(IsRecording));
        this.OnPropertyChanged(nameof(CanConfigure));
    }

    private async Task UploadRecordingAsync()
    {
        if (_userFiles is null)
            throw new InvalidOperationException("Le client de fichiers utilisateur est indisponible.");
        if (string.IsNullOrWhiteSpace(_outputFilePath) || !File.Exists(_outputFilePath))
            throw new FileNotFoundException("Le fichier vidéo finalisé est introuvable.", _outputFilePath);
        if (new FileInfo(_outputFilePath).Length == 0)
            throw new InvalidOperationException("Le fichier vidéo finalisé est vide.");

        this.SetInfoStatus("Envoi de la capture dans Mes fichiers / screencast...");
        var received = await _userFiles.UploadAsync("screencast", _outputFilePath, "video/mp4");
        if (received.Count != 1 || received[0].QuotaOffense)
            throw new InvalidOperationException("Le quota de stockage a refusé la capture.");

        File.Delete(_outputFilePath);
        _outputFilePath = null;
        this.SetInfoStatus("Capture envoyée dans Mes fichiers / screencast.");
    }

    private static string EnsureMp4Extension(string name) =>
        name.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ? name : name + ".mp4";

    private static string SanitizeFileName(string value)
    {
        string name = string.IsNullOrWhiteSpace(value) ? "screencast" : value.Trim();
        foreach (char invalid in Path.GetInvalidFileNameChars())
            name = name.Replace(invalid, '_');
        name = name.Replace('/', '_').Replace('\\', '_').Trim('.');
        return string.IsNullOrWhiteSpace(name) ? "screencast" : name;
    }

    public void StopRecordingAsync()
    {
        if (_captureService.IsRecording)
        {
            ToggleRecordingAsync().GetAwaiter().GetResult();
        }
    }

    public override bool CanNavigateNext { get => true; protected set => throw new System.NotImplementedException(); }
    public override bool CanNavigatePrevious { get => true; protected set => throw new System.NotImplementedException(); }
}
