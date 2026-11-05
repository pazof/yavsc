using System;
using System.Threading.Tasks;
using LibVLCSharp.Shared;
using Avalonia.Controls;
using Avalonia.Threading;

namespace PostIt.Services;

internal sealed class LibVlcPodcastCapture : IPodcastCapture
{
    private readonly LibVLC _vlc;
    private readonly Media _media;
    private readonly MediaPlayer _player;
    private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private bool _stopped;
    private bool _startScheduled;
    private readonly LibVLCSharp.Avalonia.VideoView? _preview;

    public Task Completion => _completion.Task;
    public Control? Preview => _preview;

    public LibVlcPodcastCapture(Func<LibVLC, Media> createMedia, string? directory, bool preview = false)
    {
        Core.Initialize();
        // The usual Linux capture instance has a dummy video output; preview needs a drawable.
        _vlc = preview ? new LibVLC("--avcodec-hw=none", "--aout=dummy") : CameraStreamer.CreatePreferredLibVlc();
        try
        {
            _media = createMedia(_vlc);
            if (directory is not null) PodcastBroadcaster.ConfigureOutput(_media, directory, preview);
            else _media.AddOption(":no-audio");
            _player = new MediaPlayer(_vlc);
            if (preview)
            {
                _preview = new LibVLCSharp.Avalonia.VideoView { MediaPlayer = _player };
                _preview.LayoutUpdated += OnPreviewLayout;
            }
            _player.EncounteredError += OnError;
            if (!preview) StartPlayer();
        }
        catch
        {
            if (_preview is not null)
            {
                _preview.LayoutUpdated -= OnPreviewLayout;
                _preview.MediaPlayer = null;
            }
            _player?.Dispose();
            _media?.Dispose();
            _vlc.Dispose();
            throw;
        }
    }

    private void StartPlayer()
    {
        if (!_player.Play(_media))
            throw new InvalidOperationException("LibVLC ne peut pas demarrer la capture. Verifiez la camera et les encodeurs H.264/AAC.");
    }

    private void OnPreviewLayout(object? sender, EventArgs e)
    {
        if (_startScheduled || _stopped || _preview is not { IsEffectivelyVisible: true } view
            || view.Bounds.Width <= 0 || view.Bounds.Height <= 0) return;
        _startScheduled = true;
        view.LayoutUpdated -= OnPreviewLayout;
        // LibVLC must receive its native drawable before the display branch starts.
        Dispatcher.UIThread.Post(() =>
        {
            if (_stopped) return;
            try
            {
                view.TryUpdateNativeControlPosition();
                StartPlayer();
            }
            catch (Exception ex) { _completion.TrySetException(ex); }
        }, DispatcherPriority.Loaded);
    }

    private void OnError(object? sender, EventArgs e)
        => _completion.TrySetException(new InvalidOperationException(
            "La capture LibVLC a echoue. Verifiez camera, microphone et encodeurs H.264/AAC."));

    public async Task StopAsync()
    {
        if (_preview is not null)
        {
            var shouldStop = await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (_stopped) return false;
                _stopped = true;
                _preview.LayoutUpdated -= OnPreviewLayout;
                return true;
            });
            if (!shouldStop) return;
        }
        else
        {
            if (_stopped) return;
            _stopped = true;
        }
        await Task.Run(_player.Stop);
        _completion.TrySetResult();
    }

    public async ValueTask DisposeAsync()
    {
        try { await StopAsync(); }
        finally
        {
            if (_preview is not null)
                await Dispatcher.UIThread.InvokeAsync(() => _preview.MediaPlayer = null);
            _player.EncounteredError -= OnError;
            _player.Dispose();
            _media.Dispose();
            _vlc.Dispose();
        }
    }
}
