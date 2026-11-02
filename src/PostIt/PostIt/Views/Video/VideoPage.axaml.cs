using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LibVLCSharp.Shared;
using PostIt.ViewModels;
using PostIt.Services;
using Yavsc.Abstract.Streaming;

namespace PostIt.Views.Blogs;

public partial class VideoPage : ContentPage
{
    private LibVLC? _libVlc;
    private MediaPlayer? _player;
    private PodcastMediaInput? _podcastInput;
    private PodcastReplayMediaInput? _replayInput;
    private CancellationTokenSource? _initialization;

    public VideoPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigatedToEventArgs e)
    {
        base.OnNavigatedTo(e);
        Platform.PlaybackSuspending += SuspendPlayback;
        Platform.PlaybackResuming += ResumePlayback;
        InitializePlayer();
    }

    private async void InitializePlayer()
    {
        ReleasePlayer();
        if (Design.IsDesignMode || DataContext is not VideoPageViewModel vm)
            return;
        var cancellation = new CancellationTokenSource();
        _initialization = cancellation;
        var ct = cancellation.Token;
        try
        {
            var replay = false;
            var podcast = vm.Podcast;
            if (podcast is not null && vm.PodcastClient is { } replayClient)
            {
                while (true)
                {
                    var manifest = await replayClient.GetAsync(podcast.Id, ct);
                    ct.ThrowIfCancellationRequested();
                    podcast = manifest.Podcast;
                    if (podcast.State != PodcastState.Completed) break;
                    PlaybackStatus.Text = "Preparation du replay MP4...";
                    if (manifest.Podcast.Replay is { State: PodcastReplayState.Ready, Bytes: > 0 } ready)
                    {
                        _replayInput = new PodcastReplayMediaInput(replayClient, podcast.Id, ready.Bytes);
                        _replayInput.Failed += PodcastFailed;
                        replay = true;
                        break;
                    }
                    if (manifest.Podcast.Replay is { State: PodcastReplayState.Failed } failed)
                    {
                        PlaybackStatus.Text = failed.Error ?? "Preparation du MP4 en echec.";
                        return;
                    }
                    await Task.Delay(1000, ct);
                }
            }
            ct.ThrowIfCancellationRequested();
            _libVlc = new LibVLC();
            _player = new MediaPlayer(_libVlc);
            _player.EncounteredError += Player_EncounteredError;
            if (!replay && podcast is { } retained && vm.PodcastClient is { } client)
            {
                _podcastInput = new PodcastMediaInput(client, retained.Id, retained.State == PodcastState.Live);
                _podcastInput.Failed += PodcastFailed;
            }
            using var media = _replayInput is not null ? new Media(_libVlc, _replayInput)
                : _podcastInput is not null ? new Media(_libVlc, _podcastInput)
                : new Media(_libVlc, vm.Source);
            if (_podcastInput is not null)
                media.AddOption(":demux=ts");
            var surface = Platform.CreateVideoSurface?.Invoke(_player, replay)
                ?? new LibVLCSharp.Avalonia.VideoView
                {
                    MediaPlayer = _player,
                    Content = replay ? new PostIt.Controls.ReplayControls(_player) : null
                };
            VideoSurface.Content = surface;
            var player = _player;
            // Navigation transforms do not change Bounds, so refresh the native surface after layout.
            Dispatcher.UIThread.Post(() =>
            {
                if (ReferenceEquals(_player, player) && surface is LibVLCSharp.Avalonia.VideoView desktopVideo)
                    desktopVideo.TryUpdateNativeControlPosition();
            }, DispatcherPriority.Loaded);
            PlayButton.IsEnabled = true;
            PlaybackStatus.Text = replay ? "video/mp4" : vm.MimeType;
            if (!_player.Play(media))
                PlaybackStatus.Text = vm.Podcast is null
                    ? "Impossible de démarrer la vidéo. Vous pouvez l'ouvrir dans le navigateur."
                    : "Impossible de demarrer la lecture du podcast.";
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception ex)
        {
            if (!ReferenceEquals(_initialization, cancellation)) return;
            ReleasePlayer();
            PlaybackStatus.Text = $"Lecture vidéo indisponible : {ex.Message}";
        }
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs e)
    {
        Platform.PlaybackSuspending -= SuspendPlayback;
        Platform.PlaybackResuming -= ResumePlayback;
        ReleasePlayer();
        base.OnNavigatedFrom(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Platform.PlaybackSuspending -= SuspendPlayback;
        Platform.PlaybackResuming -= ResumePlayback;
        ReleasePlayer();
        base.OnDetachedFromVisualTree(e);
    }

    private void ReleasePlayer()
    {
        _initialization?.Cancel();
        _initialization?.Dispose();
        _initialization = null;
        PlayButton.IsEnabled = false;
        _podcastInput?.Cancel();
        _replayInput?.Cancel();
        if (VideoSurface.Content is LibVLCSharp.Avalonia.VideoView desktopVideo)
            desktopVideo.MediaPlayer = null;
        if (VideoSurface.Content is IDisposable nativeSurface)
            nativeSurface.Dispose();
        VideoSurface.Content = null;
        if (_player is not null)
        {
            _player.EncounteredError -= Player_EncounteredError;
            _player.Stop();
            _player.Dispose();
            _player = null;
        }
        if (_podcastInput is not null)
        {
            _podcastInput.Failed -= PodcastFailed;
            _podcastInput.Dispose();
            _podcastInput = null;
        }
        _libVlc?.Dispose();
        _libVlc = null;
        if (_replayInput is not null)
        {
            _replayInput.Failed -= PodcastFailed;
            _replayInput.Dispose();
            _replayInput = null;
        }
    }

    private void SuspendPlayback()
    {
        ReleasePlayer();
        PlaybackStatus.Text = "Lecture suspendue.";
    }

    private void ResumePlayback() => InitializePlayer();

    private void PodcastFailed(string message) => Dispatcher.UIThread.Post(() => PlaybackStatus.Text = message);

    private void Player_EncounteredError(object? sender, EventArgs e)
        => Dispatcher.UIThread.Post(() =>
        {
            if (!ReferenceEquals(sender, _player) || _player is null)
                return;

            PlaybackStatus.Text = _podcastInput is null
                ? "Impossible de lire cette vidéo."
                : "Impossible de lire le podcast.";
        });

    private void PlayPause_Click(object? sender, RoutedEventArgs e)
    {
        if (_player is null)
            return;
        if (_player.IsPlaying)
            _player.Pause();
        else if (!_player.Play())
            PlaybackStatus.Text = "Impossible de reprendre la vidéo.";
    }

    private async void OpenExternal_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not VideoPageViewModel vm)
            return;
        try
        {
            if (!vm.CanOpenExternal)
            {
                PlaybackStatus.Text = "Cette ressource ne peut pas être ouverte dans le navigateur.";
                return;
            }

            var topLevel = TopLevel.GetTopLevel(this)
                ?? throw new InvalidOperationException("La fenêtre de lecture est indisponible.");
            if (!await topLevel.Launcher.LaunchUriAsync(vm.ExternalUri))
                PlaybackStatus.Text = "Impossible d'ouvrir le navigateur.";
        }
        catch (Exception ex)
        {
            PlaybackStatus.Text = $"Impossible d'ouvrir le navigateur : {ex.Message}";
        }
    }
}
