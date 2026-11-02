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
    public Control? CapturePreview { get => _capturePreview; private set => SetProperty(ref _capturePreview, value); }

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
        private set { if (SetProperty(ref _isBroadcasting, value)) UpdateCommands(); }
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
    public IAsyncRelayCommand PrepareCommand { get; }
    public IAsyncRelayCommand UpdatePlanCommand { get; }
    public IRelayCommand NewPlanCommand { get; }
    public IAsyncRelayCommand StopCommand { get; }
    public IAsyncRelayCommand PlayCommand { get; }
    public IAsyncRelayCommand FinishCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }
    public IAsyncRelayCommand RetryReplayCommand { get; }
    public override bool CanNavigateNext { get => false; protected set { } }
    public override bool CanNavigatePrevious { get => !IsBroadcasting; protected set { } }

    public PodcastsPageViewModel(PodcastApiClient client, YavscApiClient api, SessionStatusViewModel session)
    {
        _client = client;
        _api = api;
        _session = session;
        RefreshCommand = new AsyncRelayCommand(RefreshAsync, () => !IsBusy);
        DetectDevicesCommand = new RelayCommand(DetectDevices, () => CanBroadcast && !IsBroadcasting && !IsBusy);
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
        PrepareCommand.NotifyCanExecuteChanged();
        UpdatePlanCommand.NotifyCanExecuteChanged();
        NewPlanCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
        PlayCommand.NotifyCanExecuteChanged();
        FinishCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
        RetryReplayCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanNavigatePrevious));
    }

    public async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            var profileUrl = new Uri(new Uri(_api.Settings.ApiUrl.TrimEnd('/') + "/"), "account/me");
            _ownerId = (await _api.CallAsync<Me>(HttpMethod.Get, profileUrl.AbsoluteUri)).UserId;
            var podcasts = await _client.ListAsync();
            Podcasts.Clear();
            foreach (var podcast in podcasts) Podcasts.Add(podcast);
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
        _restoringPreferences = true;
        try
        {
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
        _pageCancellation.Cancel();
        if (_broadcaster is not null) await StopAsync();
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

    private async Task StartAsync()
    {
        IsBusy = true;
        try
        {
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
