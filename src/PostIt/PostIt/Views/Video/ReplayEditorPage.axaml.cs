using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using LibVLCSharp.Shared;
using PostIt.Services;
using PostIt.ViewModels;

namespace PostIt.Views.Blogs;

/// <summary>Plays the owner-only draft source and captures marks for <see cref="ReplayEditorPageViewModel"/>.</summary>
public partial class ReplayEditorPage : ContentPage
{
    private readonly DispatcherTimer _positionTimer = new() { Interval = TimeSpan.FromMilliseconds(100) };
    private LibVLC? _libVlc;
    private MediaPlayer? _player;
    private PodcastReplayMediaInput? _input;
    private ReplayEditorPageViewModel? _vm;
    private string? _snapshotDirectory;
    private Task _snapshot = Task.CompletedTask;
    private bool? _isWideLayout;

    public ReplayEditorPage()
    {
        InitializeComponent();
        if (OperatingSystem.IsAndroid()) AddMarkButton.Content = "Ajouter une marque";
        SizeChanged += (_, _) => UpdateResponsiveLayout();
        _positionTimer.Tick += (_, _) =>
        {
            if (_player is { } player && _vm is { } vm) vm.CurrentTimeMs = Math.Max(0, player.Time);
        };
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs e)
    {
        base.OnNavigatedTo(e);
        UpdateResponsiveLayout();
        InitializePlayer();
    }

    /// <summary>Keeps the video and the edit panel side by side in landscape, and stacks them on a portrait phone.</summary>
    private void UpdateResponsiveLayout()
    {
        var size = TopLevel.GetTopLevel(this)?.ClientSize ?? Bounds.Size;
        if (size.Width <= 0 || size.Height <= 0) return;
        var isWide = size.Width >= size.Height;
        if (_isWideLayout == isWide) return;
        _isWideLayout = isWide;
        EditorGrid.ColumnDefinitions = new ColumnDefinitions(isWide ? "2*,*" : "*");
        // The native video surface stays out of the scroll viewer: only the edit panel scrolls.
        EditorGrid.RowDefinitions = new RowDefinitions(isWide ? "Auto,*,Auto,Auto" : "Auto,2*,Auto,Auto,3*");
        EditorGrid.Margin = new Thickness(isWide ? 16 : 8);
        EditorTitle.FontSize = isWide ? 22 : 18;
        VideoBorder.MinHeight = isWide ? 240 : 160;
        Grid.SetColumnSpan(EditorTitle, isWide ? 2 : 1);
        Grid.SetColumn(EditPanel, isWide ? 1 : 0);
        Grid.SetRow(EditPanel, isWide ? 1 : 4);
        Grid.SetRowSpan(EditPanel, isWide ? 3 : 1);
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs e)
    {
        ReleasePlayer();
        base.OnNavigatedFrom(e);
    }

    protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        ReleasePlayer();
        base.OnDetachedFromVisualTree(e);
    }

    private void InitializePlayer()
    {
        ReleasePlayer();
        if (Design.IsDesignMode || DataContext is not ReplayEditorPageViewModel vm) return;
        try
        {
            if (vm.SourceBytes <= 0) throw new InvalidOperationException("La source du brouillon n'est pas disponible.");
            _vm = vm;
            vm.SeekRequested += Seek;
            vm.CaptureThumbnail = CaptureThumbnailAsync;
            _snapshotDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PostIt", "replay-editor", Guid.NewGuid().ToString("N"));
            _input = new PodcastReplayMediaInput(vm.PodcastClient, vm.Podcast.Id, vm.SourceBytes, draftSource: true);
            _input.Failed += InputFailed;
            _libVlc = new LibVLC();
            _player = new MediaPlayer(_libVlc);
            using var media = new Media(_libVlc, _input);
            var controls = new PostIt.Controls.ReplayControls(_player, replay: true);
            controls.PlaybackFailed += () => PlaybackStatus.Text = "Impossible de reprendre la lecture.";
            var surface = Platform.CreateVideoSurface?.Invoke(_player, true, () => PlaybackStatus.Text = "Impossible de reprendre la lecture.")
                ?? new LibVLCSharp.Avalonia.VideoView { MediaPlayer = _player, Content = controls };
            VideoSurface.Content = surface;
            var player = _player;
            Dispatcher.UIThread.Post(() =>
            {
                if (ReferenceEquals(_player, player) && surface is LibVLCSharp.Avalonia.VideoView desktopVideo)
                    desktopVideo.TryUpdateNativeControlPosition();
            }, DispatcherPriority.Loaded);
            PlaybackStatus.Text = "Source privee du brouillon.";
            if (!_player.Play(media)) PlaybackStatus.Text = "Impossible de lire la source du brouillon.";
            _positionTimer.Start();
        }
        catch (Exception ex)
        {
            ReleasePlayer();
            PlaybackStatus.Text = $"Editeur indisponible : {ex.Message}";
        }
    }

    private void ReleasePlayer()
    {
        _positionTimer.Stop();
        if (_vm is not null)
        {
            _vm.SeekRequested -= Seek;
            _vm.CaptureThumbnail = null;
            _vm.ReleaseThumbnails();
            _vm = null;
        }
        _input?.Cancel();
        // The native snapshot must not run against a disposed player.
        try { _snapshot.Wait(TimeSpan.FromSeconds(2)); }
        catch (AggregateException) { }
        if (VideoSurface.Content is LibVLCSharp.Avalonia.VideoView desktopVideo) desktopVideo.MediaPlayer = null;
        if (VideoSurface.Content is IDisposable nativeSurface) nativeSurface.Dispose();
        VideoSurface.Content = null;
        if (_player is not null)
        {
            _player.Stop();
            _player.Dispose();
            _player = null;
        }
        _libVlc?.Dispose();
        _libVlc = null;
        if (_input is not null)
        {
            _input.Failed -= InputFailed;
            _input.Dispose();
            _input = null;
        }
        if (_snapshotDirectory is not null)
        {
            try { if (Directory.Exists(_snapshotDirectory)) Directory.Delete(_snapshotDirectory, true); }
            catch (IOException) { }
            _snapshotDirectory = null;
        }
    }

    private void InputFailed(string message) => Dispatcher.UIThread.Post(() => PlaybackStatus.Text = message);

    private void Seek(long timeMs)
    {
        if (_player is not { IsSeekable: true } player) return;
        if (player.IsPlaying) player.SetPause(true);
        player.Time = timeMs;
    }

    private void PreviousFrame_Click(object? sender, RoutedEventArgs e)
    {
        if (_player is not { IsSeekable: true } player) return;
        var frameMs = player.Fps > 0 ? 1000.0 / player.Fps : 1000.0 / 30;
        if (player.IsPlaying) player.SetPause(true);
        player.Time = Math.Max(0, player.Time - (long)Math.Ceiling(frameMs));
    }

    private void NextFrame_Click(object? sender, RoutedEventArgs e) => _player?.NextFrame();

    /// <summary>
    /// Copies the frame from a native surface able to do it (Android), otherwise
    /// snapshots the displayed frame into a private temporary file, loads it in memory and deletes the file:
    /// thumbnails of the private source never reach a shared cache.
    /// </summary>
    private async Task<Bitmap?> CaptureThumbnailAsync(CancellationToken ct)
    {
        if (VideoSurface.Content is IVideoFrameCapture surface) return await surface.CaptureFrameAsync(160, ct);
        if (_player is not { } player || _snapshotDirectory is not { } directory) return null;
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, Guid.NewGuid().ToString("N") + ".png");
        try
        {
            var snapshot = Task.Run(() => player.TakeSnapshot(0, path, 160, 0), ct);
            _snapshot = snapshot;
            if (!await snapshot || !File.Exists(path)) return null;
            await using var file = File.OpenRead(path);
            return new Bitmap(file);
        }
        finally { File.Delete(path); }
    }
}
