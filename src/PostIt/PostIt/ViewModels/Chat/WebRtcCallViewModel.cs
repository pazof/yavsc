using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Services;
using Yavsc.Abstract.Chat.Calls;

namespace PostIt.ViewModels.Chat;

public partial class WebRtcCallViewModel : ObservableObject, IAsyncDisposable
{
    private readonly IWebRtcCallSignaling _signaling;
    private readonly Func<IWebRtcMedia> _createMedia;
    private readonly Settings? _settings;
    private readonly SemaphoreSlim _events = new(1);
    private readonly CancellationTokenSource _lifetime = new();
    private CancellationTokenSource? _callLifetime;
    private CallSession? _call;
    private IWebRtcMedia? _media;
    private WebRtcMediaStatus? _mediaStatus;
    private Action<CallSignal>? _sendSignal;
    private Action<WebRtcMediaStatus>? _receiveStatus;
    private bool _caller;
    private bool _accepted;
    private bool _disposed;
    private string? _camera;
    private string? _microphone;

    public ObservableCollection<CaptureDevice> Cameras { get; } = [];
    public ObservableCollection<CaptureDevice> Microphones { get; } = [];
    [ObservableProperty] public partial CaptureDevice? Camera { get; set; }
    [ObservableProperty] public partial CaptureDevice? Microphone { get; set; }
    [ObservableProperty] public partial bool AllowAddressDisclosure { get; set; }
    [ObservableProperty] public partial string Target { get; set; } = "";
    [ObservableProperty] public partial string Room { get; set; } = "";
    [ObservableProperty] public partial string Status { get; set; } = "Appel WebRTC : aucun appel.";
    [ObservableProperty, NotifyPropertyChangedFor(nameof(CanConfigure))]
    public partial bool Busy { get; set; }
    [ObservableProperty, NotifyPropertyChangedFor(nameof(CanConfigure))]
    public partial bool Incoming { get; set; }
    [ObservableProperty] public partial bool Preparing { get; set; }
    [ObservableProperty] public partial Control? LocalVideo { get; set; }
    [ObservableProperty] public partial Control? RemoteVideo { get; set; }
    public bool CanConfigure => !Busy || Incoming;

    public WebRtcCallViewModel(IWebRtcCallSignaling signaling, Func<IWebRtcMedia>? createMedia = null,
        Settings? settings = null)
    {
        _signaling = signaling;
        _settings = settings;
        _createMedia = createMedia ?? Platform.CreateWebRtcMedia ?? (() => new LinuxWebRtcMedia());
        signaling.Invitation += OnInvitation;
        signaling.Accepted += OnAccepted;
        signaling.Signal += OnSignal;
        signaling.Ended += OnEnded;
        signaling.Interrupted += OnInterrupted;
        Platform.PlaybackSuspending += OnSuspending;
        if (settings is not null)
        {
            foreach (var device in settings.PodcastCameras) Cameras.Add(device);
            foreach (var device in settings.PodcastMicrophones.Where(d => !string.IsNullOrEmpty(d.Value)))
                Microphones.Add(device);
            RestoreSelection();
        }
    }

    private void RestoreSelection()
    {
        Camera = Cameras.FirstOrDefault(d => d.Value == _settings?.PodcastCameraId) ?? Cameras.FirstOrDefault();
        Microphone = Microphones.FirstOrDefault(d => d.Value == _settings?.PodcastMicrophoneId)
            ?? Microphones.FirstOrDefault();
    }

    [RelayCommand]
    private void RefreshDevices()
    {
        try
        {
            if (!CanConfigure) throw new InvalidOperationException("Terminez l'appel avant de changer les peripheriques.");
            var devices = (Platform.GetWebRtcDevices ?? LinuxWebRtcMedia.GetDevices)();
            Cameras.Clear();
            Microphones.Clear();
            foreach (var device in devices.Cameras) Cameras.Add(device);
            foreach (var device in devices.Microphones.Where(d => !string.IsNullOrEmpty(d.Value))) Microphones.Add(device);
            RestoreSelection();
            Status = Cameras.Count == 0 ? "Aucune camera detectee."
                : "Peripheriques actualises. Le profil de masquage IP sera applique par le serveur.";
        }
        catch (Exception error) { Status = "Peripheriques indisponibles : " + error.Message; }
    }

    private void SelectDevices()
    {
        if (Camera is null || Microphone is null)
            throw new InvalidOperationException("Une camera et un microphone sont requis.");
        _camera = Camera.Value;
        _microphone = Microphone.Value;
        if (_settings is not null)
        {
            _settings.PodcastCameraId = _camera;
            _settings.PodcastMicrophoneId = _microphone;
            try { _settings.Save(); }
            catch (Exception error) when (error is System.IO.IOException or UnauthorizedAccessException)
            {
                throw new InvalidOperationException("Sauvegarde des peripheriques impossible.", error);
            }
        }
    }

