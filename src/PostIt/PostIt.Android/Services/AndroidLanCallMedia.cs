using System;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using PostIt.Services;
using Yavsc.Abstract.Chat.Calls;

namespace PostIt.Android.Services;

[SupportedOSPlatform("android26.0")]
internal sealed class AndroidLanCallMedia : ILanCallMedia
{
    private readonly LibVlcLanCallMedia _media;
    private readonly ContentControl _local = new();
    private readonly AndroidVideoSurface _remote;
    private readonly CancellationTokenSource _lifetime = new();
    private readonly TaskCompletionSource _captureEnded = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly Task _receiving;
    private IPodcastCapture? _capture;
    private bool _disposed;

    public Control LocalVideo => _local;
    public Control RemoteVideo => _remote;
    public LanCallEndpoint Endpoint => _media.Endpoint;
    public bool HasRemoteMedia => !_disposed && _media.HasRemoteMedia;
    public Task Completion { get; }

    public static PodcastCaptureDevices GetDevices()
    {
        var devices = AndroidPodcastCapture.GetDevices();
        return new(devices.Cameras.OrderByDescending(c => c.Name == "Camera avant").ToArray(),
            devices.Microphones.Where(m => m.Value == "microphone").ToArray());
    }

    public AndroidLanCallMedia(string localAddress, int port)
    {
        _media = new LibVlcLanCallMedia(localAddress, port);
        _remote = new AndroidVideoSurface(_media.Receiver, replay: false);
        _receiving = ReceiveAsync();
        Completion = ObserveAsync();
    }

    private async Task ReceiveAsync()
    {
        await _remote.Ready.WaitAsync(TimeSpan.FromSeconds(10), _lifetime.Token);
        await _media.StartReceivingAsync(_lifetime.Token);
    }

    private async Task ObserveAsync()
    {
        var first = await Task.WhenAny(_media.Completion, _captureEnded.Task, _receiving);
        await first;
        if (first == _receiving)
        {
            var finished = await Task.WhenAny(_media.Completion, _captureEnded.Task);
            await finished;
        }
    }

    public async Task StartSendingAsync(LanCallEndpoint remote, string camera, string microphone, CancellationToken ct)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_capture != null) throw new InvalidOperationException("La capture de cet appel est deja demarree.");
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, _lifetime.Token);
        _capture = await AndroidPodcastCapture.StartCallAsync(camera, microphone,
            (fd, token) => _media.StartEncodedSendingAsync(remote, fd, token), _media.StopAsync, linked.Token);
        _local.Content = _capture.Preview;
        _ = ObserveCaptureAsync(_capture);
    }

    private async Task ObserveCaptureAsync(IPodcastCapture capture)
    {
        try
        {
            await capture.Completion;
            if (!_disposed)
                _captureEnded.TrySetException(new InvalidOperationException("Capture de l'appel arretee."));
        }
        catch (Exception error) { _captureEnded.TrySetException(error); }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        _lifetime.Cancel();
        var stop = _capture?.StopAsync();
        try
        {
            if (stop != null) await stop;
        }
        finally
        {
            try
            {
                if (_capture != null) await _capture.DisposeAsync();
            }
            finally
            {
                _local.Content = null;
                try { await _media.StopAsync(); }
                finally
                {
                    _remote.Dispose();
                    try { await _media.DisposeAsync(); }
                    finally
                    {
                        _lifetime.Dispose();
                        _captureEnded.TrySetResult();
                    }
                }
            }
        }
    }
}
