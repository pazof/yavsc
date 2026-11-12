using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Services;
using Yavsc.Abstract.Chat.Calls;

namespace PostIt.ViewModels.Chat;

public partial class LanCallViewModel : ObservableObject, IAsyncDisposable
{
    private readonly ILanCallSignaling _signaling;
    private readonly Func<string, int, ILanCallMedia> _createMedia;
    private readonly SemaphoreSlim _events = new(1);
    private readonly CancellationTokenSource _lifetime = new();
    private CancellationTokenSource? _callLifetime;
    private ILanCallMedia? _media;
    private CallSession? _call;
    private bool _disposed;
    private bool _accepting;
    private bool _endpointReceived;
    private string? _activeCamera;
    private string? _activeMicrophone;
    private string? _activeAddress;
    private int _activePort;
    private readonly Settings? _settings;
    private bool _restoringPreferences;

    public ObservableCollection<string> Addresses { get; } = [];
    public ObservableCollection<CaptureDevice> Cameras { get; } = [];
    public ObservableCollection<CaptureDevice> Microphones { get; } = [];

    [ObservableProperty] public partial string? Address { get; set; }
    [ObservableProperty] public partial string Port { get; set; } = "5004";
    [ObservableProperty] public partial CaptureDevice? Camera { get; set; }
    [ObservableProperty] public partial CaptureDevice? Microphone { get; set; }
    [ObservableProperty] public partial bool AllowAddressDisclosure { get; set; }
    [ObservableProperty] public partial string Target { get; set; } = "";
    [ObservableProperty] public partial string Room { get; set; } = "";
    [ObservableProperty] public partial string Status { get; set; } = "Appel LAN/VPN : aucun appel.";
    [ObservableProperty, NotifyPropertyChangedFor(nameof(CanConfigure))]
    public partial bool Incoming { get; set; }
    [ObservableProperty, NotifyPropertyChangedFor(nameof(CanConfigure))]
    public partial bool Busy { get; set; }
    [ObservableProperty] public partial bool Preparing { get; set; }
    [ObservableProperty] public partial Control? LocalVideo { get; set; }
    [ObservableProperty] public partial Control? RemoteVideo { get; set; }
    public bool CanConfigure => !Busy || Incoming;

    public LanCallViewModel(ILanCallSignaling signaling, Func<string, int, ILanCallMedia>? createMedia = null,
        Settings? settings = null)
    {
        _signaling = signaling;
        _settings = settings;
        _createMedia = createMedia ?? Platform.CreateLanCallMedia ??
            ((address, port) => new LinuxLanCallVideo(address, port));
        signaling.Invitation += OnInvitation;
        signaling.Accepted += OnAccepted;
        signaling.Signal += OnSignal;
        signaling.Ended += OnEnded;
        signaling.Interrupted += OnInterrupted;
        Platform.PlaybackSuspending += OnSuspending;
        if (settings != null)
        {
            _restoringPreferences = true;
            try
            {
                foreach (var device in settings.PodcastCameras) Cameras.Add(device);
                foreach (var device in settings.PodcastMicrophones.Where(d => !string.IsNullOrEmpty(d.Value)))
                    Microphones.Add(device);
                RestoreDevices();
                RefreshAddresses();
            }
            catch (NetworkInformationException ex) { Status = $"Adresses reseau indisponibles : {ex.Message}"; }
            finally { _restoringPreferences = false; }
            settings.PropertyChanged += OnSettingsChanged;
        }
    }

