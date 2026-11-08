using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibVLCSharp.Shared;
using PostIt.Services;
using Yavsc.Abstract.Identity;
using Yavsc.Abstract.Streaming;
using Yavsc.Api.Client;

namespace PostIt.ViewModels;

public partial class PodcastsPageViewModel : ViewModelBase
{
    private readonly PodcastApiClient _client;
    private readonly YavscApiClient _api;
    private readonly SessionStatusViewModel _session;
    private CancellationTokenSource _pageCancellation = new();
    private PodcastBroadcaster? _broadcaster;
    private string? _ownerId;
    private string _title = "";
    private string _status = "";
    private bool _isBroadcasting;
    private bool _busy;
    private CaptureDevice? _camera;
    private CaptureDevice? _microphone;
    private PodcastResponse? _selected;
    private Control? _capturePreview;
    private bool _restoringPreferences;
    private bool _keepScreenOnDuringCapture;
    private Guid? _obsPodcastId;
    private int _obsVersion;
    [ObservableProperty] public partial string ObsServerUrl { get; set; } = "";
    [ObservableProperty] public partial string ObsStreamKey { get; set; } = "";
    [ObservableProperty] public partial string ObsStatus { get; set; } = "";
    private IDisposable? _screenAwake;
    public bool SupportsCaptureScreenAwake => OperatingSystem.IsAndroid();
    public bool KeepScreenOnDuringCapture
    {
        get => _keepScreenOnDuringCapture;
        set
        {
            if (!SetProperty(ref _keepScreenOnDuringCapture, value)) return;
            UpdateScreenAwake();
            if (!_restoringPreferences)
            {
                _api.Settings.PodcastKeepScreenOn = value;
                SavePreferences();
            }
        }
    }

    private void UpdateScreenAwake()
    {
        if (KeepScreenOnDuringCapture && IsCapturing)
            _screenAwake ??= Platform.ScreenAwake.Acquire();
        else
        {
            _screenAwake?.Dispose();
            _screenAwake = null;
        }
    }

    private readonly SemaphoreSlim _previewGate = new(1, 1);
    private IPodcastCapture? _previewCapture;
    public bool IsPreviewing => _previewCapture is not null;
    public bool IsCapturing => IsPreviewing || IsBroadcasting;
    public bool HasCapturePreview => CapturePreview is not null;
    public Control? CapturePreview
    {
        get => _capturePreview;
        private set
        {
            if (SetProperty(ref _capturePreview, value)) OnPropertyChanged(nameof(HasCapturePreview));
        }
    }

    public ObservableCollection<PodcastResponse> Podcasts { get; } = new();
    public ObservableCollection<CaptureDevice> Cameras { get; } = new();
    public ObservableCollection<CaptureDevice> Microphones { get; } = new();
    public bool CanBroadcast => Platform.CreatePodcastCaptureAsync is not null
        || (!OperatingSystem.IsAndroid() && (OperatingSystem.IsWindows() || OperatingSystem.IsLinux()));


    public string Title
    {
        get => _title;
        set
        {
            if (!SetProperty(ref _title, value)) return;
            UpdateCommands();
            if (!_restoringPreferences)
            {
                _api.Settings.PodcastDefaultTitle = value;
                SavePreferences();
            }
        }
    }

    [ObservableProperty]
    public partial DateTimeOffset? ScheduledDate { get; set; } = DateTimeOffset.Now.AddDays(1);
    [ObservableProperty]
    public partial TimeSpan? ScheduledTime { get; set; } = new TimeSpan(20, 0, 0);
    [ObservableProperty]
    public partial decimal PlannedDurationMinutes { get; set; } = 60;
    [ObservableProperty]
    public partial bool PublishAnnouncement { get; set; }
    [ObservableProperty]
    public partial string Announcement { get; set; } = "";

    public string Status { get => _status; private set => SetProperty(ref _status, value); }
    public bool IsBroadcasting
    {
        get => _isBroadcasting;
        private set
        {
            if (!SetProperty(ref _isBroadcasting, value)) return;
            OnPropertyChanged(nameof(IsCapturing));
            UpdateScreenAwake();
            UpdateCommands();
        }
    }

