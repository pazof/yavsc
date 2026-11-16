using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Android;
using Avalonia.Controls;
using Avalonia.Platform;
using LibVLCSharp.Shared;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using PostIt.Services;
using VideoView = LibVLCSharp.Platforms.Android.VideoView;
using Button = Android.Widget.Button;

namespace PostIt.Android.Services;

internal sealed class AndroidVideoSurface(MediaPlayer player, bool replay, bool showControls = false, Action? playbackFailed = null)
    : NativeControlHost, IDisposable, IVideoFrameCapture
{
    private VideoView? _video;
    private ReplayLayout? _layout;
    private bool _disposed;
    private bool _playbackSubscribed;
    private IDisposable? _screenAwake;
    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    internal Task Ready => _ready.Task;

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        var activity = MainActivity.Current
            ?? throw new InvalidOperationException("L'activite Android est indisponible.");
        var video = new VideoView(activity) { MediaPlayer = _disposed ? null : player };
        _video = video;
        if (!_disposed)
        {
            player.Playing += OnPlaybackChanged;
            player.Paused += OnPlaybackChanged;
            player.Stopped += OnPlaybackChanged;
            player.EndReached += OnPlaybackChanged;
            player.EncounteredError += OnPlaybackChanged;
            _playbackSubscribed = true;
        }
        UpdateScreenAwake();
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (!_disposed) _ready.TrySetResult();
        }, Avalonia.Threading.DispatcherPriority.Loaded);
        if ((replay || showControls) && !_disposed)
        {
            _layout = new ReplayLayout(activity, video, player, replay, playbackFailed);
            return new AndroidViewControlHandle(_layout);
        }
        return new AndroidViewControlHandle(video);
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        ReleaseScreenAwake();
        _layout?.Stop();
        _layout = null;
        if (_video is not null) _video.MediaPlayer = null;
        _video = null;
        base.DestroyNativeControlCore(control);
    }

    public void Dispose()
    {
        _disposed = true;
        ReleaseScreenAwake();
        _ready.TrySetCanceled();
        _layout?.Stop();
        if (_video is not null) _video.MediaPlayer = null;
    }

    /// <summary>Copies the displayed frame with <see cref="PixelCopy"/>, which also reads hardware-decoded video.</summary>
    public async Task<Avalonia.Media.Imaging.Bitmap?> CaptureFrameAsync(int width, CancellationToken ct)
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(24) || _disposed
            || _video is not { Width: > 0, Height: > 0, Holder.Surface.IsValid: true } video) return null;
        var height = Math.Max(1, (int)Math.Round(width * (double)video.Height / video.Width));
        using var bitmap = global::Android.Graphics.Bitmap.CreateBitmap(width, height, global::Android.Graphics.Bitmap.Config.Argb8888!);
        using var listener = new PixelCopyListener();
        using var registration = ct.Register(() => listener.Result.TrySetCanceled(ct));
        using var handler = new Handler(Looper.MainLooper!);
        PixelCopy.Request(video, bitmap, listener, handler);
        if (await listener.Result.Task != (int)PixelCopyResult.Success) return null;
        using var png = new MemoryStream();
        if (!bitmap.Compress(global::Android.Graphics.Bitmap.CompressFormat.Png!, 100, png)) return null;
        png.Position = 0;
        return new Avalonia.Media.Imaging.Bitmap(png);
    }

    private sealed class PixelCopyListener : Java.Lang.Object, PixelCopy.IOnPixelCopyFinishedListener
    {
        public TaskCompletionSource<int> Result { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public void OnPixelCopyFinished(int copyResult) => Result.TrySetResult(copyResult);
    }

    private void OnPlaybackChanged(object? sender, EventArgs e)
        => Avalonia.Threading.Dispatcher.UIThread.Post(UpdateScreenAwake);

    private void UpdateScreenAwake()
    {
        if (!_disposed && _video is not null && player.IsPlaying)
            _screenAwake ??= Platform.ScreenAwake.Acquire();
        else
        {
            _screenAwake?.Dispose();
            _screenAwake = null;
        }
    }

    private void ReleaseScreenAwake()
    {
        if (_playbackSubscribed)
        {
            player.Playing -= OnPlaybackChanged;
            player.Paused -= OnPlaybackChanged;
            player.Stopped -= OnPlaybackChanged;
            player.EndReached -= OnPlaybackChanged;
            player.EncounteredError -= OnPlaybackChanged;
            _playbackSubscribed = false;
        }
        _screenAwake?.Dispose();
        _screenAwake = null;
    }

    private sealed class ReplayLayout : FrameLayout
    {
        private readonly MediaPlayer _player;
        private readonly ReplayControlsState _state = new();
        private readonly Handler _handler = new(Looper.MainLooper!);
        private readonly Java.Lang.Runnable _tick;
        private readonly Java.Lang.Runnable _preview;
        private readonly LinearLayout _bar;
        private readonly Button _toggle;
        private readonly SeekBar _seek;
        private readonly TextView _time;
        private readonly Action? _playbackFailed;
        private bool _stopped;

        public ReplayLayout(Context context, VideoView video, MediaPlayer player, bool replay, Action? playbackFailed) : base(context)
        {
            _player = player;
            _playbackFailed = playbackFailed;
            AddView(video, new FrameLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent));
            _bar = new LinearLayout(context) { Orientation = Orientation.Horizontal };
            _bar.SetGravity(GravityFlags.CenterVertical);
            _bar.SetPadding(12, 8, 12, 8);
            _bar.SetBackgroundColor(global::Android.Graphics.Color.Argb(210, 20, 20, 20));
            _toggle = new Button(context) { Text = "Pause", ContentDescription = "Lecture ou pause de la vidéo" };
            _seek = new SeekBar(context) { Max = 10000, ContentDescription = "Position dans le replay" };
            _time = new TextView(context);
            _seek.Visibility = replay ? ViewStates.Visible : ViewStates.Gone;
            _time.Visibility = replay ? ViewStates.Visible : ViewStates.Gone;
            _time.SetTextColor(global::Android.Graphics.Color.White);
            _bar.AddView(_toggle);
            _bar.AddView(_seek, new LinearLayout.LayoutParams(0, LayoutParams.WrapContent, 1));
            _bar.AddView(_time);
            AddView(_bar, new FrameLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent, GravityFlags.Bottom));
            _toggle.Click += Toggle;
            _seek.StartTrackingTouch += BeginSeek;
            _seek.StopTrackingTouch += EndSeek;
            _seek.ProgressChanged += ProgressChanged;
            _tick = new Java.Lang.Runnable(Update);
            _preview = new Java.Lang.Runnable(ApplyPreview);
            _handler.Post(_tick);
        }

        public override bool DispatchTouchEvent(MotionEvent? e)
        {
            _state.Show();
            _bar.Visibility = ViewStates.Visible;
            return base.DispatchTouchEvent(e);
        }

        private void Update()
        {
            if (_stopped) return;
            var duration = Math.Max(0, _player.Length);
            var position = Math.Max(0, _player.Time);
            _bar.Visibility = _state.IsVisible(_player.IsPlaying, _bar.HasFocus) ? ViewStates.Visible : ViewStates.Gone;
            _toggle.Text = _player.IsPlaying ? "Pause" : "Lire";
            _seek.Enabled = duration > 0 && _player.IsSeekable;
            if (!_state.IsInteracting)
                _seek.Progress = duration > 0 ? (int)Math.Clamp(position * 10000d / duration, 0, 10000) : 0;
            _time.Text = $"{ReplayControlsState.FormatTime(_state.IsInteracting ? (long)(duration * (_seek.Progress / 10000d)) : position)} / {ReplayControlsState.FormatTime(duration)}";
            _handler.PostDelayed(_tick, 250);
        }

        private void Toggle(object? sender, EventArgs e)
        {
            if (_player.IsPlaying) _player.Pause();
            else if (!_player.Play()) _playbackFailed?.Invoke();
            _state.Show();
        }

        private void BeginSeek(object? sender, SeekBar.StartTrackingTouchEventArgs e)
        {
            _state.IsInteracting = true;
            _handler.PostDelayed(_preview, 60);
        }
        private void EndSeek(object? sender, SeekBar.StopTrackingTouchEventArgs e)
        {
            _handler.RemoveCallbacks(_preview);
            _state.EndPreview();
            Seek();
            _state.IsInteracting = false;
        }
        private void ProgressChanged(object? sender, SeekBar.ProgressChangedEventArgs e)
        {
            if (!e.FromUser) return;
            if (_state.IsInteracting) _state.RequestPreview(PositionOf(_seek.Progress));
            else Seek();
        }

        /// <summary>Shows the frame under the thumb while dragging: only the latest position is sent, at most every 60 ms.</summary>
        private void ApplyPreview()
        {
            if (_stopped || !_state.IsInteracting) return;
            if (_player.IsSeekable && _state.TryTakePreview(out var time)) _player.Time = time;
            _handler.PostDelayed(_preview, 60);
        }

        private long PositionOf(int progress) => (long)(_player.Length * (progress / 10000d));
        private void Seek()
        {
            if (_player.IsSeekable) _player.Time = PositionOf(_seek.Progress);
            _state.Show();
        }

        public void Stop()
        {
            if (_stopped) return;
            _stopped = true;
            _handler.RemoveCallbacks(_tick);
            _handler.RemoveCallbacks(_preview);
            _toggle.Click -= Toggle;
            _seek.StartTrackingTouch -= BeginSeek;
            _seek.StopTrackingTouch -= EndSeek;
            _seek.ProgressChanged -= ProgressChanged;
            _tick.Dispose();
            _preview.Dispose();
            _handler.Dispose();
        }
    }
}
