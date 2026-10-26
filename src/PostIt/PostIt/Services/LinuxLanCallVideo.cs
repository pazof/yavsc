using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using LibVLCSharp.Avalonia;
using Yavsc.Abstract.Chat.Calls;

namespace PostIt.Services;

public interface ILanCallMedia : IAsyncDisposable
{
    Control LocalVideo { get; }
    Control RemoteVideo { get; }
    LanCallEndpoint Endpoint { get; }
    Task Completion { get; }
    bool HasRemoteMedia { get; }
    Task StartSendingAsync(LanCallEndpoint remote, string camera, string microphone, CancellationToken ct);
}

public sealed class LinuxLanCallVideo : ILanCallMedia
{
    private readonly LinuxLanCallMedia _media;
    private readonly VideoView _local;
    private readonly VideoView _remote;
    private readonly TaskCompletionSource _localReady = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly CancellationTokenSource _lifetime = new();
    private bool _receiverScheduled;
    private bool _localScheduled;
    private bool _disposed;
    private readonly TaskCompletionSource _failure = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Control LocalVideo => _local;
    public Control RemoteVideo => _remote;
    public LanCallEndpoint Endpoint => _media.Endpoint;
    public Task Completion { get; }
    public bool HasRemoteMedia => !_disposed && _media.HasRemoteMedia;

    public LinuxLanCallVideo(string localAddress, int port)
    {
        _media = new LinuxLanCallMedia(localAddress, port);
        _local = new VideoView { MediaPlayer = _media.Sender, MinHeight = 140 };
        _remote = new VideoView { MediaPlayer = _media.Receiver, MinHeight = 260 };
        _local.LayoutUpdated += OnLocalLayout;
        _remote.LayoutUpdated += OnRemoteLayout;
        Completion = ObserveCompletionAsync();
    }

    private async Task ObserveCompletionAsync()
    {
        var completed = await Task.WhenAny(_media.Completion, _failure.Task);
        await completed;
    }

    private void OnLocalLayout(object? sender, EventArgs e)
    {
        if (_localScheduled || _disposed || !_local.IsEffectivelyVisible ||
            _local.Bounds.Width <= 0 || _local.Bounds.Height <= 0) return;
        _localScheduled = true;
        Dispatcher.UIThread.Post(() =>
        {
            if (_disposed) return;
            _local.TryUpdateNativeControlPosition();
            _localReady.TrySetResult();
        }, DispatcherPriority.Loaded);
    }

    private void OnRemoteLayout(object? sender, EventArgs e)
    {
        if (_receiverScheduled || _disposed || !_remote.IsEffectivelyVisible ||
            _remote.Bounds.Width <= 0 || _remote.Bounds.Height <= 0) return;
        _receiverScheduled = true;
        Dispatcher.UIThread.Post(StartReceiver, DispatcherPriority.Loaded);
    }

    private async void StartReceiver()
    {
        if (_disposed) return;
        try
        {
            _remote.TryUpdateNativeControlPosition();
            await _media.StartReceivingAsync(_lifetime.Token);
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception error) { _failure.TrySetException(error); }
    }

    public async Task StartSendingAsync(LanCallEndpoint remote, string camera, string microphone, CancellationToken ct)
    {
        await _localReady.Task.WaitAsync(TimeSpan.FromSeconds(10), ct);
        await _media.StartSendingAsync(remote, camera, microphone, ct);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        _lifetime.Cancel();
        _localReady.TrySetCanceled();
        _local.LayoutUpdated -= OnLocalLayout;
        _remote.LayoutUpdated -= OnRemoteLayout;
        try { await _media.StopAsync(); }
        finally
        {
            _local.MediaPlayer = null;
            _remote.MediaPlayer = null;
            try { await _media.DisposeAsync(); }
            finally
            {
                _lifetime.Dispose();
                _failure.TrySetResult();
            }
        }
    }
}
