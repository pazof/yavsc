using System;
using System.Threading.Tasks;
using LibVLCSharp.Shared;

namespace PostIt.Services;

internal sealed class LibVlcPodcastCapture : IPodcastCapture
{
    private readonly LibVLC _vlc;
    private readonly Media _media;
    private readonly MediaPlayer _player;
    private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private bool _stopped;

    public Task Completion => _completion.Task;

    public LibVlcPodcastCapture(Func<LibVLC, Media> createMedia, string directory)
    {
        Core.Initialize();
        _vlc = CameraStreamer.CreatePreferredLibVlc();
        try
        {
            _media = createMedia(_vlc);
            PodcastBroadcaster.ConfigureOutput(_media, directory);
            _player = new MediaPlayer(_vlc);
            _player.EncounteredError += OnError;
            if (!_player.Play(_media))
                throw new InvalidOperationException("LibVLC ne peut pas demarrer la capture. Verifiez la camera et les encodeurs H.264/AAC.");
        }
        catch
        {
            _player?.Dispose();
            _media?.Dispose();
            _vlc.Dispose();
            throw;
        }
    }

    private void OnError(object? sender, EventArgs e)
        => _completion.TrySetException(new InvalidOperationException(
            "La capture LibVLC a echoue. Verifiez camera, microphone et encodeurs H.264/AAC."));

    public async Task StopAsync()
    {
        if (_stopped) return;
        _stopped = true;
        await Task.Run(_player.Stop);
        _completion.TrySetResult();
    }

    public async ValueTask DisposeAsync()
    {
        try { await StopAsync(); }
        finally
        {
            _player.EncounteredError -= OnError;
            _player.Dispose();
            _media.Dispose();
            _vlc.Dispose();
        }
    }
}