    private void OnSettingsChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args)
    {
        if (_restoringPreferences || !CanConfigure) return;
        if (args.PropertyName is not (nameof(Settings.PodcastCameraId) or nameof(Settings.PodcastMicrophoneId)
            or nameof(Settings.PodcastCameras) or nameof(Settings.PodcastMicrophones))) return;
        _restoringPreferences = true;
        try
        {
            Cameras.Clear();
            Microphones.Clear();
            foreach (var device in _settings!.PodcastCameras) Cameras.Add(device);
            foreach (var device in _settings.PodcastMicrophones.Where(d => !string.IsNullOrEmpty(d.Value))) Microphones.Add(device);
            RestoreDevices();
        }
        finally { _restoringPreferences = false; }
    }

    private void RestoreDevices()
    {
        Camera = Cameras.FirstOrDefault(d => d.Value == _settings?.PodcastCameraId) ?? Cameras.FirstOrDefault();
        Microphone = _settings?.PodcastMicrophoneId == "" ? null
            : Microphones.FirstOrDefault(d => d.Value == _settings?.PodcastMicrophoneId) ?? Microphones.FirstOrDefault();
    }

    private void RefreshAddresses()
    {
        var preferred = _settings?.LanCallAddress ?? Address;
        Addresses.Clear();
        foreach (var address in NetworkInterface.GetAllNetworkInterfaces()
            .Where(n => n.OperationalStatus == OperationalStatus.Up)
            .SelectMany(n => n.GetIPProperties().UnicastAddresses)
            .Select(a => a.Address.ToString()).Where(LanCallNetwork.IsUnicastAddress)
            .Distinct().OrderBy(a => IPAddress.IsLoopback(IPAddress.Parse(a))))
            Addresses.Add(address);
        Address = preferred == null ? Addresses.FirstOrDefault() : Addresses.FirstOrDefault(a => a == preferred);
    }

    partial void OnCameraChanged(CaptureDevice? value)
    {
        if (_restoringPreferences || _settings == null || value == null) return;
        _settings.PodcastCameraId = value.Value;
        SavePreferences();
    }

    partial void OnMicrophoneChanged(CaptureDevice? value)
    {
        if (_restoringPreferences || _settings == null || value == null) return;
        _settings.PodcastMicrophoneId = value.Value;
        SavePreferences();
    }

    partial void OnAddressChanged(string? value)
    {
        if (_restoringPreferences || _settings == null || value == null) return;
        _settings.LanCallAddress = value;
        SavePreferences();
    }

    private void SavePreferences()
    {
        try { _settings?.Save(); }
        catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException)
        {
            Status = $"Preferences d'appel appliquees, mais sauvegarde impossible : {ex.Message}";
        }
    }

    [RelayCommand]
    private void RefreshDevices()
    {
        _restoringPreferences = true;
        try
        {
            if (!CanConfigure) throw new InvalidOperationException("Terminez l'appel avant de changer les peripheriques.");
            RefreshAddresses();
            Cameras.Clear();
            Microphones.Clear();
            if (Platform.GetLanCallDevices is { } getDevices)
            {
                var devices = getDevices();
                foreach (var device in devices.Cameras) Cameras.Add(device);
                foreach (var device in devices.Microphones.Where(d => !string.IsNullOrEmpty(d.Value)))
                    Microphones.Add(device);
            }
            else
            {
                LibVLCSharp.Shared.Core.Initialize();
                using var vlc = CameraStreamer.CreatePreferredLibVlc();
                var streamer = new CameraStreamer(vlc);
                foreach (var device in streamer.GetCaptureDevices()) Cameras.Add(new(device.Key, device.Value));
                foreach (var device in streamer.GetAudioCaptureDevices()) Microphones.Add(new(device.Key, device.Value));
            }
            RestoreDevices();
            Status = Cameras.Count == 0 ? "Aucune camera detectee. Verifiez les peripheriques puis actualisez."
                : Incoming && _call != null
                    ? $"{_call.Caller} propose un appel dans {_call.RoomName}. Selectionnez vos peripheriques puis acceptez ou refusez."
                : "Selectionnez le pair, l'adresse IPv4/IPv6 locale ou publique et les peripheriques. Utilisez un casque.";
            if (_settings != null)
            {
                _settings.PodcastCameras = Cameras.ToArray();
                var noMicrophone = _settings.PodcastMicrophones.Where(d => string.IsNullOrEmpty(d.Value));
                _settings.PodcastMicrophones = noMicrophone.Concat(Microphones).ToArray();
                if (Camera != null) _settings.PodcastCameraId = Camera.Value;
                if (Microphone != null) _settings.PodcastMicrophoneId = Microphone.Value;
                if (Address != null) _settings.LanCallAddress = Address;
                SavePreferences();
            }
        }
        catch (Exception error) { Status = "Peripheriques indisponibles : " + error.Message; }
        finally { _restoringPreferences = false; }
    }

    private void ValidateSelection()
    {
        if (!AllowAddressDisclosure)
            throw new InvalidOperationException("Confirmez le partage de votre adresse IP avec le pair.");
        if (Camera == null || Microphone == null)
            throw new InvalidOperationException("Une camera et un microphone sont requis.");
        if (Address == null || !Addresses.Contains(Address) || !LanCallNetwork.IsUnicastAddress(Address) ||
            !int.TryParse(Port, out var port) || port is < 1024 or > 65534 || port % 2 != 0)
            throw new InvalidOperationException("Selectionnez une adresse locale et un port UDP pair (1024-65534).");
        _activeCamera = Camera.Value;
        _activeMicrophone = Microphone.Value;
        _activeAddress = Address;
        _activePort = port;
    }

    [RelayCommand]
    private Task InviteAsync() => RunAsync(async () =>
    {
        if (Busy) throw new InvalidOperationException("Un appel est deja en cours.");
        ValidateSelection();
        if (string.IsNullOrWhiteSpace(Target) || string.IsNullOrWhiteSpace(Room))
            throw new InvalidOperationException("Selectionnez une salle jointe et un autre utilisateur.");
        Busy = true;
        _call = await _signaling.InviteLanAsync(Room, Target.Trim(), _lifetime.Token);
        Status = $"Invitation envoyee a {_call.Callee} ; attente du consentement.";
    });

    [RelayCommand]
    private Task AcceptAsync() => RunAsync(async () =>
    {
        if (_call == null || !Incoming)
            throw new InvalidOperationException("Aucune invitation a accepter.");
        ValidateSelection();
        Incoming = false;
        _accepting = true;
        await _signaling.AcceptAsync(_call.Id, _lifetime.Token);
        Status = "Invitation acceptee ; preparation des medias.";
    });

    [RelayCommand]
    public Task HangupAsync()
    {
        _callLifetime?.Cancel();
        return RunAsync(() => StopAsync("Appel termine.", true));
    }

    private void OnInvitation(CallSession call)
    {
        if (call.Transport != CallTransports.LanSrtp) return;
        Queue(async () =>
        {
            if (_call != null) throw new InvalidOperationException("Invitation concurrente inattendue.");
            if (call.RelayOnly) throw new InvalidOperationException("Le mode LAN ne peut pas masquer les adresses IP.");
            _call = call;
            Room = call.RoomName;
            Target = call.Caller;
            Busy = true;
            Incoming = true;
            Status = $"{call.Caller} propose un appel SRTP dans {call.RoomName}. Acceptez ou refusez.";
            await Task.CompletedTask;
        });
    }

    private void OnAccepted(CallAccepted accepted) => Queue(async () =>
    {
        if (_call?.Id != accepted.Session.Id) return;
        if (_media != null) throw new InvalidOperationException("Acceptation repetee.");
        if (accepted.Session.Transport != CallTransports.LanSrtp || accepted.Session.RelayOnly ||
            (Incoming && !_accepting))
            throw new InvalidOperationException("Acceptation ou politique de transport inattendue.");
        _call = accepted.Session;
        Incoming = false;
        _accepting = false;
        _callLifetime = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token);
        _media = _createMedia(_activeAddress!, _activePort);
        LocalVideo = _media.LocalVideo;
        RemoteVideo = _media.RemoteVideo;
        Status = "Connexion SRTP : attente du pair (30 secondes maximum).";
        await _signaling.SendAsync(_call.Id, new("rtp-endpoint", Endpoint: _media.Endpoint), _callLifetime.Token);
        _ = MonitorAsync(_call.Id, _media, _callLifetime.Token);
    });

    private void OnSignal(Guid id, CallSignal signal) => Queue(async () =>
    {
        if (_call?.Id != id) return;
        if (_media == null || _callLifetime == null || signal.Kind != "rtp-endpoint" ||
            signal.Endpoint?.IsValid() != true || _endpointReceived)
            throw new InvalidOperationException("Destination SRTP manquante, invalide ou repetee.");
        _endpointReceived = true;
        await _media.StartSendingAsync(signal.Endpoint, _activeCamera!, _activeMicrophone!, _callLifetime.Token);
        Status = "Emission SRTP demarree ; verification de la reception audio-video.";
    });

    private void OnEnded(CallEnded ended) => QueueStop(ended.CallId, async () =>
    {
        if (_call?.Id == ended.CallId)
            await StopAsync("Appel termine : " + ended.Reason, false);
    });

    private void OnInterrupted() => QueueStop(null,
        () => StopAsync("Connexion SignalR interrompue ; medias arretes.", false));

    private void OnSuspending() => QueueStop(null,
        () => StopAsync("Application en arriere-plan ; appel termine.", true));

    private void QueueStop(Guid? id, Func<Task> action) => Dispatcher.UIThread.Post(async () =>
    {
        if (id == null || _call?.Id == id) _callLifetime?.Cancel();
        await RunAsync(action);
    });

    private async Task MonitorAsync(Guid id, ILanCallMedia media, CancellationToken ct)
    {
        var start = Stopwatch.StartNew();
        var received = false;
        try
        {
            while (!ct.IsCancellationRequested)
            {
                if (media.Completion.IsCompleted) await media.Completion;
                if (!received && media.HasRemoteMedia)
                {
                    received = true;
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (_call?.Id == id) Status = "Reception audio-video SRTP etablie (LAN/VPN).";
                    });
                }
                if (!received && start.Elapsed > TimeSpan.FromSeconds(30))
                    throw new TimeoutException("Aucun media distant : verifiez adresses, port UDP et pare-feu LAN/VPN.");
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

    private void Queue(Func<Task> action) =>
        Dispatcher.UIThread.Post(async () => await RunAsync(action));

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
        finally
        {
            Preparing = false;
            _events.Release();
        }
    }

    private async Task FailAsync(Exception error)
    {
        try { await StopAsync("Echec de l'appel : " + error.Message, true); }
        catch (Exception cleanupError)
        {
            Status = $"Echec de l'appel : {error.Message}. Nettoyage/signalisation : {cleanupError.Message}";
        }
    }

    private async Task StopAsync(string status, bool notify)
    {
        var call = _call;
        _call = null;
        Incoming = false;
        Busy = false;
        _accepting = false;
        _endpointReceived = false;
        _callLifetime?.Cancel();
        var media = _media;
        _media = null;
        try
        {
            if (media != null) await media.DisposeAsync();
        }
        finally
        {
            LocalVideo = null;
            RemoteVideo = null;
            _callLifetime?.Dispose();
            _callLifetime = null;
            Status = status;
        }
        if (notify && call != null)
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await _signaling.EndAsync(call.Id, timeout.Token);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _lifetime.Cancel();
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
            if (_settings != null) _settings.PropertyChanged -= OnSettingsChanged;
            try { await StopAsync("Appel arrete.", true); }
            finally { _lifetime.Dispose(); }
        }
        finally { _events.Release(); }
    }
}
