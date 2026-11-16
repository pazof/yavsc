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

    private readonly Clock _clock = new();

    private LiveProcessor New(long quota = 1024 * 1024)
    {
        var processor = new LiveProcessor(NullLoggerFactory.Instance,
            Options.Create(new PodcastOptions { RootPath = _root, MaxSegmentBytes = 1024 * 1024, MaxOwnerBytes = quota }),
            _clock);
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
        await using (var ts = await restored.OpenSegmentAsync(podcast.Id, 0, "alice", _ct))
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

    [Fact]
    public void Edit_plan_merges_clamps_and_rejects_invalid_or_total_removals()
    {
        Assert.Equal([new ReplayRange(0, 1000), new ReplayRange(3000, 3500), new ReplayRange(4000, 5000)],
            ReplayEditPlan.KeptRanges([new(2000, 3000), new(1000, 2500), new(3500, 4000)], 5000));
        Assert.Equal([new ReplayRange(1000, 4000)],
            ReplayEditPlan.KeptRanges([new(-500, 1000), new(4000, 9000)], 5000));
        Assert.Equal([new ReplayRange(0, 5000)], ReplayEditPlan.KeptRanges([], 5000));
        Assert.Throws<ArgumentException>(() => ReplayEditPlan.KeptRanges([new(0, 2000), new(2000, 5000)], 5000));
        Assert.Throws<ArgumentException>(() => ReplayEditPlan.KeptRanges([new(1000, 1000)], 5000));
        Assert.Throws<ArgumentException>(() => ReplayEditPlan.KeptRanges([new(5000, 6000)], 5000));
        Assert.Throws<ArgumentException>(() => ReplayEditPlan.KeptRanges(
            Enumerable.Range(0, ReplayEditPlan.MaxRemovedRanges + 1).Select(i => new ReplayRange(i * 10, i * 10 + 5)), 5000));
        Assert.Equal([new ReplayRange(0, 3000), new ReplayRange(4000, 5000)],
            ReplayEditPlan.RemovedRanges([new(2000, 3000), new(1000, 2500), new(-500, 1000), new(4000, 9000)], 5000));
        Assert.Throws<ArgumentException>(() => ReplayEditPlan.RemovedRanges([new(0, 2000), new(2000, 5000)], 5000));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("1.4")] // Replays remuxed from live TS segments do not start at zero.
    public async Task Ffmpeg_editor_cuts_at_exact_frames_and_keeps_audio_in_sync(string startOffset)
    {
        Directory.CreateDirectory(_root);
        var source = Path.Combine(_root, "frames.mp4");
        // 10 fps, frame N has luma 20 + 5N; keyframes every 2 s so cuts fall between keyframes.
        await PodcastMediaFixture.RunAsync("ffmpeg", ["-v", "error",
            "-f", "lavfi", "-i", "color=black:s=32x32:r=10:d=4,format=yuv420p,geq=lum='20+5*N':cb=128:cr=128",
            "-f", "lavfi", "-i", "sine=frequency=440:sample_rate=48000:duration=4",
            "-c:v", "libx264", "-qp", "0", "-g", "20", "-c:a", "aac", "-shortest",
            "-output_ts_offset", startOffset, source], _ct);
        var kept = ReplayEditPlan.KeptRanges([new(1000, 1600), new(2400, 3000)], 4000).ToArray();
        kept[^1] = kept[^1] with { EndMs = ReplayRange.OpenEnd };
        var output = Path.Combine(_root, "edited.mp4");
        var editor = new FfmpegPodcastReplayEditor(Options.Create(new PodcastOptions()));

        await editor.RenderAsync(source, kept, output, 10_000_000, _ct);

        var frames = Path.Combine(_root, "edited.yuv");
        await PodcastMediaFixture.RunAsync("ffmpeg",
            ["-v", "error", "-i", output, "-fps_mode", "passthrough", "-f", "rawvideo", "-pix_fmt", "yuv420p", frames], _ct);
        var bytes = await File.ReadAllBytesAsync(frames, _ct);
        const int FrameBytes = 32 * 32 * 3 / 2;
        var indices = Enumerable.Range(0, bytes.Length / FrameBytes)
            .Select(i => (int)Math.Round((bytes[i * FrameBytes + 16 * 32 + 16] - 20) / 5.0)).ToArray();
        Assert.Equal(Enumerable.Range(0, 10).Concat(Enumerable.Range(16, 8)).Concat(Enumerable.Range(30, 10)), indices);

        var probe = await PodcastMediaFixture.RunAsync("ffprobe",
            ["-v", "error", "-show_streams", "-of", "json", output], _ct);
        using var json = JsonDocument.Parse(probe);
        var durations = json.RootElement.GetProperty("streams").EnumerateArray().ToDictionary(
            s => s.GetProperty("codec_type").GetString()!,
            s => double.Parse(s.GetProperty("duration").GetString()!, System.Globalization.CultureInfo.InvariantCulture));
        // Ranges use the format timeline, like LibVLC and FFmpeg input seeking. When audio starts
        // slightly before video, each range keeps that lead, so allow one frame (100 ms) of slack.
        Assert.InRange(durations["video"], 2.7, 2.91);
        Assert.InRange(durations["audio"], 2.7, 2.91);
        Assert.InRange(Math.Abs(durations["video"] - durations["audio"]), 0, 0.1);
        var ascii = Encoding.Latin1.GetString(await File.ReadAllBytesAsync(output, _ct));
        Assert.True(ascii.IndexOf("moov", StringComparison.Ordinal) < ascii.IndexOf("mdat", StringComparison.Ordinal));
        await Assert.ThrowsAsync<IOException>(() => editor.RenderAsync(source, kept,
            Path.Combine(_root, "too-big.mp4"), 10, _ct));
    }

    [Fact]
    public async Task Ffmpeg_editor_detects_a_source_without_audio()
    {
        Directory.CreateDirectory(_root);
        var source = Path.Combine(_root, "silent.mp4");
        await PodcastMediaFixture.RunAsync("ffmpeg", ["-v", "error", "-f", "lavfi", "-i", "testsrc2=s=32x32:r=10:d=2",
            "-c:v", "libx264", source], _ct);
        var output = Path.Combine(_root, "silent-edited.mp4");

        await new FfmpegPodcastReplayEditor(Options.Create(new PodcastOptions())).RenderAsync(source,
            [new(0, 500), new(1000, ReplayRange.OpenEnd)], output, 10_000_000, _ct);

        var probe = await PodcastMediaFixture.RunAsync("ffprobe",
            ["-v", "error", "-show_entries", "stream=codec_type", "-of", "csv=p=0", output], _ct);
        Assert.Equal("video", probe.Trim());
    }

    private async Task<PodcastResponse> Refined(LiveProcessor processor, int sourceBytes = 10,
        int retentionDays = PodcastRetention.DefaultDays)
    {
        var podcast = await processor.CreateAsync("alice", "alice", "Refined", _ct, refineReplay: true,
            retentionDays: retentionDays);
        await processor.AppendAsync(podcast.Id, "alice", 0, 2, new MemoryStream(LiveProcessorTests.Packets()), _ct);
        await processor.AppendAsync(podcast.Id, "alice", 1, 2, new MemoryStream(LiveProcessorTests.Packets()), _ct);
        var completed = await processor.CompleteAsync(podcast.Id, "alice", _ct);
        Assert.Equal(PodcastReplayState.Pending, completed.Replay?.State);
        Assert.Equal(PodcastDraftState.Preparing, completed.Draft?.State);
        Assert.True(await processor.PrepareNextReplayAsync(Bytes(sourceBytes), _ct));
        return (await processor.GetAsync(podcast.Id, _ct)).Podcast;
    }

    private static SavePodcastDraftRequest Save(int revision, long[] marks, params PodcastDraftRange[] removed)
        => new() { Revision = revision, Marks = marks, RemovedRanges = removed };

    [Fact]
    public async Task Refined_replay_stays_private_until_the_saved_draft_is_published_and_republished()
    {
        var processor = New();
        var podcast = await Refined(processor);
        var directory = Path.Combine(_root, podcast.Id.ToString("N"));
        Assert.Equal(PodcastDraftState.Ready, podcast.Draft?.State);
        Assert.Equal(10, podcast.Draft!.SourceBytes);
        Assert.True(File.Exists(Path.Combine(directory, "source.mp4")));
        Assert.False(File.Exists(Path.Combine(directory, "replay.mp4")));
        var segmentsBytes = podcast.Bytes - 10;
        Assert.False(await processor.PrepareNextReplayAsync(Bytes(4), _ct));
        Assert.False(await processor.PublishNextDraftAsync(new Editor((_, _, _, _, _) => Task.CompletedTask), _ct));
        Assert.Equal(409, (await Assert.ThrowsAsync<PodcastException>(
            () => processor.OpenReplayAsync(podcast.Id, _ct))).StatusCode);
        Assert.Equal(403, (await Assert.ThrowsAsync<PodcastException>(
            () => processor.OpenSegmentAsync(podcast.Id, 0, "bob", _ct))).StatusCode);
        Assert.Equal(403, (await Assert.ThrowsAsync<PodcastException>(
            () => processor.OpenDraftSourceAsync(podcast.Id, "bob", _ct))).StatusCode);
        var hidden = PodcastVisibility.ForViewer(await processor.GetAsync(podcast.Id, _ct), "bob");
        Assert.Null(hidden.Podcast.Draft);
        Assert.Empty(hidden.Segments);
        await using (await processor.OpenSegmentAsync(podcast.Id, 0, "alice", _ct)) { }
        await using (var source = await processor.OpenDraftSourceAsync(podcast.Id, "alice", _ct))
            Assert.Equal(10, source.Length);

        Assert.Equal(403, (await Assert.ThrowsAsync<PodcastException>(
            () => processor.SaveDraftAsync(podcast.Id, "bob", Save(0, []), _ct))).StatusCode);
        Assert.Equal(400, (await Assert.ThrowsAsync<PodcastException>(() => processor.SaveDraftAsync(
            podcast.Id, "alice", Save(0, [], new PodcastDraftRange(0, 4000)), _ct))).StatusCode);
        Assert.Equal(400, (await Assert.ThrowsAsync<PodcastException>(() => processor.SaveDraftAsync(
            podcast.Id, "alice", Save(0, [-1]), _ct))).StatusCode);
        var saved = await processor.SaveDraftAsync(podcast.Id, "alice",
            Save(0, [3500, 500, 1500, 500], new PodcastDraftRange(1000, 2000)), _ct);
        Assert.Equal(1, saved.Draft!.Revision);
        Assert.Equal([500L, 3500], saved.Draft.Marks); // The mark in the removed passage disappears.
        Assert.Equal(409, (await Assert.ThrowsAsync<PodcastException>(() => processor.SaveDraftAsync(
            podcast.Id, "alice", Save(0, []), _ct))).StatusCode);

        Assert.Equal(PodcastDraftState.Publishing,
            (await processor.PublishDraftAsync(podcast.Id, "alice", 1, _ct)).Draft?.State);
        Assert.Equal(PodcastDraftState.Publishing,
            (await processor.PublishDraftAsync(podcast.Id, "alice", 1, _ct)).Draft?.State);
        Assert.Equal(409, (await Assert.ThrowsAsync<PodcastException>(() => processor.SaveDraftAsync(
            podcast.Id, "alice", Save(1, []), _ct))).StatusCode);
        // A restart keeps the queued publication.
        processor = New();
        Assert.True(await processor.PublishNextDraftAsync(new Editor(async (source, kept, output, allowance, ct) =>
        {
            Assert.Equal(Path.Combine(directory, "source.mp4"), source);
            Assert.Equal([new ReplayRange(0, 1000), new ReplayRange(2000, ReplayRange.OpenEnd)], kept);
            Assert.Equal(1024 * 1024 - podcast.Bytes, allowance);
            await File.WriteAllBytesAsync(output, new byte[6], ct);
        }), _ct));
        var published = (await processor.GetAsync(podcast.Id, _ct)).Podcast;
        Assert.Equal(new PodcastReplayResponse(PodcastReplayState.Ready, 6), published.Replay);
        Assert.Equal(PodcastDraftState.Ready, published.Draft?.State);
        Assert.Equal(1, published.Draft!.PublishedRevision);
        Assert.Equal(segmentsBytes + 10 + 6, published.Bytes);
        Assert.Equal(published, await processor.PublishDraftAsync(podcast.Id, "alice", 1, _ct));
        Assert.Equal(403, (await Assert.ThrowsAsync<PodcastException>(
            () => processor.OpenSegmentAsync(podcast.Id, 0, "bob", _ct))).StatusCode);

        // Re-editing keeps the published MP4 served until its atomic replacement.
        await processor.SaveDraftAsync(podcast.Id, "alice", Save(1, [500]), _ct);
        await processor.PublishDraftAsync(podcast.Id, "alice", 2, _ct);
        await using var previous = await processor.OpenReplayAsync(podcast.Id, _ct);
        Assert.True(await processor.PublishNextDraftAsync(new Editor((_, _, _, _, _) =>
            throw new InvalidOperationException("Without removals the source is copied.")), _ct));
        Assert.Equal(6, previous.Length);
        var replaced = (await processor.GetAsync(podcast.Id, _ct)).Podcast;
        Assert.Equal(new PodcastReplayResponse(PodcastReplayState.Ready, 10), replaced.Replay);
        Assert.Equal(segmentsBytes + 10 + 10, replaced.Bytes);
        await using (var replay = await processor.OpenReplayAsync(podcast.Id, _ct))
            Assert.Equal(10, replay.Length);
    }

    [Fact]
    public async Task Failed_publication_keeps_the_draft_and_the_published_replay()
    {
        var processor = New();
        var podcast = await Refined(processor);
        await processor.PublishDraftAsync(podcast.Id, "alice", 0, _ct);
        Assert.True(await processor.PublishNextDraftAsync(new Editor((_, _, _, _, _) => Task.CompletedTask), _ct));
        await processor.SaveDraftAsync(podcast.Id, "alice", Save(0, [], new PodcastDraftRange(0, 1000)), _ct);
        await processor.PublishDraftAsync(podcast.Id, "alice", 1, _ct);
        Assert.True(await processor.PublishNextDraftAsync(new Editor(async (_, _, output, _, ct) =>
        {
            await File.WriteAllBytesAsync(output, [1, 2], ct);
            throw new InvalidDataException("Synthetic failure");
        }), _ct));
        var failed = (await processor.GetAsync(podcast.Id, _ct)).Podcast;
        Assert.Equal(PodcastDraftState.Ready, failed.Draft?.State);
        Assert.NotEmpty(failed.Draft!.Error!);
        Assert.Equal(1, failed.Draft.Revision);
        Assert.Equal(0, failed.Draft.PublishedRevision);
        Assert.Equal(new PodcastReplayResponse(PodcastReplayState.Ready, 10), failed.Replay);
        Assert.False(File.Exists(Path.Combine(_root, podcast.Id.ToString("N"), "replay.tmp.mp4")));
    }

    [Fact]
    public async Task Interrupted_refined_emission_prepares_a_private_draft_and_failed_sources_can_be_retried()
    {
        var processor = New();
        var podcast = await processor.CreateAsync("alice", "alice", "Interrupted", _ct, refineReplay: true);
        await processor.AppendAsync(podcast.Id, "alice", 0, 2, new MemoryStream(LiveProcessorTests.Packets()), _ct);
        await using (await processor.OpenSegmentAsync(podcast.Id, 0, "bob", _ct)) { }
        var restarted = New();
        var interrupted = (await restarted.GetAsync(podcast.Id, _ct)).Podcast;
        Assert.Equal(PodcastState.Interrupted, interrupted.State);
        Assert.Equal(PodcastDraftState.Preparing, interrupted.Draft?.State);
        Assert.Equal(403, (await Assert.ThrowsAsync<PodcastException>(
            () => restarted.OpenSegmentAsync(podcast.Id, 0, "bob", _ct))).StatusCode);
        Assert.True(await restarted.PrepareNextReplayAsync(new Remuxer(async (_, _, output, _, ct) =>
        {
            await File.WriteAllBytesAsync(output, [1], ct);
            throw new InvalidDataException("Synthetic failure");
        }), _ct));
        var failed = (await restarted.GetAsync(podcast.Id, _ct)).Podcast;
        Assert.Equal(PodcastDraftState.Failed, failed.Draft?.State);
        Assert.Equal(PodcastReplayState.Pending, failed.Replay?.State);
        Assert.Equal(interrupted.Bytes, failed.Bytes);
        Assert.False(File.Exists(Path.Combine(_root, podcast.Id.ToString("N"), "source.mp4")));
        Assert.Equal(403, (await Assert.ThrowsAsync<PodcastException>(
            () => restarted.RetryReplayAsync(podcast.Id, "bob", _ct))).StatusCode);
        await restarted.RetryReplayAsync(podcast.Id, "alice", _ct);
        Assert.True(await restarted.PrepareNextReplayAsync(Bytes(3), _ct));
        var completed = await restarted.CompleteAsync(podcast.Id, "alice", _ct);
        Assert.Equal(PodcastDraftState.Ready, completed.Draft?.State);
        Assert.Equal(3, completed.Draft!.SourceBytes);
    }

    private async Task<PodcastResponse> Publish(LiveProcessor processor, PodcastResponse podcast)
    {
        await processor.PublishDraftAsync(podcast.Id, "alice", podcast.Draft!.Revision, _ct);
        Assert.True(await processor.PublishNextDraftAsync(new Editor((_, _, _, _, _) => Task.CompletedTask), _ct));
        return (await processor.GetAsync(podcast.Id, _ct)).Podcast;
    }

    [Fact]
    public async Task Retention_purges_published_originals_at_the_deadline_and_deletes_unpublished_emissions()
    {
        var processor = New();
        var ended = _clock.Now;
        var published = await Publish(processor, await Refined(processor));
        var unpublished = await Refined(processor);
        var publishing = await Refined(processor);
        Assert.Equal(ended.AddDays(7), published.OriginalsExpireAt);
        Assert.Null(PodcastVisibility.ForViewer(published, "bob").OriginalsExpireAt);
        await processor.PublishDraftAsync(publishing.Id, "alice", 0, _ct);

        _clock.Now = ended.AddDays(7).AddTicks(-1);
        Assert.Empty(await processor.PurgeExpiredOriginalsAsync(_ct));
        Assert.False((await processor.GetAsync(published.Id, _ct)).Podcast.OriginalsDeleted);

        _clock.Now = ended.AddDays(7);
        processor = New(); // The deadlines survive a restart.
        Assert.Equal([unpublished.Id], (await processor.PurgeExpiredOriginalsAsync(_ct)).Select(p => p.Id));
        Assert.False(Directory.Exists(Path.Combine(_root, unpublished.Id.ToString("N"))));
        Assert.Equal(404, (await Assert.ThrowsAsync<PodcastException>(
            () => processor.GetAsync(unpublished.Id, _ct))).StatusCode);

        var purged = await processor.GetAsync(published.Id, _ct);
        Assert.True(purged.Podcast.OriginalsDeleted);
        Assert.Null(purged.Podcast.Draft);
        Assert.Null(purged.Podcast.OriginalsExpireAt);
        Assert.Empty(purged.Segments);
        Assert.Equal(0, purged.Podcast.SegmentCount);
        Assert.Equal(10, purged.Podcast.Bytes); // Only the published MP4 is still counted.
        var directory = Path.Combine(_root, published.Id.ToString("N"));
        Assert.Equal(["index.json", "replay.mp4"], Directory.GetFiles(directory).Select(Path.GetFileName).Order());
        await using (var replay = await processor.OpenReplayAsync(published.Id, _ct)) Assert.Equal(10, replay.Length);
        Assert.Equal(409, (await Assert.ThrowsAsync<PodcastException>(() => processor.SaveDraftAsync(
            published.Id, "alice", Save(0, []), _ct))).StatusCode);
        Assert.Equal(409, (await Assert.ThrowsAsync<PodcastException>(
            () => processor.OpenDraftSourceAsync(published.Id, "alice", _ct))).StatusCode);

        // A queued publication finishes before its originals are purged.
        Assert.False((await processor.GetAsync(publishing.Id, _ct)).Podcast.OriginalsDeleted);
        Assert.True(await processor.PublishNextDraftAsync(new Editor((_, _, _, _, _) => Task.CompletedTask), _ct));
        Assert.Empty(await processor.PurgeExpiredOriginalsAsync(_ct));
        Assert.True((await processor.GetAsync(publishing.Id, _ct)).Podcast.OriginalsDeleted);
    }

    [Fact]
    public async Task Zero_day_retention_starts_on_publication_and_owner_can_delete_published_originals()
    {
        var processor = New();
        Assert.Equal(400, (await Assert.ThrowsAsync<PodcastException>(() => processor.CreateAsync(
            "alice", "alice", "Too long", _ct, refineReplay: true, retentionDays: 366))).StatusCode);
        var immediate = await Refined(processor, retentionDays: 0);
        Assert.Null(immediate.OriginalsExpireAt);
        _clock.Now = _clock.Now.AddYears(1);
        Assert.Empty(await processor.PurgeExpiredOriginalsAsync(_ct));
        Assert.Equal(409, (await Assert.ThrowsAsync<PodcastException>(
            () => processor.DeleteOriginalsAsync(immediate.Id, "alice", _ct))).StatusCode);
        Assert.Equal(_clock.Now, (await Publish(processor, immediate)).OriginalsExpireAt);
        Assert.Empty(await processor.PurgeExpiredOriginalsAsync(_ct));
        Assert.True((await processor.GetAsync(immediate.Id, _ct)).Podcast.OriginalsDeleted);

        var kept = await Publish(processor, await Refined(processor));
        Assert.Equal(403, (await Assert.ThrowsAsync<PodcastException>(
            () => processor.DeleteOriginalsAsync(kept.Id, "bob", _ct))).StatusCode);
        var deleted = await processor.DeleteOriginalsAsync(kept.Id, "alice", _ct);
        Assert.True(deleted.OriginalsDeleted);
        Assert.Equal(deleted, await processor.DeleteOriginalsAsync(kept.Id, "alice", _ct));
        Assert.False(File.Exists(Path.Combine(_root, kept.Id.ToString("N"), "source.mp4")));
    }

    private static Remuxer Bytes(int count) => new((_, _, output, _, ct) => File.WriteAllBytesAsync(output, new byte[count], ct));
    private sealed class Editor(Func<string, IReadOnlyList<ReplayRange>, string, long, CancellationToken, Task> render)
        : IPodcastReplayEditor
    {
        public Task RenderAsync(string source, IReadOnlyList<ReplayRange> kept, string output, long max, CancellationToken ct)
            => render(source, kept, output, max, ct);
    }

    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => Now;
    }

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
