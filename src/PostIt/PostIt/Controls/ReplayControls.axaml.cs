using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LibVLCSharp.Shared;
using PostIt.Services;

namespace PostIt.Controls;

public partial class ReplayControls : UserControl
{
    private readonly ReplayControlsState _state = new();
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(250) };
    private MediaPlayer? _player;
    private bool _updating;

    public ReplayControls()
    {
        InitializeComponent();
        _timer.Tick += Update;
        PointerMoved += (_, _) => _state.Show();
        PointerPressed += (_, _) => _state.Show();
        KeyDown += (_, _) => _state.Show();
        PositionSlider.AddHandler(PointerPressedEvent, BeginSeek, RoutingStrategies.Tunnel);
        PositionSlider.AddHandler(PointerReleasedEvent, EndSeek, RoutingStrategies.Bubble, handledEventsToo: true);
        PositionSlider.PointerCaptureLost += (_, _) =>
        {
            if (_state.IsInteracting) CommitSeek();
        };
    }

    public ReplayControls(MediaPlayer player) : this() => _player = player;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _state.Show();
        _timer.Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _timer.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    private void Update(object? sender, EventArgs e)
    {
        if (_player is not { } player) return;
        var duration = Math.Max(0, player.Length);
        var position = Math.Max(0, player.Time);
        ControlBar.IsVisible = _state.IsVisible(player.IsPlaying, ControlBar.IsKeyboardFocusWithin);
        ToggleButton.Content = player.IsPlaying ? "Pause" : "Lire";
        PositionSlider.IsEnabled = duration > 0 && player.IsSeekable;
        _updating = true;
        PositionSlider.Maximum = Math.Max(1, duration);
        if (!_state.IsInteracting) PositionSlider.Value = Math.Min(position, duration);
        _updating = false;
        TimeLabel.Text = $"{ReplayControlsState.FormatTime(_state.IsInteracting ? (long)PositionSlider.Value : position)} / {ReplayControlsState.FormatTime(duration)}";
    }

    private void BeginSeek(object? sender, PointerPressedEventArgs e)
    {
        _state.IsInteracting = true;
        _state.Show();
    }

    private void EndSeek(object? sender, PointerReleasedEventArgs e) => CommitSeek();

    private void CommitSeek()
    {
        if (_player is { IsSeekable: true } player) player.Time = (long)PositionSlider.Value;
        _state.IsInteracting = false;
        _state.Show();
    }

    private void Position_Changed(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (!_updating && !_state.IsInteracting) CommitSeek();
    }

    private void Toggle_Click(object? sender, RoutedEventArgs e)
    {
        if (_player is not { } player) return;
        if (player.IsPlaying) player.Pause();
        else player.Play();
        _state.Show();
    }
}
