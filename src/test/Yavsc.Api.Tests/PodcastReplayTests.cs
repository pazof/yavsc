using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Yavsc.Abstract.Streaming;
using Yavsc.Services;

namespace Yavsc.Api.Test;

[Trait("Category", "non-reg")]
public sealed class PodcastReplayTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"yavsc-replay-{Guid.NewGuid():N}");
    private readonly List<LiveProcessor> _processors = [];
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    private LiveProcessor New(long quota = 1024 * 1024)
    {
        var processor = new LiveProcessor(NullLoggerFactory.Instance,
            Options.Create(new PodcastOptions { RootPath = _root, MaxSegmentBytes = 1024 * 1024, MaxOwnerBytes = quota }));
        _processors.Add(processor);
        return processor;
    }

    private async Task<PodcastResponse> Complete(LiveProcessor processor, string owner = "alice")
    {
        var podcast = await processor.CreateAsync(owner, owner, "Test", _ct);
        await processor.AppendAsync(podcast.Id, owner, 0, 2, new MemoryStream(LiveProcessorTests.Packets()), _ct);
        return await processor.CompleteAsync(podcast.Id, owner, _ct);
    }

    [Fact]
    public async Task Ready_is_atomic_retains_TS_and_accounts_both_formats_once_across_restart()
    {
        var processor = New();
        var podcast = await Complete(processor);
        Assert.Equal(PodcastReplayState.Pending, podcast.Replay?.State);
        Assert.Equal(409, (await Assert.ThrowsAsync<PodcastException>(
            () => processor.OpenReplayAsync(podcast.Id, _ct))).StatusCode);
        var remuxer = new Remuxer(async (_, _, output, allowance, ct) =>
        {
            Assert.Equal(1024 * 1024 - podcast.Bytes, allowance);
            Assert.Equal(PodcastReplayState.Processing, (await processor.GetAsync(podcast.Id, ct)).Podcast.Replay?.State);
            Assert.Equal(409, (await Assert.ThrowsAsync<PodcastException>(
                () => processor.DeleteAsync(podcast.Id, "alice", ct))).StatusCode);
            Assert.Equal(409, (await Assert.ThrowsAsync<PodcastException>(
                () => processor.OpenReplayAsync(podcast.Id, ct))).StatusCode);
            await File.WriteAllBytesAsync(output, [1, 2, 3, 4], ct);
        });
        Assert.True(await processor.PrepareNextReplayAsync(remuxer, _ct));
        Assert.False(await processor.PrepareNextReplayAsync(remuxer, _ct));
        var restored = New();
        var ready = (await restored.GetAsync(podcast.Id, _ct)).Podcast;
        Assert.Equal(new PodcastReplayResponse(PodcastReplayState.Ready, 4), ready.Replay);
        Assert.Equal(podcast.Bytes + 4, ready.Bytes);
        Assert.Equal(ready, await restored.CompleteAsync(podcast.Id, "alice", _ct));
        await using (var mp4 = await restored.OpenReplayAsync(podcast.Id, _ct))
        {
            Assert.True(mp4.CanSeek);
            mp4.Position = 2;
            Assert.Equal(3, mp4.ReadByte());
        }
        await using (var ts = await restored.OpenSegmentAsync(podcast.Id, 0, _ct))
            Assert.Equal(podcast.Bytes, ts.Length);
        await restored.DeleteAsync(podcast.Id, "alice", _ct);
        Assert.False(Directory.Exists(Path.Combine(_root, podcast.Id.ToString("N"))));
    }

    [Fact]
    public async Task Failed_remux_cleans_partial_file_and_only_owner_can_retry()
    {
        var processor = New();
        var podcast = await Complete(processor);
        await processor.PrepareNextReplayAsync(new Remuxer(async (_, _, output, _, ct) =>
        {
            await File.WriteAllBytesAsync(output, [1, 2], ct);
            throw new IOException("Synthetic failure");
        }), _ct);
        var failed = (await processor.GetAsync(podcast.Id, _ct)).Podcast;
        Assert.Equal(PodcastReplayState.Failed, failed.Replay?.State);
        Assert.NotEmpty(failed.Replay!.Error!);
        Assert.Equal(podcast.Bytes, failed.Bytes);
        Assert.False(File.Exists(Path.Combine(_root, podcast.Id.ToString("N"), "replay.tmp.mp4")));
        Assert.Equal(403, (await Assert.ThrowsAsync<PodcastException>(
            () => processor.RetryReplayAsync(podcast.Id, "bob", _ct))).StatusCode);
        await processor.RetryReplayAsync(podcast.Id, "alice", _ct);
        Assert.Equal(409, (await Assert.ThrowsAsync<PodcastException>(
            () => processor.RetryReplayAsync(podcast.Id, "alice", _ct))).StatusCode);
        Assert.True(await processor.PrepareNextReplayAsync(Bytes(4), _ct));
        Assert.Equal(podcast.Bytes + 4, (await processor.GetAsync(podcast.Id, _ct)).Podcast.Bytes);
    }

    [Fact]
    public async Task Quota_rechecked_after_concurrent_upload_and_failed_output_is_not_published()
    {
        const long quota = 1024 * 1024;
        var processor = New(quota);
        var podcast = await Complete(processor);
        var other = await processor.CreateAsync("alice", "alice", "Next", _ct);
        await processor.PrepareNextReplayAsync(new Remuxer(async (_, _, output, _, ct) =>
        {
            await File.WriteAllBytesAsync(output, new byte[quota - podcast.Bytes], ct);
            await processor.AppendAsync(other.Id, "alice", 0, 2, new MemoryStream(LiveProcessorTests.Packets()), ct);
        }), _ct);
        Assert.Equal(PodcastReplayState.Failed, (await processor.GetAsync(podcast.Id, _ct)).Podcast.Replay?.State);
        Assert.Equal(podcast.Bytes, (await processor.GetAsync(podcast.Id, _ct)).Podcast.Bytes);
        Assert.False(File.Exists(Path.Combine(_root, podcast.Id.ToString("N"), "replay.mp4")));
    }

    [Fact]
    public async Task Host_cancellation_requeues_and_releases_processing_guard()
    {
        var processor = New();
        var podcast = await Complete(processor);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(_ct);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => processor.PrepareNextReplayAsync(
            new Remuxer((_, _, _, _, ct) =>
            {
                cancellation.Cancel();
                ct.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            }), cancellation.Token));
        Assert.Equal(PodcastReplayState.Pending, (await processor.GetAsync(podcast.Id, _ct)).Podcast.Replay?.State);
        Assert.True(await processor.PrepareNextReplayAsync(Bytes(4), _ct));
        await processor.DeleteAsync(podcast.Id, "alice", _ct);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Processing")]
    public async Task Legacy_completed_and_in_progress_indexes_are_requeued_after_restart(string? state)
    {
        var processor = New();
        var podcast = await Complete(processor);
        var directory = Path.Combine(_root, podcast.Id.ToString("N"));
        var index = Path.Combine(directory, "index.json");
        var document = JsonNode.Parse(await File.ReadAllTextAsync(index, _ct))!;
        document["Manifest"]!["Podcast"]!["Replay"] = state is null ? null
            : JsonSerializer.SerializeToNode(new PodcastReplayResponse(PodcastReplayState.Processing));
        await File.WriteAllTextAsync(index, document.ToJsonString(), _ct);
        // Simulate a crash between rename and metadata publication.
        await File.WriteAllBytesAsync(Path.Combine(directory, "replay.mp4"), [9, 9], _ct);
        var restarted = New();
        Assert.Equal(PodcastReplayState.Pending, (await restarted.GetAsync(podcast.Id, _ct)).Podcast.Replay?.State);
        Assert.True(await restarted.PrepareNextReplayAsync(Bytes(4), _ct));
        Assert.Equal(podcast.Bytes + 4, (await restarted.GetAsync(podcast.Id, _ct)).Podcast.Bytes);
    }

    [Fact]
    public async Task Interrupted_is_not_queued_until_owner_explicitly_completes_it()
    {
        var processor = New();
        var podcast = await processor.CreateAsync("alice", "alice", "Interrupted", _ct);
        await processor.AppendAsync(podcast.Id, "alice", 0, 2, new MemoryStream(LiveProcessorTests.Packets()), _ct);
        var restarted = New();
        Assert.False(await restarted.PrepareNextReplayAsync(Bytes(4), _ct));
        Assert.Equal(PodcastState.Interrupted, (await restarted.GetAsync(podcast.Id, _ct)).Podcast.State);
        await restarted.CompleteAsync(podcast.Id, "alice", _ct);
        Assert.True(await restarted.PrepareNextReplayAsync(Bytes(4), _ct));
    }

    [Fact]
    public async Task Ffmpeg_remux_preserves_duration_tracks_faststart_and_decodable_time_seek()
    {
        var processor = New();
        var source = Path.Combine(_root, "fixture");
        var segments = await PodcastMediaFixture.CreateAsync(source, _ct);
        var podcast = await processor.CreateAsync("alice", "Alice", "Synthetic", _ct);
        for (var i = 0; i < segments.Length; i++)
            await processor.AppendAsync(podcast.Id, "alice", i, 1,
                new MemoryStream(await File.ReadAllBytesAsync(segments[i], _ct)), _ct);
        await processor.CompleteAsync(podcast.Id, "alice", _ct);
        var remuxer = new FfmpegPodcastReplayRemuxer(Options.Create(new PodcastOptions()));
        Assert.True(await processor.PrepareNextReplayAsync(remuxer, _ct));
        var ready = (await processor.GetAsync(podcast.Id, _ct)).Podcast;
        Assert.Equal(PodcastReplayState.Ready, ready.Replay?.State);
        var mp4 = Path.Combine(_root, podcast.Id.ToString("N"), "replay.mp4");
        var probe = await PodcastMediaFixture.RunAsync("ffprobe",
            ["-v", "error", "-show_format", "-show_streams", "-of", "json", mp4], _ct);
        using var json = JsonDocument.Parse(probe);
        Assert.Equal(["h264", "aac"], json.RootElement.GetProperty("streams").EnumerateArray()
            .Select(s => s.GetProperty("codec_name").GetString()!).ToArray());
        var duration = double.Parse(json.RootElement.GetProperty("format").GetProperty("duration").GetString()!,
            System.Globalization.CultureInfo.InvariantCulture);
        Assert.InRange(duration, 3.9, 4.3);
        var ascii = Encoding.Latin1.GetString(await File.ReadAllBytesAsync(mp4, _ct));
        Assert.True(ascii.IndexOf("moov", StringComparison.Ordinal) < ascii.IndexOf("mdat", StringComparison.Ordinal));
        var frame = Path.Combine(source, "seek.rgb");
        await PodcastMediaFixture.RunAsync("ffmpeg",
            ["-v", "error", "-ss", "2", "-i", mp4, "-frames:v", "1", "-f", "rawvideo", "-pix_fmt", "rgb24", frame], _ct);
        Assert.Equal(160 * 120 * 3, new FileInfo(frame).Length);
        await PodcastMediaFixture.RunAsync("ffmpeg", ["-v", "error", "-i", mp4, "-f", "null", "-"], _ct);
        await Assert.ThrowsAsync<IOException>(() => remuxer.RemuxAsync(source, segments.Length,
            Path.Combine(source, "too-big.mp4"), 10, _ct));
    }

    private static Remuxer Bytes(int count) => new((_, _, output, _, ct) => File.WriteAllBytesAsync(output, new byte[count], ct));
    private sealed class Remuxer(Func<string, int, string, long, CancellationToken, Task> remux) : IPodcastReplayRemuxer
    {
        public Task RemuxAsync(string directory, int count, string output, long max, CancellationToken ct)
            => remux(directory, count, output, max, ct);
    }

    public void Dispose()
    {
        foreach (var processor in _processors) processor.Dispose();
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }
}

