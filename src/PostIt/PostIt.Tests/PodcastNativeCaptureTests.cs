using System.Net;
using System.Net.Http;
using LibVLCSharp.Shared;
using PostIt.Services;
using Yavsc.Abstract.Streaming;
using Yavsc.Api.Client;

namespace PostIt.Tests;

public sealed class PodcastNativeCaptureTests
{
    public static bool HasFixture => File.Exists(Environment.GetEnvironmentVariable("POSTIT_TEST_MEDIA"));

    [Fact(Skip = "Set POSTIT_TEST_MEDIA to a local video with audio and install LibVLC with H.264/AAC encoders.", SkipUnless = nameof(HasFixture))]
    public async Task LibVLC_produces_complete_H264_AAC_HLS_segments_without_external_encoder()
    {
        var root = Path.Combine(Path.GetTempPath(), $"postit native's hls {Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            Core.Initialize();
            using var vlc = new LibVLC();
            using var media = new Media(vlc, new Uri(Path.GetFullPath(Environment.GetEnvironmentVariable("POSTIT_TEST_MEDIA")!)));
            using var player = new MediaPlayer(vlc);
            PodcastBroadcaster.ConfigureOutput(media, root);
            var ended = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            player.EndReached += (_, _) => ended.TrySetResult();
            player.EncounteredError += (_, _) => ended.TrySetException(new InvalidOperationException("LibVLC encoding failed."));
            Assert.True(player.Play(media));
            await ended.Task.WaitAsync(TimeSpan.FromSeconds(45), TestContext.Current.CancellationToken);
            player.Stop();
            var playlist = await File.ReadAllTextAsync(Path.Combine(root, "index.m3u8"), TestContext.Current.CancellationToken);
            var segments = HlsCapturePlaylist.Parse(playlist);
            Assert.True(segments.Count >= 2, playlist);
            Assert.Equal(0, segments[0].Sequence);
            foreach (var segment in segments)
            {
                var bytes = await File.ReadAllBytesAsync(Path.Combine(root, segment.FileName), TestContext.Current.CancellationToken);
                Assert.True(bytes.Length > 188);
                Assert.Equal(0, bytes.Length % 188);
                Assert.InRange(segment.DurationSeconds, 0.001, 15);
                for (var i = 0; i < bytes.Length; i += 188) Assert.Equal(0x47, bytes[i]);
            }
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact(Skip = "Set POSTIT_TEST_MEDIA to a local video with audio and install LibVLC with H.264/AAC encoders.", SkipUnless = nameof(HasFixture))]
    public async Task Broadcaster_uploads_final_segment_retries_safely_and_replay_decodes_in_LibVLC()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var transport = new CaptureTransport();
        var client = new PodcastApiClient(transport, () => "https://api.example/api/v1/");
        await using (var broadcaster = await PodcastBroadcaster.StartAsync(client, "Native test",
            vlc => new Media(vlc, new Uri(Path.GetFullPath(Environment.GetEnvironmentVariable("POSTIT_TEST_MEDIA")!))), ct))
        {
            await transport.FirstTwo.Task.WaitAsync(TimeSpan.FromSeconds(45), ct);
            var livePlayback = PlayThroughAsync(client, transport.Podcast.Id, live: true, ct);
            await transport.ReadStarted.Task.WaitAsync(TimeSpan.FromSeconds(45), ct);
            await broadcaster.StopAsync().WaitAsync(TimeSpan.FromSeconds(45), ct);
            Assert.True(broadcaster.Completion.IsCompletedSuccessfully);
            await livePlayback;
        }
        Assert.Equal(PodcastState.Completed, transport.Podcast.State);
        Assert.True(transport.Uploaded.Count >= 3);
        Assert.Equal(2, transport.FirstSegmentAttempts);
        await PlayThroughAsync(client, transport.Podcast.Id, live: false, ct);
    }