    public bool IsBusy
    {
        get => _busy;
        private set { if (SetProperty(ref _busy, value)) UpdateCommands(); }
    }

    public CaptureDevice? SelectedCamera
    {
        get => _camera;
        set
        {
            if (!SetProperty(ref _camera, value)) return;
            UpdateCommands();
            if (!_restoringPreferences && value != null)
            {
                _api.Settings.PodcastCameraId = value.Value;
                SavePreferences();
            }
        }
    }

    public CaptureDevice? SelectedMicrophone
    {
        get => _microphone;
        set
        {
            if (!SetProperty(ref _microphone, value)) return;
            UpdateCommands();
            if (!_restoringPreferences && value != null)
            {
                _api.Settings.PodcastMicrophoneId = value.Value;
                SavePreferences();
            }
        }
    }

    public PodcastResponse? SelectedPodcast
    {
        get => _selected;
        set
        {
            if (!SetProperty(ref _selected, value)) return;
            if (_obsPodcastId != value?.Id) ClearObsCredentials();
            if (value is { State: PodcastState.Planned } && value.OwnerId == _ownerId)
            {
                _title = value.Title;
                OnPropertyChanged(nameof(Title));
                ScheduledDate = value.ScheduledStartAt?.ToLocalTime();
                ScheduledTime = value.ScheduledStartAt?.ToLocalTime().TimeOfDay;
                PlannedDurationMinutes = value.PlannedDurationMinutes ?? 60;
                Announcement = value.PublicAnnouncement ?? "";
                PublishAnnouncement = !string.IsNullOrEmpty(Announcement);
            }
            UpdateCommands();
        }
    }

