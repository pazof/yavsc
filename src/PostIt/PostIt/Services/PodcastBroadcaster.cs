using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using LibVLCSharp.Shared;
using Yavsc.Abstract.Streaming;
using Yavsc.Api.Client;

namespace PostIt.Services;

public sealed class PodcastBroadcaster : IAsyncDisposable
{
    private readonly PodcastApiClient _api;
    private readonly IPodcastCapture _capture;
    private readonly string _directory;
    private readonly CancellationTokenSource _cancellation = new();
    private readonly SemaphoreSlim _stopGate = new(1);
    private volatile bool _stopped;
    private bool _disposed;

    public PodcastResponse Podcast { get; private set; }
    public Task Completion { get; private set; } = Task.CompletedTask;
    public event Action<PodcastResponse>? Progress;

    private PodcastBroadcaster(PodcastApiClient api, PodcastResponse podcast,
        IPodcastCapture capture, string directory)
    {
        _api = api;
        Podcast = podcast;
        _capture = capture;
        _directory = directory;
    }

    public static void ConfigureOutput(Media media, string directory)
    {
        var path = directory.Replace('\\', '/').Replace("\"", "\\\"");
        media.AddOption(":sout=#transcode{vcodec=h264,venc=x264{preset=ultrafast,tune=zerolatency,keyint=60},vb=1200,fps=30,"
            + "acodec=mp4a,ab=128,channels=2,samplerate=48000}:std{"
            + $"access=livehttp{{seglen=2,delsegs=false,numsegs=0,initial-segment-number=0,index=\"{path}/index.m3u8\","
            + $"index-url=segment-########.ts}},mux=ts{{use-key-frames}},dst=\"{path}/segment-########.ts\"}}");
    }

    public static async Task<PodcastBroadcaster> StartAsync(PodcastApiClient api, string title,
        string camera, string? microphone, CancellationToken ct)
        => await StartAsync(api, title, vlc => new CameraStreamer(vlc).CreateCaptureMedia(camera, microphone), ct);

    internal static async Task<PodcastBroadcaster> StartAsync(PodcastApiClient api, string title,
        Func<LibVLC, Media> createMedia, CancellationToken ct)
        => await StartAsync(api, title,
            (directory, _) => Task.FromResult<IPodcastCapture>(new LibVlcPodcastCapture(createMedia, directory)), ct);

    public static async Task<PodcastBroadcaster> StartAsync(PodcastApiClient api, string title,
        Func<string, CancellationToken, Task<IPodcastCapture>> createCapture, CancellationToken ct)
    {
        IPodcastCapture? capture = null;
        PodcastBroadcaster? broadcaster = null;
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PostIt", "podcast-capture", Guid.NewGuid().ToString("N"));
        PodcastResponse? podcast = null;
        try
        {
            Directory.CreateDirectory(directory);
            podcast = await api.CreateAsync(title, ct);
            await File.WriteAllTextAsync(Path.Combine(directory, "podcast.json"), JsonSerializer.Serialize(podcast), ct);
            ct.ThrowIfCancellationRequested();
            capture = await createCapture(directory, ct);
            ct.ThrowIfCancellationRequested();
            broadcaster = new PodcastBroadcaster(api, podcast, capture, directory);
            broadcaster.Completion = broadcaster.UploadAsync(broadcaster._cancellation.Token);
            return broadcaster;
        }
        catch (Exception startupError)
        {
            if (broadcaster is not null)
            {
                broadcaster._cancellation.Dispose();
                broadcaster._stopGate.Dispose();
            }
            if (capture is not null) await capture.DisposeAsync();
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
            if (podcast is not null)
            {
                try
                {
                    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                    await api.CompleteAsync(podcast.Id, timeout.Token);
                }
                catch (Exception finishError)
                {
                    throw new AggregateException("Capture impossible ; le direct serveur expirera automatiquement.", startupError, finishError);
                }
            }
            throw;
        }
    }

    public async Task StopAsync()
    {
        await StopEncoderAsync();
        await Completion;
    }

    private async Task StopEncoderAsync()
    {
        await _stopGate.WaitAsync();
        try
        {
            if (!_stopped)
            {
                await _capture.StopAsync();
                _stopped = true;
            }
        }
        finally { _stopGate.Release(); }
    }

    private async Task UploadAsync(CancellationToken ct)
    {
        var next = 0;
        var lastSegment = Stopwatch.StartNew();
        try
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                var finalPass = _stopped || _capture.Completion.IsCompleted;
                if (_capture.Completion.IsCompleted)
                    await _capture.Completion;
                var pendingBytes = Directory.EnumerateFiles(_directory, "*.ts").Sum(path => new FileInfo(path).Length);
                if (pendingBytes > 64L * 1024 * 1024)
                    throw new IOException("Le reseau est trop lent : la capture depasse 64 Mio de segments en attente.");
                var playlist = Path.Combine(_directory, "index.m3u8");
                if (File.Exists(playlist))
                {
                    using var reader = new StreamReader(new FileStream(playlist, FileMode.Open,
                        FileAccess.Read, FileShare.ReadWrite | FileShare.Delete));
                    var segments = HlsCapturePlaylist.Parse(await reader.ReadToEndAsync(ct));
                    foreach (var segment in segments.Where(s => s.Sequence >= next))
                    {
                        if (segment.Sequence != next)
                            throw new InvalidDataException("Un segment manque dans la capture.");
                        var path = Path.Combine(_directory, segment.FileName);
                        if (new FileInfo(path).Length > 8 * 1024 * 1024)
                            throw new IOException("Un segment de capture depasse 8 Mio.");
                        var bytes = await File.ReadAllBytesAsync(path, ct);
                        var response = await UploadWithRetryAsync(segment, bytes, ct);
                        Podcast = response;
                        File.Delete(path);
                        next++;
                        lastSegment.Restart();
                        Progress?.Invoke(response);
                    }
                }
                if (finalPass)
                {
                    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    timeout.CancelAfter(TimeSpan.FromSeconds(20));
                    Podcast = await _api.CompleteAsync(Podcast.Id, timeout.Token);
                    return;
                }
                if (lastSegment.Elapsed > TimeSpan.FromSeconds(25))
                    throw new IOException("Aucun segment produit depuis 25 secondes. Verifiez la camera et les encodeurs H.264/AAC.");
                await Task.Delay(400, ct);
            }
        }
        finally
        {
            // Stop the camera even when uploading or parsing fails.
            await StopEncoderAsync();
        }
    }

    private async Task<PodcastResponse> UploadWithRetryAsync(CaptureSegment segment, byte[] bytes, CancellationToken ct)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeout.CancelAfter(TimeSpan.FromSeconds(10));
                return await _api.UploadAsync(Podcast.Id, segment.Sequence, segment.DurationSeconds, bytes, timeout.Token);
            }
            catch (HttpRequestException ex) when (attempt < 2 && (ex.StatusCode is null
                or HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests || (int)ex.StatusCode >= 500))
            {
                await Task.Delay(TimeSpan.FromSeconds(attempt + 1), ct);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested && attempt < 2)
            {
                await Task.Delay(TimeSpan.FromSeconds(attempt + 1), ct);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        _cancellation.Cancel();
        try { await Completion; }
        finally
        {
            await _capture.DisposeAsync();
            _cancellation.Dispose();
            _stopGate.Dispose();
            // Keep failed, unacknowledged segments for manual recovery.
            if (Completion.IsCompletedSuccessfully && Directory.Exists(_directory))
                Directory.Delete(_directory, recursive: true);
        }
    }
}
