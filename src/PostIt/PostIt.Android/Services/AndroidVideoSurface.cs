using System;
using Avalonia.Android;
using Avalonia.Controls;
using Avalonia.Platform;
using LibVLCSharp.Shared;
using VideoView = LibVLCSharp.Platforms.Android.VideoView;

namespace PostIt.Android.Services;

internal sealed class AndroidVideoSurface(MediaPlayer player) : NativeControlHost, IDisposable
{
    private VideoView? _video;
    private bool _disposed;

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        var activity = MainActivity.Current
            ?? throw new InvalidOperationException("L'activite Android est indisponible.");
        var video = new VideoView(activity) { MediaPlayer = _disposed ? null : player };
        _video = video;
        return new AndroidViewControlHandle(video);
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (control is AndroidViewControlHandle { View: VideoView video })
            video.MediaPlayer = null;
        _video = null;
        base.DestroyNativeControlCore(control);
    }

    public void Dispose()
    {
        _disposed = true;
        if (_video is not null) _video.MediaPlayer = null;
    }
}
