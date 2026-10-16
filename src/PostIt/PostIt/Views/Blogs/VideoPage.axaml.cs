using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LibVLCSharp.Shared;
using PostIt.ViewModels;

namespace PostIt.Views.Blogs;

public partial class VideoPage : ContentPage
{
    private LibVLC? _libVlc;
    private MediaPlayer? _player;
    private Media? _media;

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
            _libVlc = new LibVLC();
            _player = new MediaPlayer(_libVlc);
            _player.EncounteredError += Player_EncounteredError;
            _media = new Media(_libVlc, vm.Source);
            Video.MediaPlayer = _player;
            PlayButton.IsEnabled = true;
            PlaybackStatus.Text = vm.MimeType;
            if (!_player.Play(_media))
                PlaybackStatus.Text = "Impossible de démarrer la vidéo. Vous pouvez l'ouvrir dans le navigateur.";
        }
        catch (Exception ex) when (ex is VLCException or DllNotFoundException or EntryPointNotFoundException
            or BadImageFormatException or PlatformNotSupportedException)
        {
            ReleasePlayer();
            PlaybackStatus.Text = $"Lecture vidéo indisponible : {ex.Message}. Vérifiez l'installation de LibVLC.";
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
        if (_player is not null)
        {
            _player.EncounteredError -= Player_EncounteredError;
            _player.Stop();
            _player.Dispose();
            _player = null;
        }
        _media?.Dispose();
        _media = null;
        _libVlc?.Dispose();
        _libVlc = null;
    }

    private void Player_EncounteredError(object? sender, EventArgs e)
        => Dispatcher.UIThread.Post(() =>
        {
            if (ReferenceEquals(sender, _player))
                PlaybackStatus.Text = "Impossible de lire cette vidéo. Vous pouvez l'ouvrir dans le navigateur.";
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
