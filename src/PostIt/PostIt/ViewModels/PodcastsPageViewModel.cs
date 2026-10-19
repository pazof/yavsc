using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibVLCSharp.Shared;
using PostIt.Services;
using Yavsc.Abstract.Identity;
using Yavsc.Abstract.Streaming;
using Yavsc.Api.Client;

namespace PostIt.ViewModels;

public sealed record CaptureDevice(string Name, string Value);

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

    public ObservableCollection<PodcastResponse> Podcasts { get; } = new();
    public ObservableCollection<CaptureDevice> Cameras { get; } = new();
    public ObservableCollection<CaptureDevice> Microphones { get; } = new();
    public bool IsDesktop => OperatingSystem.IsWindows() || OperatingSystem.IsLinux();


    public string Title { get => _title; set { if (SetProperty(ref _title, value)) UpdateCommands(); } }


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
        set { if (SetProperty(ref _camera, value)) UpdateCommands(); }
    }

    public CaptureDevice? SelectedMicrophone
    {
        get => _microphone;
        set { if (SetProperty(ref _microphone, value)) UpdateCommands(); }
    }

    public PodcastResponse? SelectedPodcast
    {
        get => _selected;
        set { if (SetProperty(ref _selected, value)) UpdateCommands(); }
    }

    public IAsyncRelayCommand RefreshCommand { get; }
    public IRelayCommand DetectDevicesCommand { get; }
    public IAsyncRelayCommand StartCommand { get; }
    public IAsyncRelayCommand StopCommand { get; }
    public IAsyncRelayCommand PlayCommand { get; }
    public IAsyncRelayCommand FinishCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }
    public override bool CanNavigateNext { get => false; protected set { } }
    public override bool CanNavigatePrevious { get => !IsBroadcasting; protected set { } }

    public PodcastsPageViewModel(PodcastApiClient client, YavscApiClient api, SessionStatusViewModel session)
    {
        _client = client;
        _api = api;
        _session = session;
        RefreshCommand = new AsyncRelayCommand(RefreshAsync, () => !IsBusy);
        DetectDevicesCommand = new RelayCommand(DetectDevices, () => IsDesktop && !IsBroadcasting && !IsBusy);
        StartCommand = new AsyncRelayCommand(StartAsync,
            () => IsDesktop && !IsBroadcasting && !IsBusy && _session.IsLoggedIn
                && SelectedCamera is not null && !string.IsNullOrWhiteSpace(Title));
        StopCommand = new AsyncRelayCommand(StopAsync, () => IsBroadcasting && !IsBusy);
        PlayCommand = new AsyncRelayCommand(PlayAsync, () => SelectedPodcast is { SegmentCount: > 0 } && !IsBroadcasting && !IsBusy);
        FinishCommand = new AsyncRelayCommand(FinishAsync,
            () => SelectedPodcast is { State: not PodcastState.Completed } p
                && p.OwnerId == _ownerId && !IsBroadcasting && !IsBusy);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync,
            () => SelectedPodcast is { State: not PodcastState.Live } p
                && p.OwnerId == _ownerId && !IsBroadcasting && !IsBusy);
    }

    private void UpdateCommands()
    {
        RefreshCommand.NotifyCanExecuteChanged();
        DetectDevicesCommand.NotifyCanExecuteChanged();
        StartCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
        PlayCommand.NotifyCanExecuteChanged();
        FinishCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
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

    public async Task ActivateAsync()
    {
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
        try
        {
            Core.Initialize();
            using var vlc = CameraStreamer.CreatePreferredLibVlc();
            var capture = new CameraStreamer(vlc);
            Cameras.Clear();
            foreach (var item in capture.GetCaptureDevices()) Cameras.Add(new(item.Key, item.Value));
            Microphones.Clear();
            Microphones.Add(new("Sans microphone", ""));
            foreach (var item in capture.GetAudioCaptureDevices()) Microphones.Add(new(item.Key, item.Value));
            SelectedCamera = Cameras.Count == 0 ? null : Cameras[0];
            SelectedMicrophone = Microphones.Count > 1 ? Microphones[1] : Microphones[0];
            Status = Cameras.Count == 0 ? "Aucune camera detectee." : "Choisissez camera et microphone avant de demarrer.";
        }
        catch (Exception ex) { Status = $"Detection impossible : {ex.Message}. Verifiez l'installation de LibVLC."; }
    }

    private async Task StartAsync()
    {
        IsBusy = true;
        try
        {
            if (SelectedCamera is null) throw new InvalidOperationException("Choisissez une camera.");
            _broadcaster = await PodcastBroadcaster.StartAsync(_client, Title, SelectedCamera.Value,
                string.IsNullOrWhiteSpace(SelectedMicrophone?.Value) ? null : SelectedMicrophone.Value, _pageCancellation.Token);
            _broadcaster.Progress += OnProgress;
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
            await app.PushPageAsync(new VideoPageViewModel(manifestUrl, "application/vnd.apple.mpegurl")
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
}
