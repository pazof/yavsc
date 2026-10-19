using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Net.WebSockets;
using Yavsc.Models;
using Yavsc.Abstract.Streaming;
using Yavsc.Services;

namespace Yavsc.Api.Test;

public sealed class LiveProcessorTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"yavsc-podcast-unit-{Guid.NewGuid():N}");
    private readonly List<LiveProcessor> _processors = new();
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    private LiveProcessor New(Action<PodcastOptions>? configure = null, TimeProvider? clock = null)
    {
        var options = new PodcastOptions { RootPath = _root };
        configure?.Invoke(options);
        var processor = new LiveProcessor(NullLoggerFactory.Instance, Options.Create(options), clock);
        _processors.Add(processor);
        return processor;
    }

    internal static byte[] Packets(int count = 2, byte marker = 1)
    {
        var bytes = Enumerable.Repeat(marker, 188 * count).ToArray();
        for (var i = 0; i < bytes.Length; i += 188) bytes[i] = 0x47;
        return bytes;
    }

    private Task<PodcastResponse> Append(LiveProcessor processor, Guid id, int sequence, byte[]? bytes = null,
        double duration = 2, string owner = "alice")
        => processor.AppendAsync(id, owner, sequence, duration, new MemoryStream(bytes ?? Packets()), _ct);

    [Fact]
    public async Task Complete_and_restart_preserve_order_bytes_and_metadata()
    {
        var processor = New();
        var podcast = await processor.CreateAsync("alice", "Alice", " Concert ", _ct);
        var first = Packets(2, 11);
        var second = Packets(3, 12);
        await Append(processor, podcast.Id, 0, first);
        await Append(processor, podcast.Id, 1, second, 2.5);
        var completed = await processor.CompleteAsync(podcast.Id, "alice", _ct);
        Assert.Equal(PodcastState.Completed, completed.State);
        Assert.Equal(4.5, completed.DurationSeconds);
        Assert.Equal(first.Length + second.Length, completed.Bytes);
        var restored = New();
        var manifest = await restored.GetAsync(podcast.Id, _ct);
        Assert.Equal("Concert", manifest.Podcast.Title);
        Assert.Equal(PodcastState.Completed, manifest.Podcast.State);
        Assert.Equal(new[] { 0, 1 }, manifest.Segments.Select(s => s.Sequence));
        await using var stream = await restored.OpenSegmentAsync(podcast.Id, 1, _ct);
        using var actual = new MemoryStream();
        await stream.CopyToAsync(actual, _ct);
        Assert.Equal(second, actual.ToArray());
    }

    [Fact]
    public async Task Ordered_ingestion_is_idempotent_and_conflicting_retries_are_rejected()
    {
        var processor = New();
        var podcast = await processor.CreateAsync("alice", "Alice", "Live", _ct);
        Assert.Equal(409, (await Assert.ThrowsAsync<PodcastException>(() => Append(processor, podcast.Id, 1))).StatusCode);
        var first = await Append(processor, podcast.Id, 0);
        Assert.Equal(first, await Append(processor, podcast.Id, 0));
        Assert.Equal(409, (await Assert.ThrowsAsync<PodcastException>(
            () => Append(processor, podcast.Id, 0, Packets(marker: 2)))).StatusCode);
        Assert.Equal(409, (await Assert.ThrowsAsync<PodcastException>(
            () => Append(processor, podcast.Id, 0, duration: 3))).StatusCode);
        await processor.CompleteAsync(podcast.Id, "alice", _ct);
        Assert.Equal(1, (await Append(processor, podcast.Id, 0)).SegmentCount);
        Assert.Equal(409, (await Assert.ThrowsAsync<PodcastException>(() => Append(processor, podcast.Id, 1))).StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(15.01)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public async Task Invalid_durations_never_commit(double duration)
    {
        var processor = New();
        var podcast = await processor.CreateAsync("alice", "Alice", "Live", _ct);
        Assert.Equal(400, (await Assert.ThrowsAsync<PodcastException>(
            () => Append(processor, podcast.Id, 0, duration: duration))).StatusCode);
        Assert.Empty((await processor.GetAsync(podcast.Id, _ct)).Segments);
    }

    [Fact]
    public async Task Invalid_empty_and_oversized_transport_streams_never_commit()
    {
        var processor = New(o => { o.MaxSegmentBytes = 376; o.MaxOwnerBytes = 752; });
        var podcast = await processor.CreateAsync("alice", "Alice", "Live", _ct);
        foreach (var bytes in new[] { Array.Empty<byte>(), new byte[188], Packets()[..^1] })
            Assert.Equal(400, (await Assert.ThrowsAsync<PodcastException>(() => Append(processor, podcast.Id, 0, bytes))).StatusCode);
        Assert.Equal(413, (await Assert.ThrowsAsync<PodcastException>(
            () => Append(processor, podcast.Id, 0, Packets(3)))).StatusCode);
        Assert.Empty((await processor.GetAsync(podcast.Id, _ct)).Segments);
        Assert.Equal(376, (await Append(processor, podcast.Id, 0, Packets(2))).Bytes);
    }

    [Fact]
    public async Task Owner_quota_is_exact_shared_across_replays_and_released_on_delete()
    {
        var processor = New(o => { o.MaxSegmentBytes = 376; o.MaxOwnerBytes = 564; });
        var first = await processor.CreateAsync("alice", "Alice", "One", _ct);
        await Append(processor, first.Id, 0, Packets(2));
        await processor.CompleteAsync(first.Id, "alice", _ct);
        var second = await processor.CreateAsync("alice", "Alice", "Two", _ct);
        await Append(processor, second.Id, 0, Packets(1));
        Assert.Equal(413, (await Assert.ThrowsAsync<PodcastException>(
            () => Append(processor, second.Id, 1, Packets(1)))).StatusCode);
        await processor.DeleteAsync(first.Id, "alice", _ct);
        Assert.Equal(376, (await Append(processor, second.Id, 1, Packets(1))).Bytes);
    }

    [Fact]
    public async Task Ownership_and_one_live_per_owner_are_enforced()
    {
        var processor = New();
        var podcast = await processor.CreateAsync("alice", "Alice", "One", _ct);
        Assert.Equal(409, (await Assert.ThrowsAsync<PodcastException>(
            () => processor.CreateAsync("alice", "Alice", "Two", _ct))).StatusCode);
        Assert.Equal(403, (await Assert.ThrowsAsync<PodcastException>(
            () => Append(processor, podcast.Id, 0, owner: "bob"))).StatusCode);
        Assert.Equal(403, (await Assert.ThrowsAsync<PodcastException>(
            () => processor.CompleteAsync(podcast.Id, "bob", _ct))).StatusCode);
        Assert.Equal(403, (await Assert.ThrowsAsync<PodcastException>(
            () => processor.DeleteAsync(podcast.Id, "bob", _ct))).StatusCode);
        Assert.Equal(409, (await Assert.ThrowsAsync<PodcastException>(
            () => processor.DeleteAsync(podcast.Id, "alice", _ct))).StatusCode);
        await processor.CreateAsync("bob", "Bob", "Other user", _ct);
        Assert.Equal(2, (await processor.ListAsync(_ct)).Count);
    }

    [Fact]
    public async Task Concurrent_identical_uploads_commit_only_once()
    {
        var processor = New();
        var podcast = await processor.CreateAsync("alice", "Alice", "One", _ct);
        await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Append(processor, podcast.Id, 0)));
        var manifest = await processor.GetAsync(podcast.Id, _ct);
        Assert.Single(manifest.Segments);
        Assert.Equal(376, manifest.Podcast.Bytes);
    }

    [Fact]
    public async Task Restart_or_idle_timeout_ends_live_broadcast_but_keeps_its_segments()
    {
        var clock = new Clock();
        var processor = New(clock: clock);
        var first = await processor.CreateAsync("alice", "Alice", "One", _ct);
        await Append(processor, first.Id, 0);
        clock.Now += TimeSpan.FromSeconds(61);
        Assert.Equal(PodcastState.Interrupted, (await processor.GetAsync(first.Id, _ct)).Podcast.State);
        var second = await processor.CreateAsync("alice", "Alice", "Two", _ct);
        await Append(processor, second.Id, 0);
        var restarted = New(clock: clock);
        var replay = await restarted.GetAsync(second.Id, _ct);
        Assert.Equal(PodcastState.Interrupted, replay.Podcast.State);
        Assert.Single(replay.Segments);
        await using var segment = await restarted.OpenSegmentAsync(second.Id, 0, _ct);
        Assert.Equal(376, segment.Length);
    }

    [Fact]
    public async Task Segment_and_podcast_count_limits_are_enforced()
    {
        var processor = New(o => { o.MaxSegments = 1; o.MaxOwnerPodcasts = 1; });
        var first = await processor.CreateAsync("alice", "Alice", "One", _ct);
        await Append(processor, first.Id, 0);
        Assert.Equal(413, (await Assert.ThrowsAsync<PodcastException>(() => Append(processor, first.Id, 1))).StatusCode);
        await processor.CompleteAsync(first.Id, "alice", _ct);
        Assert.Equal(413, (await Assert.ThrowsAsync<PodcastException>(
            () => processor.CreateAsync("alice", "Alice", "Two", _ct))).StatusCode);
    }

    [Fact]
    public async Task Cancelled_upload_does_not_publish_partial_bytes_and_can_be_retried()
    {
        var processor = New();
        var podcast = await processor.CreateAsync("alice", "Alice", "One", _ct);
        using var cancelled = new CancellationTokenSource();
        using var input = new CancellingStream(Packets(), cancelled);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => processor.AppendAsync(
            podcast.Id, "alice", 0, 2, input, cancelled.Token));
        Assert.Equal(188, input.Position);
        Assert.Empty((await processor.GetAsync(podcast.Id, _ct)).Segments);
        Assert.Equal(1, (await Append(processor, podcast.Id, 0)).SegmentCount);
    }

    [Fact]
    public async Task Legacy_websocket_preserves_different_frame_lengths_and_cleans_up()
    {
        var processor = New();
        Directory.CreateDirectory(_root);
        var first = new byte[] { 1, 2, 3 };
        var second = new byte[] { 4, 5, 6, 7, 8 };
        using var socket = new IncomingSocket(first, second);
        var context = new DefaultHttpContext();
        context.Features.Set<IHttpWebSocketFeature>(socket);
        var user = new ApplicationUser { UserName = "legacy", DiskQuota = 1024, DiskUsage = 0 };
        Assert.True(await processor.AcceptStream(context, user, _root, "legacy.ts"));
        Assert.Equal(first.Concat(second), await File.ReadAllBytesAsync(Path.Combine(_root, "legacy.ts"), _ct));
        Assert.Empty(processor.Casters);
        Assert.Equal(8, user.DiskUsage);
    }

    public void Dispose()
    {
        foreach (var processor in _processors) processor.Dispose();
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
    }

    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class CancellingStream(byte[] bytes, CancellationTokenSource cancellation) : MemoryStream(bytes)
    {
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var count = Read(buffer.Span[..Math.Min(188, buffer.Length)]);
            cancellation.Cancel();
            return ValueTask.FromResult(count);
        }
    }

    private sealed class IncomingSocket(params byte[][] frames) : WebSocket, IHttpWebSocketFeature
    {
        private int _index;
        private WebSocketState _state = WebSocketState.Open;
        public bool IsWebSocketRequest => true;
        public Task<WebSocket> AcceptAsync(WebSocketAcceptContext context) => Task.FromResult<WebSocket>(this);
        public override WebSocketCloseStatus? CloseStatus => _state == WebSocketState.CloseReceived ? WebSocketCloseStatus.NormalClosure : null;
        public override string? CloseStatusDescription => null;
        public override WebSocketState State => _state;
        public override string? SubProtocol => null;
        public override void Abort() => _state = WebSocketState.Aborted;
        public override Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
        { _state = WebSocketState.Closed; return Task.CompletedTask; }
        public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
            => CloseAsync(closeStatus, statusDescription, cancellationToken);
        public override void Dispose() => _state = WebSocketState.Closed;
        public override Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            if (_index == frames.Length)
            {
                _state = WebSocketState.CloseReceived;
                return Task.FromResult(new WebSocketReceiveResult(0, WebSocketMessageType.Close, true, WebSocketCloseStatus.NormalClosure, null));
            }
            var frame = frames[_index++];
            frame.CopyTo(buffer.AsSpan());
            return Task.FromResult(new WebSocketReceiveResult(frame.Length, WebSocketMessageType.Binary, true));
        }
        public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
