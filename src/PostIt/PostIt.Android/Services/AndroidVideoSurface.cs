using System;
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

internal sealed class AndroidVideoSurface(MediaPlayer player, bool replay) : NativeControlHost, IDisposable
{
    private VideoView? _video;
    private ReplayLayout? _layout;
    private bool _disposed;
    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    internal Task Ready => _ready.Task;

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        var activity = MainActivity.Current
            ?? throw new InvalidOperationException("L'activite Android est indisponible.");
        var video = new VideoView(activity) { MediaPlayer = _disposed ? null : player };
        _video = video;
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (!_disposed) _ready.TrySetResult();
        }, Avalonia.Threading.DispatcherPriority.Loaded);
        if (replay && !_disposed)
        {
            _layout = new ReplayLayout(activity, video, player);
            return new AndroidViewControlHandle(_layout);
        }
        return new AndroidViewControlHandle(video);
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        _layout?.Stop();
        _layout = null;
        if (_video is not null) _video.MediaPlayer = null;
        _video = null;
        base.DestroyNativeControlCore(control);
    }

    public void Dispose()
    {
        _disposed = true;
        _ready.TrySetCanceled();
        _layout?.Stop();
        if (_video is not null) _video.MediaPlayer = null;
    }

    private sealed class ReplayLayout : FrameLayout
    {
        private readonly MediaPlayer _player;
        private readonly ReplayControlsState _state = new();
        private readonly Handler _handler = new(Looper.MainLooper!);
        private readonly Java.Lang.Runnable _tick;
        private readonly LinearLayout _bar;
        private readonly Button _toggle;
        private readonly SeekBar _seek;
        private readonly TextView _time;
        private bool _stopped;

        public ReplayLayout(Context context, VideoView video, MediaPlayer player) : base(context)
        {
            _player = player;
            AddView(video, new FrameLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent));
            _bar = new LinearLayout(context) { Orientation = Orientation.Horizontal };
            _bar.SetGravity(GravityFlags.CenterVertical);
            _bar.SetPadding(12, 8, 12, 8);
            _bar.SetBackgroundColor(global::Android.Graphics.Color.Argb(210, 20, 20, 20));
            _toggle = new Button(context) { Text = "Pause", ContentDescription = "Lecture ou pause du replay" };
            _seek = new SeekBar(context) { Max = 10000, ContentDescription = "Position dans le replay" };
            _time = new TextView(context);
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
            else _player.Play();
            _state.Show();
        }

        private void BeginSeek(object? sender, SeekBar.StartTrackingTouchEventArgs e) => _state.IsInteracting = true;
        private void EndSeek(object? sender, SeekBar.StopTrackingTouchEventArgs e)
        {
            Seek();
            _state.IsInteracting = false;
        }
        private void ProgressChanged(object? sender, SeekBar.ProgressChangedEventArgs e)
        {
            if (e.FromUser && !_state.IsInteracting) Seek();
        }
        private void Seek()
        {
            if (_player.IsSeekable) _player.Time = (long)(_player.Length * (_seek.Progress / 10000d));
            _state.Show();
        }

        public void Stop()
        {
            if (_stopped) return;
            _stopped = true;
            _handler.RemoveCallbacks(_tick);
            _toggle.Click -= Toggle;
            _seek.StartTrackingTouch -= BeginSeek;
            _seek.StopTrackingTouch -= EndSeek;
            _seek.ProgressChanged -= ProgressChanged;
            _tick.Dispose();
            _handler.Dispose();
        }
    }
}