    private static async Task PlayThroughAsync(PodcastApiClient client, Guid id, bool live, CancellationToken ct)
    {
        using var vlcPlayer = new LibVLC("--vout=dummy", "--aout=dummy", "--no-xlib");
        using var input = new PodcastMediaInput(client, id, live);
        using var replay = new Media(vlcPlayer, input);
        replay.AddOption(":demux=ts");
        using var player = new MediaPlayer(vlcPlayer);
        var ended = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        input.Failed += message => ended.TrySetException(new InvalidOperationException(message));
        player.EndReached += (_, _) => ended.TrySetResult();
        player.EncounteredError += (_, _) => ended.TrySetException(new InvalidOperationException("Replay decode failed."));
        Assert.True(player.Play(replay));
        try
        {
            await ended.Task.WaitAsync(TimeSpan.FromSeconds(45), ct);
            Assert.Contains(replay.Tracks, track => track.TrackType == TrackType.Video && track.Codec == 0x34363268);
            Assert.Contains(replay.Tracks, track => track.TrackType == TrackType.Audio && track.Codec == 0x6134706d);
        }
        finally
        {
            input.Cancel();
            player.Stop();
        }
    }

    private sealed class CaptureTransport : IYavscApiClient
    {
        private readonly object _sync = new();
        public HttpClient Http { get; } = new();
        public Dictionary<int, byte[]> Uploaded { get; } = new();
        private readonly List<PodcastSegmentResponse> _segments = new();
        public TaskCompletionSource FirstTwo { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource ReadStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int FirstSegmentAttempts { get; private set; }
        public PodcastResponse Podcast { get; private set; } = new(Guid.NewGuid(), "Test", "alice", "Alice",
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, PodcastState.Live, 0, 0, 0);

        public Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            lock (_sync)
            {
                object result;
                if (typeof(T) == typeof(PodcastManifestResponse))
                    result = new PodcastManifestResponse(Podcast, _segments.ToArray());
                else
                {
                    if (path.EndsWith("/complete", StringComparison.Ordinal))
                        Podcast = Podcast with { State = PodcastState.Completed };
                    result = Podcast;
                }
                return Task.FromResult((T)result);
            }
        }

        public async Task<T> CallAsync<T>(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default)
        {
            var uri = new Uri(path);
            var sequence = int.Parse(uri.Segments[^1], System.Globalization.CultureInfo.InvariantCulture);
            var duration = double.Parse(uri.Query["?duration=".Length..], System.Globalization.CultureInfo.InvariantCulture);
            using var content = contentFactory();
            var bytes = await content.ReadAsByteArrayAsync(ct);
            if (sequence >= 2) await ReadStarted.Task.WaitAsync(ct);
            lock (_sync)
            {
                if (Uploaded.TryGetValue(sequence, out var existing))
                    Assert.Equal(existing, bytes);
                else
                {
                    Assert.Equal(_segments.Count, sequence);
                    Uploaded.Add(sequence, bytes);
                    _segments.Add(new(sequence, duration, bytes.Length));
                    Podcast = Podcast with { SegmentCount = _segments.Count, Bytes = Podcast.Bytes + bytes.Length };
                }
                if (sequence == 0 && ++FirstSegmentAttempts == 1)
                    throw new HttpRequestException("Acknowledgement lost", null, HttpStatusCode.ServiceUnavailable);
                if (Uploaded.Count >= 2) FirstTwo.TrySetResult();
                return (T)(object)Podcast;
            }
        }

        public Task CallAsync(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
            => CallAsync<PodcastResponse>(method, path, body, ct);
        public Task CallAsync(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default)
            => CallAsync<PodcastResponse>(method, path, contentFactory, ct);
        public Task<byte[]> DownloadAsync(HttpMethod method, string path, CancellationToken ct = default)
        {
            var sequence = int.Parse(Path.GetFileNameWithoutExtension(new Uri(path).AbsolutePath),
                System.Globalization.CultureInfo.InvariantCulture);
            lock (_sync)
            {
                ReadStarted.TrySetResult();
                return Task.FromResult(Uploaded[sequence]);
            }
        }
        public ValueTask DisposeAsync() { Http.Dispose(); return ValueTask.CompletedTask; }
    }
}
