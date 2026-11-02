using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using LibVLCSharp.Shared;
using Yavsc.Abstract.Chat.Calls;

namespace PostIt.Services;

public sealed class LibVlcLanCallMedia : IAsyncDisposable
{
    private readonly LibVLC _senderVlc;
    private readonly LibVLC _receiverVlc;
    private readonly Media _receiverMedia;
    private Media? _senderMedia;
    private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly SemaphoreSlim _gate = new(1);
    private bool _receiving;
    private bool _disposed;
    private Task? _stop;

    public LanCallEndpoint Endpoint { get; }
    public MediaPlayer Sender { get; }
    public MediaPlayer Receiver { get; }
    public Task Completion => _completion.Task;
    public MediaStats Statistics => _receiverMedia.Statistics;
    public bool HasRemoteMedia => !_disposed && Statistics.DecodedAudio > 0 && Statistics.DecodedVideo > 0;

    public LibVlcLanCallMedia(string localAddress, int port, bool headless = false)
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsAndroidVersionAtLeast(26))
            throw new PlatformNotSupportedException("Les appels SRTP LAN requierent Linux ou Android 8+.");
        Endpoint = new(localAddress, port, Convert.ToHexString(RandomNumberGenerator.GetBytes(16)),
            Convert.ToHexString(RandomNumberGenerator.GetBytes(14)));
        if (!Endpoint.IsValid())
            throw new ArgumentException("Adresse IPv4/IPv6 unicast et port UDP pair (1024-65534) requis.");
        var localIp = IPAddress.Parse(localAddress);
        using (var reservation = new Socket(localIp.AddressFamily, SocketType.Dgram, ProtocolType.Udp))
        using (var rtcp = new Socket(localIp.AddressFamily, SocketType.Dgram, ProtocolType.Udp))
        {
            reservation.Bind(new IPEndPoint(IPAddress.Parse(localAddress), port));
            rtcp.Bind(new IPEndPoint(IPAddress.Parse(localAddress), port + 1));
        }
        Core.Initialize();
        _senderVlc = headless ? CameraStreamer.CreateHeadlessLibVlc()
            : new LibVLC("--avcodec-hw=none", "--aout=dummy", "--intf=dummy");
        try
        {
            _receiverVlc = headless ? CameraStreamer.CreateHeadlessLibVlc()
                : new LibVLC("--avcodec-hw=none", "--intf=dummy");
            try
            {
                var host = localIp.AddressFamily == AddressFamily.InterNetworkV6 ? $"[{localAddress}]" : localAddress;
                _receiverMedia = new Media(_receiverVlc, $"rtp://@{host}:{port}", FromType.FromLocation);
                _receiverMedia.AddOption(":network-caching=150");
                _receiverMedia.AddOption(":rtcp-port=" + (port + 1));
                _receiverMedia.AddOption(":srtp-key=" + Endpoint.Key);
                _receiverMedia.AddOption(":srtp-salt=" + Endpoint.Salt);
                Receiver = new MediaPlayer(_receiverVlc);
                Sender = new MediaPlayer(_senderVlc);
                Sender.EncounteredError += OnSenderError;
                Receiver.EncounteredError += OnReceiverError;
                Sender.EndReached += OnSenderEnded;
            }
            catch
            {
                _receiverMedia?.Dispose();
                Receiver?.Dispose();
                Sender?.Dispose();
                _receiverVlc.Dispose();
                throw;
            }
        }
        catch
        {
            _senderVlc.Dispose();
            throw;
        }
    }

    public async Task StartReceivingAsync(CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_stop != null) throw new InvalidOperationException("Cet appel est arrete.");
            if (_receiving) return;
            ct.ThrowIfCancellationRequested();
            if (!Receiver.Play(_receiverMedia))
                throw new InvalidOperationException("Reception SRTP impossible. Verifiez adresse et port UDP.");
            _receiving = true;
        }
        finally { _gate.Release(); }
    }

    public Task StartSendingAsync(LanCallEndpoint remote, string camera, string microphone, CancellationToken ct) =>
        StartSendingAsync(remote, vlc => new CameraStreamer(vlc).CreateCaptureMedia(camera, microphone), true, ct);

    internal async Task StartSendingAsync(LanCallEndpoint remote, Func<LibVLC, Media> source,
        bool preview, CancellationToken ct, bool transcode = true)
    {
        if (!remote.IsValid())
            throw new ArgumentException("Destination SRTP LAN invalide.", nameof(remote));
        if (remote.Address == Endpoint.Address && remote.Port == Endpoint.Port)
            throw new ArgumentException("Les deux pairs doivent avoir des ports distincts sur une meme adresse.");
        await _gate.WaitAsync(ct);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_stop != null || _senderMedia != null)
                throw new InvalidOperationException("La capture de cet appel est deja demarree ou arretee.");
            _senderMedia = source(_senderVlc);
            ConfigureOutput(_senderMedia, remote, preview, transcode);
            ct.ThrowIfCancellationRequested();
            if (!Sender.Play(_senderMedia))
                throw new InvalidOperationException("Capture SRTP impossible. Verifiez camera, microphone et encodeurs H.264/AAC.");
        }
        finally { _gate.Release(); }
    }

    public Task StartEncodedSendingAsync(LanCallEndpoint remote, int fileDescriptor, CancellationToken ct)
    {
        if (fileDescriptor < 0) throw new ArgumentOutOfRangeException(nameof(fileDescriptor));
        return StartSendingAsync(remote, vlc =>
        {
            var media = new Media(vlc, $"fd://{fileDescriptor}", FromType.FromLocation);
            media.AddOption(":demux=ts");
            media.AddOption(":live-caching=100");
            media.AddOption(":clock-jitter=0");
            return media;
        }, false, ct, transcode: false);
    }

    internal static void ConfigureOutput(Media media, LanCallEndpoint remote, bool preview, bool transcode = true)
    {
        if (!remote.IsValid()) throw new ArgumentException("Destination SRTP invalide.");
        // Same capture/encoders as Podcasts, but no HLS files, segmentation or upload.
        var encoding = (transcode ? "transcode{vcodec=h264,venc=x264{preset=ultrafast,tune=zerolatency,keyint=30,bframes=0},"
            + "vb=1200,fps=30,acodec=mp4a,ab=96,channels=1,samplerate=48000}:" : "")
            // Android LibVLC 3 crashes formatting the valueless rtcp-mux SDP attribute.
            + $"rtp{{dst={remote.Address},port={remote.Port},mux=ts,"
            + $"key={remote.Key},salt={remote.Salt},caching=50}}";
        media.AddOption(":sout=#" + (preview
            ? $"duplicate{{dst=display{{noaudio}},dst=\"{encoding}\"}}" : encoding));
    }

    private void OnSenderError(object? sender, EventArgs args) => _completion.TrySetException(
        new InvalidOperationException("Capture/encodage SRTP en echec. Verifiez les peripheriques et encodeurs."));

    private void OnReceiverError(object? sender, EventArgs args) => _completion.TrySetException(
        new InvalidOperationException("Reception/decodage SRTP en echec. Verifiez le reseau LAN/VPN et les ports UDP."));

    private void OnSenderEnded(object? sender, EventArgs args) => _completion.TrySetException(
        new InvalidOperationException("La source audio-video s'est terminee."));

    private Task StopCoreAsync() => _stop ??= Task.Run(() =>
    {
        try { Sender.Stop(); }
        finally { Receiver.Stop(); }
    });

    public async Task StopAsync()
    {
        await _gate.WaitAsync();
        try
        {
            if (!_disposed) await StopCoreAsync();
        }
        finally { _gate.Release(); }
    }

    public async ValueTask DisposeAsync()
    {
        await _gate.WaitAsync();
        try
        {
            if (_disposed) return;
            _disposed = true;
            Sender.EncounteredError -= OnSenderError;
            Receiver.EncounteredError -= OnReceiverError;
            Sender.EndReached -= OnSenderEnded;
            try { await StopCoreAsync(); }
            finally
            {
                Sender.Dispose();
                Receiver.Dispose();
                _senderMedia?.Dispose();
                _receiverMedia.Dispose();
                _senderVlc.Dispose();
                _receiverVlc.Dispose();
                _completion.TrySetResult();
            }
        }
        finally { _gate.Release(); }
    }
}
