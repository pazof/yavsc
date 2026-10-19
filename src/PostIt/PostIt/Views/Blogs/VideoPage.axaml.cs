using System;
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
    private Media? _media;
    private PodcastMediaInput? _podcastInput;
    private bool _externalFallbackTriggered;

    public VideoPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigatedToEventArgs e)
    {
        base.OnNavigatedTo(e);
        ReleasePlayer();
        if (DataContext is not VideoPageViewModel vm)
            return;
        try
        {
            Core.Initialize();
            _libVlc = CameraStreamer.CreatePlaybackLibVlc();
            _player = new MediaPlayer(_libVlc);
            _player.EncounteredError += Player_EncounteredError;
            if (vm.Podcast is { } podcast && vm.PodcastClient is { } client)
            {
                _podcastInput = new PodcastMediaInput(client, podcast.Id, podcast.State == PodcastState.Live);
                _podcastInput.Failed += PodcastFailed;
                _media = new Media(_libVlc, _podcastInput);
                _media.AddOption(":demux=ts");
            }
            else
                _media = new Media(_libVlc, vm.Source);
            Video.MediaPlayer = _player;
            PlayButton.IsEnabled = true;
            PlaybackStatus.Text = vm.MimeType;
            if (!_player.Play(_media))
                PlaybackStatus.Text = vm.Podcast is null
                    ? "Impossible de démarrer la vidéo. Vous pouvez l'ouvrir dans le navigateur."
                    : "Impossible de demarrer la lecture du podcast.";
        }
        catch (Exception ex) when (ex is VLCException or DllNotFoundException or EntryPointNotFoundException
            or BadImageFormatException)
        {
            ReleasePlayer();
            _externalFallbackTriggered = true;
            PlaybackStatus.Text = $"Lecture vidéo indisponible : {ex.Message}. Ouverture du navigateur pour afficher le média.";
            _ = OpenExternalFallbackAsync(vm.Source);
        }
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs e)
    {
        ReleasePlayer();
        base.OnNavigatedFrom(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ReleasePlayer();
        base.OnDetachedFromVisualTree(e);
    }

    private void ReleasePlayer()
    {
        PlayButton.IsEnabled = false;
        Video.MediaPlayer = null;
        _podcastInput?.Cancel();
        if (_player is not null)
        {
            _player.EncounteredError -= Player_EncounteredError;
            _player.Stop();
            _player.Dispose();
            _player = null;
        }
        _media?.Dispose();
        _media = null;
        if (_podcastInput is not null)
        {
            _podcastInput.Failed -= PodcastFailed;
            _podcastInput.Dispose();
            _podcastInput = null;
        }
        _libVlc?.Dispose();
        _libVlc = null;
    }

    private void PodcastFailed(string message) => Dispatcher.UIThread.Post(() => PlaybackStatus.Text = message);

    private static bool CanLaunchExternally(Uri source)
        => source is { IsAbsoluteUri: true }
            && !string.Equals(source.Scheme, "about", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(source.Host, "localhost", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(source.AbsolutePath, "/", StringComparison.OrdinalIgnoreCase);

    private async Task OpenExternalFallbackAsync(Uri source)
    {
        try
        {
            if (!CanLaunchExternally(source))
            {
                PlaybackStatus.Text = "Lecture intégrée indisponible ; ce flux ne peut pas être ouvert dans le navigateur.";
                return;
            }

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is null)
            {
                PlaybackStatus.Text = "Lecture intégrée indisponible ; ouvrez le média dans le navigateur.";
                return;
            }

            if (await topLevel.Launcher.LaunchUriAsync(source))
                PlaybackStatus.Text = "Lecture intégrée indisponible ; le média a été ouvert dans le navigateur.";
            else
                PlaybackStatus.Text = "Lecture intégrée indisponible ; ouvrez le média dans le navigateur.";
        }
        catch (Exception ex)
        {
            PlaybackStatus.Text = $"Lecture intégrée indisponible : {ex.Message}. Ouvrez le média dans le navigateur.";
        }
    }

    private void Player_EncounteredError(object? sender, EventArgs e)
        => Dispatcher.UIThread.Post(() =>
        {
            if (ReferenceEquals(sender, _player) && !_externalFallbackTriggered)
            {
                _externalFallbackTriggered = true;
                if (DataContext is VideoPageViewModel vm)
                    _ = OpenExternalFallbackAsync(vm.Source);
                PlaybackStatus.Text = _podcastInput is null
                    ? "Impossible de lire cette vidéo. Ouverture du navigateur…"
                    : "Impossible de lire le podcast. Ouverture du navigateur…";
            }
        });

    private void PlayPause_Click(object? sender, RoutedEventArgs e)
    {
        if (_player is null || _media is null)
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