    public IAsyncRelayCommand RefreshCommand { get; }
    public IRelayCommand DetectDevicesCommand { get; }
    public IAsyncRelayCommand StartCommand { get; }
    public IAsyncRelayCommand PreviewCommand { get; }
    public IAsyncRelayCommand StopPreviewCommand { get; }
    public IAsyncRelayCommand PrepareCommand { get; }
    public IAsyncRelayCommand UpdatePlanCommand { get; }
    public IRelayCommand NewPlanCommand { get; }
    public IAsyncRelayCommand StopCommand { get; }
    public IAsyncRelayCommand PlayCommand { get; }
    public IAsyncRelayCommand FinishCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }
    public IAsyncRelayCommand RetryReplayCommand { get; }
    public IAsyncRelayCommand ActivateObsCommand { get; }
    public IAsyncRelayCommand DeactivateObsCommand { get; }
    public override bool CanNavigateNext { get => false; protected set { } }
    public override bool CanNavigatePrevious { get => !IsBroadcasting; protected set { } }

    public PodcastsPageViewModel(PodcastApiClient client, YavscApiClient api, SessionStatusViewModel session)
    {
        _client = client;
        _api = api;
        _session = session;
        RefreshCommand = new AsyncRelayCommand(RefreshAsync, () => !IsBusy);
        ActivateObsCommand = new AsyncRelayCommand(ActivateObsAsync,
            () => !IsBusy && !IsCapturing && _session.IsLoggedIn
                && SelectedPodcast is { State: PodcastState.Planned } p && p.OwnerId == _ownerId);
        DeactivateObsCommand = new AsyncRelayCommand(DeactivateObsAsync,
            () => !IsBusy && !IsBroadcasting && _session.IsLoggedIn
                && SelectedPodcast is { FlowId: not null } p && p.OwnerId == _ownerId);
        DetectDevicesCommand = new RelayCommand(DetectDevices, () => CanBroadcast && !IsCapturing && !IsBusy);
        PreviewCommand = new AsyncRelayCommand(StartPreviewAsync,
            () => CanBroadcast && !IsCapturing && !IsBusy && _session.IsLoggedIn && SelectedCamera is not null);
        StopPreviewCommand = new AsyncRelayCommand(() => StopPreviewAsync(), () => IsPreviewing && !IsBusy);
        StartCommand = new AsyncRelayCommand(StartAsync,
            () => CanBroadcast && !IsBroadcasting && !IsBusy && _session.IsLoggedIn
                && SelectedCamera is not null && !string.IsNullOrWhiteSpace(Title)
                && (SelectedPodcast == null || SelectedPodcast is { State: PodcastState.Planned } p && p.OwnerId == _ownerId));
        PrepareCommand = new AsyncRelayCommand(() => SavePlanAsync(false),
            () => !IsBusy && !IsBroadcasting && _session.IsLoggedIn && !string.IsNullOrWhiteSpace(Title));
        UpdatePlanCommand = new AsyncRelayCommand(() => SavePlanAsync(true),
            () => !IsBusy && !IsBroadcasting && SelectedPodcast is { State: PodcastState.Planned } p && p.OwnerId == _ownerId);
        NewPlanCommand = new RelayCommand(() =>
        {
            SelectedPodcast = null;
            Title = _api.Settings.PodcastDefaultTitle;
        }, () => !IsBusy && !IsBroadcasting);
        StopCommand = new AsyncRelayCommand(StopAsync, () => IsBroadcasting && !IsBusy);
        PlayCommand = new AsyncRelayCommand(PlayAsync, () => SelectedPodcast is { SegmentCount: > 0 } && !IsBroadcasting && !IsBusy);
        FinishCommand = new AsyncRelayCommand(FinishAsync,
            () => SelectedPodcast is { State: PodcastState.Live or PodcastState.Interrupted } p
                && p.OwnerId == _ownerId && !IsBroadcasting && !IsBusy);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync,
            () => SelectedPodcast is { State: not PodcastState.Live } p
                && p.OwnerId == _ownerId && !IsBroadcasting && !IsBusy);
        RetryReplayCommand = new AsyncRelayCommand(RetryReplayAsync,
            () => SelectedPodcast is { State: PodcastState.Completed, Replay.State: PodcastReplayState.Failed } p
                && p.OwnerId == _ownerId && !IsBroadcasting && !IsBusy);
        _title = _api.Settings.PodcastDefaultTitle;
        _keepScreenOnDuringCapture = _api.Settings.PodcastKeepScreenOn;
        _restoringPreferences = true;
        try
        {
            foreach (var device in _api.Settings.PodcastCameras) Cameras.Add(device);
            foreach (var device in _api.Settings.PodcastMicrophones) Microphones.Add(device);
            RestoreDeviceSelection();
        }
        finally { _restoringPreferences = false; }
    }

    private void RestoreDeviceSelection()
    {
        SelectedCamera = Cameras.FirstOrDefault(d => d.Value == _api.Settings.PodcastCameraId)
            ?? Cameras.FirstOrDefault();
        SelectedMicrophone = Microphones.FirstOrDefault(d => d.Value == _api.Settings.PodcastMicrophoneId)
            ?? (Microphones.Count > 1 ? Microphones[1] : Microphones.FirstOrDefault());
    }

    private void SavePreferences()
    {
        try { _api.Settings.Save(); }
        catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException)
        {
            Status = $"Preferences podcast appliquees, mais sauvegarde impossible : {ex.Message}";
        }
    }

    private void UpdateCommands()
    {
        RefreshCommand.NotifyCanExecuteChanged();
        DetectDevicesCommand.NotifyCanExecuteChanged();
        StartCommand.NotifyCanExecuteChanged();
        PreviewCommand.NotifyCanExecuteChanged();
        StopPreviewCommand.NotifyCanExecuteChanged();
        PrepareCommand.NotifyCanExecuteChanged();
        UpdatePlanCommand.NotifyCanExecuteChanged();
        NewPlanCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
        PlayCommand.NotifyCanExecuteChanged();
        FinishCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
        RetryReplayCommand.NotifyCanExecuteChanged();
        ActivateObsCommand.NotifyCanExecuteChanged();
        DeactivateObsCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanNavigatePrevious));
    }

    public async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            var profileUrl = new Uri(new Uri(_api.Settings.ApiUrl.TrimEnd('/') + "/"), "account/me");
            var ownerId = (await _api.CallAsync<Me>(HttpMethod.Get, profileUrl.AbsoluteUri)).UserId;
            if (_ownerId != ownerId) ClearObsCredentials();
            _ownerId = ownerId;
            var podcasts = await _client.ListAsync();
            Podcasts.Clear();
            foreach (var podcast in podcasts) Podcasts.Add(podcast);
            if (SelectedPodcast is { FlowId: not null } selected && selected.OwnerId == _ownerId)
            {
                var obs = await _client.ObsStatusAsync(selected.Id);
                ObsStatus = obs.Error ?? (obs.Enabled ? "Entree OBS activee. Le direct commence a la connexion du diffuseur." : "Entree OBS desactivee.");
                if (!obs.Enabled) ClearObsCredentials(clearStatus: false);
            }
            Status = "Directs et replays reserves aux utilisateurs connectes.";
        }
        catch (Exception ex) { Status = $"Chargement impossible : {ex.Message}"; }
        finally { IsBusy = false; }
    }

    private async Task SavePlanAsync(bool update)
    {
        IsBusy = true;
        try
        {
            if (ScheduledDate == null || ScheduledTime == null || PlannedDurationMinutes != decimal.Truncate(PlannedDurationMinutes)
                || PlannedDurationMinutes is < 1 or > int.MaxValue)
                throw new InvalidOperationException("Choisissez un debut et une duree entiere positive en minutes.");
            if (PublishAnnouncement && string.IsNullOrWhiteSpace(Announcement))
                throw new InvalidOperationException("Saisissez le texte de l'annonce publique ou desactivez sa publication.");
            var local = DateTime.SpecifyKind(ScheduledDate.Value.Date + ScheduledTime.Value, DateTimeKind.Unspecified);
            if (TimeZoneInfo.Local.IsInvalidTime(local) || TimeZoneInfo.Local.IsAmbiguousTime(local))
                throw new InvalidOperationException("Cette heure est inexistante ou ambigue lors du changement d'heure. Choisissez une autre heure.");
            var start = new DateTimeOffset(local, TimeZoneInfo.Local.GetUtcOffset(local));
            var plan = await _client.PrepareAsync(new PreparePodcastRequest
            {
                Title = Title, ScheduledStartAt = start,
                PlannedDurationMinutes = (int)PlannedDurationMinutes,
                PublicAnnouncement = PublishAnnouncement ? Announcement : null
            }, update ? SelectedPodcast?.Id : null, _pageCancellation.Token);
            await RefreshAsync();
            SelectedPodcast = plan;
            Status = "Emission preparee. Debut et duree previsionnels : demarrage et arret manuels.";
        }
        catch (Exception ex) { Status = $"Preparation impossible : {ex.Message}"; }
        finally { IsBusy = false; }
    }

    public async Task ActivateAsync()
    {
        _session.LogoutCompleted -= ClearObsOnLogout;
        _session.LogoutCompleted += ClearObsOnLogout;
        _restoringPreferences = true;
        try
        {
            KeepScreenOnDuringCapture = _api.Settings.PodcastKeepScreenOn;
            Cameras.Clear();
            Microphones.Clear();
            foreach (var device in _api.Settings.PodcastCameras) Cameras.Add(device);
            foreach (var device in _api.Settings.PodcastMicrophones) Microphones.Add(device);
            RestoreDeviceSelection();
        }
        finally { _restoringPreferences = false; }
        if (_pageCancellation.IsCancellationRequested)
        {
            _pageCancellation.Dispose();
            _pageCancellation = new();
        }
        await RefreshAsync();
    }

    public async Task DeactivateAsync()
    {
        _session.LogoutCompleted -= ClearObsOnLogout;
        ClearObsCredentials();
        _pageCancellation.Cancel();
        await StopPreviewAsync();
        if (_broadcaster is not null) await StopAsync();
    }

    private void ClearObsOnLogout()
    {
        ClearObsCredentials();
        _ownerId = null;
        SelectedPodcast = null;
        UpdateCommands();
    }

    private void ClearObsCredentials(bool clearStatus = true)
    {
        ObsServerUrl = "";
        ObsStreamKey = "";
        _obsPodcastId = null;
        _obsVersion++;
        if (clearStatus) ObsStatus = "";
    }

    private async Task ActivateObsAsync()
    {
        IsBusy = true;
        try
        {
            var podcast = SelectedPodcast ?? throw new InvalidOperationException("Selectionnez une emission programmee.");
            var owner = _ownerId;
            ClearObsCredentials();
            var version = _obsVersion;
            var token = _pageCancellation.Token;
            var credentials = await _client.ActivateObsAsync(podcast.Id, token);
            token.ThrowIfCancellationRequested();
            if (_obsVersion != version || !_session.IsLoggedIn || _ownerId != owner || SelectedPodcast?.Id != podcast.Id)
                throw new InvalidOperationException("La session ou l'emission a change ; les identifiants OBS ne seront pas affiches.");
            _obsPodcastId = podcast.Id;
            ObsServerUrl = credentials.ServerUrl;
            ObsStreamKey = credentials.StreamKey;
            ObsStatus = $"Connectez OBS avant {credentials.ExpiresAt.ToLocalTime():g}. Cette cle remplace toute cle precedente.";
        }

        catch (Exception ex) { Status = $"Activation OBS impossible : {ex.Message}"; }
        finally { IsBusy = false; }
    }

    private async Task DeactivateObsAsync()
    {
        IsBusy = true;
        try
        {
            var podcast = SelectedPodcast ?? throw new InvalidOperationException("Selectionnez une emission.");
            await _client.DeactivateObsAsync(podcast.Id);
            ClearObsCredentials();
            ObsStatus = "Cle revoquee et diffuseur deconnecte. Le replay est finalise apres l'arret de la reception.";
            await RefreshAsync();
        }
        catch (Exception ex) { Status = $"Desactivation OBS impossible : {ex.Message}"; }
        finally { IsBusy = false; }
    }

    private void DetectDevices()
    {
        _restoringPreferences = true;
        try
        {
            Cameras.Clear();
            Microphones.Clear();
            if (Platform.GetPodcastCaptureDevices is { } getDevices)
            {
                var devices = getDevices();
                foreach (var item in devices.Cameras) Cameras.Add(item);
                foreach (var item in devices.Microphones) Microphones.Add(item);
            }
            else
            {
                Core.Initialize();
                using var vlc = CameraStreamer.CreatePreferredLibVlc();
                var capture = new CameraStreamer(vlc);
                foreach (var item in capture.GetCaptureDevices()) Cameras.Add(new(item.Key, item.Value));
                Microphones.Add(new("Sans microphone", ""));
                foreach (var item in capture.GetAudioCaptureDevices()) Microphones.Add(new(item.Key, item.Value));
            }
            RestoreDeviceSelection();
            _api.Settings.PodcastCameras = Cameras.ToArray();
            _api.Settings.PodcastMicrophones = Microphones.ToArray();
            if (SelectedCamera != null) _api.Settings.PodcastCameraId = SelectedCamera.Value;
            if (SelectedMicrophone != null) _api.Settings.PodcastMicrophoneId = SelectedMicrophone.Value;
            Status = Cameras.Count == 0 ? "Aucune camera detectee." : "Choisissez camera et microphone avant de demarrer.";
            SavePreferences();
        }
        catch (Exception ex) { Status = $"Detection impossible : {ex.Message}. Verifiez l'installation de LibVLC."; }
        finally { _restoringPreferences = false; }
    }

    private async Task StartPreviewAsync()
    {
        IsBusy = true;
        await _previewGate.WaitAsync();
        IPodcastCapture? capture = null;
        try
        {
            var ct = _pageCancellation.Token;
            ct.ThrowIfCancellationRequested();
            if (_previewCapture is not null) return;
            var camera = SelectedCamera?.Value ?? throw new InvalidOperationException("Choisissez une camera.");
            capture = Platform.CreatePodcastPreviewAsync is { } createPreview
                ? await createPreview(camera, ct)
                : new LibVlcPodcastCapture(
                    vlc => new CameraStreamer(vlc).CreateCaptureMedia(camera, null), null, preview: true);
            ct.ThrowIfCancellationRequested();
            if (!_session.IsLoggedIn) throw new InvalidOperationException("La session a ete deconnectee.");
            if (capture.Preview is null) throw new InvalidOperationException("Apercu camera indisponible.");
            _previewCapture = capture;
            CapturePreview = capture.Preview;
            _session.LogoutCompleted += OnLogout;
            UpdatePreviewState();
            Status = "Previsualisation locale uniquement : aucune emission, aucun enregistrement, microphone inactif.";
            _ = ObservePreviewAsync(capture);
            capture = null;
        }
        catch (Exception ex) { Status = $"Previsualisation impossible : {ex.Message}"; }
        finally
        {
            try { if (capture is not null) await capture.DisposeAsync(); }
            catch (Exception ex) { Status += $" Nettoyage : {ex.Message}"; }
            _previewGate.Release();
            IsBusy = false;
        }
    }

    private void UpdatePreviewState()
    {
        OnPropertyChanged(nameof(IsPreviewing));
        OnPropertyChanged(nameof(IsCapturing));
        UpdateScreenAwake();
        UpdateCommands();
    }

    private async Task ObservePreviewAsync(IPodcastCapture capture)
    {
        string? failure = null;
        try { await capture.Completion; }
        catch (Exception ex) { failure = ex.Message; }
        await StopPreviewAsync(capture, failure);
    }

    private async Task<bool> StopPreviewAsync(IPodcastCapture? expected = null, string? failure = null)
    {
        await _previewGate.WaitAsync();
        try
        {
            if (_previewCapture is not { } capture || (expected is not null && expected != capture)) return true;
            _previewCapture = null;
            CapturePreview = null;
            _session.LogoutCompleted -= OnLogout;
            try { await capture.DisposeAsync(); }
            finally { UpdatePreviewState(); }
            Status = failure is null ? "Previsualisation arretee." : $"Previsualisation interrompue : {failure}";
            return true;
        }
        catch (Exception ex)
        {
            Status = $"Arret de la previsualisation impossible : {ex.Message}";
            return false;
        }
        finally { _previewGate.Release(); }
    }

    private async Task StartAsync()
    {
        IsBusy = true;
        try
        {
            if (!await StopPreviewAsync())
                throw new InvalidOperationException("Impossible de liberer la camera de previsualisation.");
            _pageCancellation.Token.ThrowIfCancellationRequested();
            if (SelectedCamera is null) throw new InvalidOperationException("Choisissez une camera.");
            var camera = SelectedCamera.Value;
            var microphone = string.IsNullOrWhiteSpace(SelectedMicrophone?.Value) ? null : SelectedMicrophone.Value;
            if (Platform.RequestPodcastPermissionsAsync is { } requestPermissions)
                await requestPermissions(microphone, _pageCancellation.Token);
            _broadcaster = Platform.CreatePodcastCaptureAsync is { } createCapture
                ? await PodcastBroadcaster.StartAsync(_client, Title,
                    (directory, ct) => createCapture(camera, microphone, directory, ct), _pageCancellation.Token, SelectedPodcast?.Id)
                : await PodcastBroadcaster.StartAsync(_client, Title, camera, microphone, _pageCancellation.Token, SelectedPodcast?.Id);
            _broadcaster.Progress += OnProgress;
            CapturePreview = _broadcaster.Preview;
            _session.LogoutCompleted += OnLogout;
            IsBroadcasting = true;
            Status = "Capture en cours ; attente du premier segment HLS.";
            _ = ObserveAsync(_broadcaster);
        }
        catch (Exception ex) { Status = $"Demarrage impossible : {ex.Message}"; }
        finally { IsBusy = false; }
    }

    private void OnProgress(PodcastResponse podcast)
        => Dispatcher.UIThread.Post(() =>
        {
            if (_broadcaster?.Podcast.Id == podcast.Id)
                Status = $"En direct : {podcast.SegmentCount} segments, {podcast.DurationSeconds:0} s envoyees.";
        });

    private async Task ObserveAsync(PodcastBroadcaster broadcaster)
    {
        try
        {
            await broadcaster.Completion;
            Status = broadcaster.Podcast.SegmentCount > 0
                ? "Direct termine. Le replay est disponible."
                : "Direct termine sans segment recu : aucun replay a lire.";
        }
        catch (Exception ex)
        {
            Status = $"Diffusion interrompue : {ex.Message}. Les segments recus restent disponibles ; les segments non envoyes sont conserves localement.";
        }
        finally
        {
            CapturePreview = null;
            broadcaster.Progress -= OnProgress;
            _session.LogoutCompleted -= OnLogout;
            try { await broadcaster.DisposeAsync(); }
            catch (Exception ex) { Status += $" Nettoyage : {ex.Message}"; }
            _broadcaster = null;
            IsBroadcasting = false;
        }
    }

    public async Task StopAsync()
    {
        IsBusy = true;
        try
        {
            if (_broadcaster is not null) await _broadcaster.StopAsync();
        }
        catch (Exception ex) { Status = $"Arret du direct : {ex.Message}"; }
        finally { IsBusy = false; }
    }

    private async void OnLogout()
    {
        // Releasing capture is required even after the HTTP credentials have been cleared.
        await StopPreviewAsync();
        await StopAsync();
    }

    private async Task PlayAsync()
    {
        try
        {
            var podcast = SelectedPodcast ?? throw new InvalidOperationException("Selectionnez un podcast.");
            var app = Application.Current as App ?? throw new InvalidOperationException("Application indisponible.");
            var manifestUrl = new Uri(new Uri(_api.Settings.ApiUrl.TrimEnd('/') + "/"), $"podcasts/{podcast.Id}/index.m3u8");
            var catalogueUrl = new Uri(new Uri(_api.Settings.Authentication.Authority.TrimEnd('/') + "/"), "Podcast");
            await app.PushPageAsync(new VideoPageViewModel(manifestUrl, "application/vnd.apple.mpegurl", catalogueUrl)
            {
                PodcastClient = _client, Podcast = podcast
            });
        }
        catch (Exception ex) { Status = $"Lecture impossible : {ex.Message}"; }
    }

    private async Task FinishAsync()
    {
        IsBusy = true;
        try
        {
            var podcast = SelectedPodcast ?? throw new InvalidOperationException("Selectionnez un podcast.");
            await _client.CompleteAsync(podcast.Id);
            await RefreshAsync();
        }
        catch (Exception ex) { Status = $"Finalisation impossible : {ex.Message}"; }
        finally { IsBusy = false; }
    }

    private async Task DeleteAsync()
    {
        IsBusy = true;
        try
        {
            var podcast = SelectedPodcast ?? throw new InvalidOperationException("Selectionnez un podcast.");
            await _client.DeleteAsync(podcast.Id);
            SelectedPodcast = null;
            await RefreshAsync();
        }
        catch (Exception ex) { Status = $"Suppression impossible : {ex.Message}"; }
        finally { IsBusy = false; }
    }

    private async Task RetryReplayAsync()
    {
        IsBusy = true;
        try
        {
            var podcast = SelectedPodcast ?? throw new InvalidOperationException("Selectionnez un podcast.");
            await _client.RetryReplayAsync(podcast.Id);
            await RefreshAsync();
        }
        catch (Exception ex) { Status = $"Relance du MP4 impossible : {ex.Message}"; }
        finally { IsBusy = false; }
    }
}
