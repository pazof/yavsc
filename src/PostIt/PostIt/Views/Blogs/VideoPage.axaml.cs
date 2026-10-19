using System;
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

    public VideoPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigatedToEventArgs e)
    {
        base.OnNavigatedTo(e);
        Platform.PlaybackSuspending += SuspendPlayback;
        Platform.PlaybackResuming += ResumePlayback;
        InitializePlayer();
    }

    private void InitializePlayer()
    {
        ReleasePlayer();
        if (Design.IsDesignMode || DataContext is not VideoPageViewModel vm)
            return;
        try
        {
            _libVlc = new LibVLC();
            _player = new MediaPlayer(_libVlc);
            _player.EncounteredError += Player_EncounteredError;
            if (vm.Podcast is { } podcast && vm.PodcastClient is { } client)
            {
                _podcastInput = new PodcastMediaInput(client, podcast.Id, podcast.State == PodcastState.Live);
                _podcastInput.Failed += PodcastFailed;
            }
            using var media = _podcastInput is null
                ? new Media(_libVlc, vm.Source)
                : new Media(_libVlc, _podcastInput);
            if (_podcastInput is not null)
                media.AddOption(":demux=ts");
            var surface = Platform.CreateVideoSurface?.Invoke(_player)
                ?? new LibVLCSharp.Avalonia.VideoView { MediaPlayer = _player };
            VideoSurface.Content = surface;
            var player = _player;
            // Navigation transforms do not change Bounds, so refresh the native surface after layout.
            Dispatcher.UIThread.Post(() =>
            {
                if (ReferenceEquals(_player, player) && surface is LibVLCSharp.Avalonia.VideoView desktopVideo)
                    desktopVideo.TryUpdateNativeControlPosition();
            }, DispatcherPriority.Loaded);
            PlayButton.IsEnabled = true;
            PlaybackStatus.Text = vm.MimeType;
            if (!_player.Play(media))
                PlaybackStatus.Text = vm.Podcast is null
                    ? "Impossible de démarrer la vidéo. Vous pouvez l'ouvrir dans le navigateur."
                    : "Impossible de demarrer la lecture du podcast.";
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or BadImageFormatException or VLCException)
        {
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
        PlayButton.IsEnabled = false;
        _podcastInput?.Cancel();
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
    }

    private void SuspendPlayback()
    {
        ReleasePlayer();
        PlaybackStatus.Text = "Lecture suspendue.";
    }

    private void ResumePlayback() => InitializePlayer();

    private void PodcastFailed(string message) => Dispatcher.UIThread.Post(() => PlaybackStatus.Text = message);

    private static bool CanLaunchExternally(Uri source)
        => source is { IsAbsoluteUri: true }
            && (string.Equals(source.Scheme, "http", StringComparison.OrdinalIgnoreCase)
                || string.Equals(source.Scheme, "https", StringComparison.OrdinalIgnoreCase))
            && !string.Equals(source.Host, "localhost", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(source.AbsolutePath, "/", StringComparison.OrdinalIgnoreCase)
            && !source.AbsolutePath.Contains("/api/", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(source.Scheme, "about", StringComparison.OrdinalIgnoreCase);

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
            if (!CanLaunchExternally(vm.Source))
            {
                PlaybackStatus.Text = "Ce flux interne ne peut pas être ouvert dans le navigateur.";
                return;
            }

            var topLevel = TopLevel.GetTopLevel(this)
                ?? throw new InvalidOperationException("La fenêtre de lecture est indisponible.");
            if (!await topLevel.Launcher.LaunchUriAsync(vm.Source))
                PlaybackStatus.Text = "Impossible d'ouvrir le navigateur.";
        }
        catch (Exception ex)
        {
            PlaybackStatus.Text = $"Impossible d'ouvrir le navigateur : {ex.Message}";
        }
    }
}