internal static class PodcastMediaFixture
{
    public static async Task<string[]> CreateAsync(string directory, CancellationToken ct)
    {
        Directory.CreateDirectory(directory);
        await RunAsync("ffmpeg", ["-v", "error", "-f", "lavfi", "-i", "testsrc2=size=160x120:rate=10",
            "-f", "lavfi", "-i", "sine=frequency=440:sample_rate=48000", "-t", "4",
            "-c:v", "libx264", "-threads", "1", "-g", "10", "-c:a", "aac",
            "-f", "segment", "-segment_time", "1", "-reset_timestamps", "0",
            Path.Combine(directory, "%08d.ts")], ct);
        return Directory.GetFiles(directory, "*.ts").Order(StringComparer.Ordinal).ToArray();
    }

    public static async Task<string> RunAsync(string tool, string[] arguments, CancellationToken ct)
    {
        var start = new ProcessStartInfo(tool) { RedirectStandardError = true, RedirectStandardOutput = true };
        foreach (var argument in arguments) start.ArgumentList.Add(argument);
        using var process = Process.Start(start) ?? throw new IOException($"Cannot start {tool}.");
        var output = process.StandardOutput.ReadToEndAsync(ct);
        var errors = process.StandardError.ReadToEndAsync(ct);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
            var error = await errors;
            Assert.True(process.ExitCode == 0, $"{tool} failed: {error}");
            Assert.True(string.IsNullOrWhiteSpace(error), $"{tool} reported media errors: {error}");
            return await output;
        }
        finally
        {
            if (!process.HasExited) process.Kill(true);
            await process.WaitForExitAsync(CancellationToken.None);
        }
    }
}
