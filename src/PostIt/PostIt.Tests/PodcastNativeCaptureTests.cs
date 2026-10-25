using System.Net;
using System.Net.Http;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using LibVLCSharp.Shared;
using PostIt.Services;
using Yavsc.Abstract.Streaming;
using Yavsc.Api.Client;

namespace PostIt.Tests;

public sealed class PodcastNativeCaptureTests(ITestOutputHelper output)
{
    public static bool HasFixture => File.Exists(Environment.GetEnvironmentVariable("POSTIT_TEST_MEDIA"));

    [Theory(Skip = "Set POSTIT_TEST_MEDIA to a local video with audio and install LibVLC with H.264/AAC encoders.", SkipUnless = nameof(HasFixture))]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LibVLC_produces_complete_H264_AAC_HLS_segments_without_external_encoder(bool preview)
    {
        var root = Path.Combine(Path.GetTempPath(), $"postit native's hls {Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var pixels = preview ? Marshal.AllocHGlobal(640 * 480 * 4) : IntPtr.Zero;
        try
        {
            Core.Initialize();
            using var vlc = new LibVLC("--vout=dummy", "--aout=dummy", "--no-xlib",
                "--avcodec-hw=none", "--avcodec-threads=2");
            using var media = new Media(vlc, new Uri(Path.GetFullPath(Environment.GetEnvironmentVariable("POSTIT_TEST_MEDIA")!)));
            using var player = new MediaPlayer(vlc);
            var displayed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            if (preview)
            {
                player.SetVideoFormat("RV32", 640, 480, 640 * 4);
                player.SetVideoCallbacks((_, planes) =>
                {
                    Marshal.WriteIntPtr(planes, pixels);
                    return IntPtr.Zero;
                }, null, (_, _) => displayed.TrySetResult());
            }
            PodcastBroadcaster.ConfigureOutput(media, root, preview);
            var ended = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            player.EndReached += (_, _) => ended.TrySetResult();
            player.EncounteredError += (_, _) => ended.TrySetException(new InvalidOperationException("LibVLC encoding failed."));
            Assert.True(player.Play(media));
            output.WriteLine($"Encoding started (preview={preview}).");
            if (preview)
                await displayed.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
            await ended.Task.WaitAsync(TimeSpan.FromSeconds(45), TestContext.Current.CancellationToken);
            output.WriteLine("End reached; stopping player.");
            player.Stop();
            output.WriteLine("Player stopped; checking segments.");
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
            var start = new ProcessStartInfo("ffprobe") { RedirectStandardOutput = true, RedirectStandardError = true };
            foreach (var argument in new[] { "-v", "error", "-show_streams", "-of", "json", Path.Combine(root, segments[0].FileName) })
                start.ArgumentList.Add(argument);
            using var probe = Process.Start(start) ?? throw new IOException("Cannot start ffprobe.");
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(10));
            var stdout = probe.StandardOutput.ReadToEndAsync(timeout.Token);
            var stderr = probe.StandardError.ReadToEndAsync(timeout.Token);
            try
            {
                await probe.WaitForExitAsync(timeout.Token);
                Assert.True(probe.ExitCode == 0, await stderr);
                using var json = JsonDocument.Parse(await stdout);
                var streams = json.RootElement.GetProperty("streams").EnumerateArray().ToArray();
                Assert.Contains(streams, track => track.GetProperty("codec_name").GetString() == "h264");
                Assert.Contains(streams, track => track.GetProperty("codec_name").GetString() == "aac");
            }
            finally
            {
                if (!probe.HasExited) probe.Kill(entireProcessTree: true);
                await probe.WaitForExitAsync(CancellationToken.None);
            }
        }
        finally
        {
            if (pixels != IntPtr.Zero) Marshal.FreeHGlobal(pixels);
            Directory.Delete(root, recursive: true);
        }
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