    private void CheckPolicy(CallSession session)
    {
        if (session.Transport != CallTransports.WebRtc)
            throw new InvalidOperationException("Le transport demande n'est pas WebRTC.");
        if (!session.RelayOnly && !AllowAddressDisclosure)
            throw new InvalidOperationException(
                "Activez le masquage IP dans votre profil ou consentez au partage IP pour cet appel.");
    }

    [RelayCommand]
    private Task InviteAsync() => RunAsync(async () =>
    {
        if (Busy) throw new InvalidOperationException("Un appel est deja en cours.");
        SelectDevices();
        if (string.IsNullOrWhiteSpace(Target) || string.IsNullOrWhiteSpace(Room))
            throw new InvalidOperationException("Selectionnez une salle jointe et un autre utilisateur.");
        _callLifetime = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token);
        _caller = true;
        Busy = true;
        _call = await _signaling.InviteAsync(Room, Target.Trim(), _callLifetime.Token);
        CheckPolicy(_call);
        Status = $"Invitation envoyee a {_call.Callee} ; attente du consentement.";
    });

    [RelayCommand]
    private Task AcceptAsync() => RunAsync(async () =>
    {
        if (_call is null || !Incoming) throw new InvalidOperationException("Aucune invitation a accepter.");
        CheckPolicy(_call);
        SelectDevices();
        Incoming = false;
        _accepted = true;
        await _signaling.AcceptAsync(_call.Id, _callLifetime!.Token);
        Status = "Invitation acceptee ; preparation des medias.";
    });

    [RelayCommand]
    public Task HangupAsync()
    {
        _callLifetime?.Cancel();
        return RunAsync(() => StopAsync("Appel termine.", true));
    }

    private void OnInvitation(CallSession call) => Queue(async () =>
    {
        if (call.Transport != CallTransports.WebRtc || _call is not null)
        {
            await _signaling.EndAsync(call.Id, _lifetime.Token);
            Status = "Invitation refusee : transport incompatible ou appel deja en cours.";
            return;
        }
        _call = call;
        _caller = false;
        _callLifetime = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token);
        Busy = Incoming = true;
        Status = $"{call.Caller} propose un appel dans {call.RoomName}. "
            + (call.RelayOnly ? "Masquage IP : relais TURN obligatoire. " : "Mode direct : consentement au partage IP requis. ")
            + "Acceptez ou refusez.";
    });

    private void OnAccepted(CallAccepted accepted) => Queue(async () =>
    {
        if (_call?.Id != accepted.Session.Id) return;
        if (_media is not null || (!_caller && !_accepted) ||
            (_call.RelayOnly && !accepted.Session.RelayOnly))
            throw new InvalidOperationException("Acceptation ou politique WebRTC inattendue.");
        CheckPolicy(accepted.Session);
        _call = accepted.Session;
        Incoming = false;
        var configuration = new WebRtcMediaConfiguration(accepted.IceServers, _call.RelayOnly,
            _caller, _camera!, _microphone!);
        WebRtcPrivacy.ValidateConfiguration(configuration);
        _media = _createMedia();
        var id = _call.Id;
        var media = _media;
        _sendSignal = signal => Queue(async () =>
        {
            if (_call?.Id != id || !ReferenceEquals(_media, media)) return;
            WebRtcPrivacy.ValidateSignal(signal, _call.RelayOnly);
            await _signaling.SendAsync(id, signal, _callLifetime!.Token);
        });
        _receiveStatus = status => Queue(() =>
        {
            if (_call?.Id == id && ReferenceEquals(_media, media))
            {
                WebRtcPrivacy.ValidateStatus(status, _call.RelayOnly);
                _mediaStatus = status;
            }
            return Task.CompletedTask;
        });
        media.Signal += _sendSignal;
        media.StatusChanged += _receiveStatus;
        LocalVideo = media.LocalVideo;
        RemoteVideo = media.RemoteVideo;
        Status = _call.RelayOnly ? "Connexion WebRTC via TURN uniquement ; aucun repli direct."
            : "Connexion WebRTC ICE/STUN/TURN.";
        await media.StartAsync(configuration, _callLifetime!.Token);
        _ = MonitorAsync(id, media, _callLifetime.Token);
    });

    private void OnSignal(Guid id, CallSignal signal) => Queue(async () =>
    {
        if (_call?.Id != id) return;
        if (_media is null) throw new InvalidOperationException("Signal recu avant le consentement.");
        WebRtcPrivacy.ValidateSignal(signal, false);
        await _media.HandleSignalAsync(signal, _callLifetime!.Token);
    });

    private void OnEnded(CallEnded ended) => QueueStop(ended.CallId,
        () => StopAsync("Appel termine : " + ended.Reason, false));
    private void OnInterrupted() => QueueStop(null,
        () => StopAsync("Connexion SignalR interrompue ; medias arretes.", false));
    private void OnSuspending() => QueueStop(null,
        () => StopAsync("Application en arriere-plan ; appel termine.", true));

    private void QueueStop(Guid? id, Func<Task> action) => Dispatcher.UIThread.Post(async () =>
    {
        if (id is not null && _call?.Id != id) return;
        _callLifetime?.Cancel();
        await RunAsync(() => id is null || _call?.Id == id ? action() : Task.CompletedTask);
    });

    private async Task MonitorAsync(Guid id, IWebRtcMedia media, CancellationToken ct)
    {
        var progress = Stopwatch.StartNew();
        var received = false;
        long audio = 0, video = 0;
        try
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                if (media.Completion.IsCompleted)
                {
                    await media.Completion;
                    throw new InvalidOperationException("Le moteur WebRTC s'est arrete.");
                }
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (_call?.Id != id || _mediaStatus is not { } status) return;
                    if (status.ConnectionState is "failed" or "closed" or "disconnected")
                        throw new InvalidOperationException("Connexion ICE interrompue ; aucun repli automatique.");
                    if (!status.HasRemoteMedia) return;
                    if (status.AudioPackets > audio && status.VideoFrames > video)
                    {
                        audio = status.AudioPackets;
                        video = status.VideoFrames;
                        progress.Restart();
                    }
                    received = true;
                    Status = _call.RelayOnly ? "Audio-video WebRTC etabli via TURN (IP masquee)."
                        : "Audio-video WebRTC etabli.";
                });
                if (progress.Elapsed > TimeSpan.FromSeconds(30))
                    throw new TimeoutException(received ? "Reception audio-video interrompue."
                        : "Aucun media distant. Verifiez ICE, TURN et les pare-feu ; aucun repli hors politique.");
                await Task.Delay(500, ct);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception error)
        {
            Queue(() => _call?.Id == id && ReferenceEquals(_media, media)
                ? FailAsync(error) : Task.CompletedTask);
        }
    }

    private void Queue(Func<Task> action) => Dispatcher.UIThread.Post(async () => await RunAsync(action));

    private async Task RunAsync(Func<Task> action)
    {
        await _events.WaitAsync();
        try
        {
            if (_disposed) return;
            Preparing = true;
            await action();
        }
        catch (OperationCanceledException) when (
            _lifetime.IsCancellationRequested || _callLifetime?.IsCancellationRequested == true) { }
        catch (Exception error) { await FailAsync(error); }
        finally { Preparing = false; _events.Release(); }
    }

    private async Task FailAsync(Exception error)
    {
        try { await StopAsync("Echec de l'appel : " + error.Message, true); }
        catch (Exception cleanup)
        {
            Status = $"Echec de l'appel : {error.Message}. Nettoyage/signalisation : {cleanup.Message}";
        }
    }

    private async Task StopAsync(string status, bool notify)
    {
        var call = _call;
        _call = null;
        Incoming = Busy = _accepted = false;
        _callLifetime?.Cancel();
        var media = _media;
        _media = null;
        _mediaStatus = null;
        try
        {
            if (media is not null)
            {
                media.Signal -= _sendSignal;
                media.StatusChanged -= _receiveStatus;
                await media.DisposeAsync();
            }
        }
        finally
        {
            LocalVideo = RemoteVideo = null;
            _sendSignal = null;
            _receiveStatus = null;
            _callLifetime?.Dispose();
            _callLifetime = null;
            Status = status;
            if (notify && call is not null)
            {
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await _signaling.EndAsync(call.Id, timeout.Token);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _lifetime.Cancel();
        _callLifetime?.Cancel();
        await _events.WaitAsync();
        try
        {
            if (_disposed) return;
            _disposed = true;
            _signaling.Invitation -= OnInvitation;
            _signaling.Accepted -= OnAccepted;
            _signaling.Signal -= OnSignal;
            _signaling.Ended -= OnEnded;
            _signaling.Interrupted -= OnInterrupted;
            Platform.PlaybackSuspending -= OnSuspending;
            try { await StopAsync("Appel arrete.", true); }
            finally { _lifetime.Dispose(); }
        }
        finally { _events.Release(); }
    }
}
