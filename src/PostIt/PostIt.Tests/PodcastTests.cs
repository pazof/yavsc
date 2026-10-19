using System.Globalization;
using System.Net.Http;
using System.Runtime.InteropServices;
using LibVLCSharp.Shared;
using PostIt.Services;
using Yavsc.Abstract.Streaming;
using Yavsc.Api.Client;

namespace PostIt.Tests;

public sealed class PodcastTests
{
    private static readonly Guid Id = Guid.Parse("9e2f6d47-2b9a-47ae-a870-073980bf2628");

    [Fact]
    public void Parses_only_complete_ordered_capture_segments_with_invariant_durations()
    {
        var segments = HlsCapturePlaylist.Parse("#EXTM3U\n#EXTINF:2.5,\nsegment-00000000.ts\n#EXTINF:1.75,\nsegment-00000001.ts\n");
        Assert.Equal(2, segments.Count);
        Assert.Equal(2.5, segments[0].DurationSeconds);
        Assert.Equal(1, segments[1].Sequence);
        Assert.Empty(HlsCapturePlaylist.Parse("#EXTM3U\n#EXTINF:2,\n"));
    }

    [Theory]
    [InlineData("#EXTINF:NaN,\nsegment-00000000.ts\n")]
    [InlineData("#EXTINF:20,\nsegment-00000000.ts\n")]
    [InlineData("#EXTINF:2,\n../escape.ts\n")]
    [InlineData("#EXTINF:2,\nhttps://other.test/segment-00000000.ts\n")]
    [InlineData("#EXTINF:2,\nsegment-00000000.ts\n#EXTINF:2,\nsegment-00000002.ts\n")]
    public void Rejects_invalid_or_nonlocal_capture_playlist(string text)
        => Assert.Throws<FormatException>(() => HlsCapturePlaylist.Parse(text));

    [Fact]
    public async Task Upload_uses_raw_transport_stream_and_replayable_content_factory()
    {
        var transport = new RecordingTransport();
        var client = new PodcastApiClient(transport, () => "https://api.example/");
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            await client.UploadAsync(Id, 0, 2.5, new byte[] { 0x47, 1 }, TestContext.Current.CancellationToken);
            Assert.EndsWith($"/api/v1/podcasts/{Id}/segments/0?duration=2.5", transport.Path);
            Assert.Equal(HttpMethod.Put, transport.Method);
            Assert.NotNull(transport.ContentFactory);
            using var first = transport.ContentFactory();
            using var retry = transport.ContentFactory();
            Assert.Equal("video/mp2t", first.Headers.ContentType?.MediaType);
            Assert.Equal(new byte[] { 0x47, 1 }, await first.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
            Assert.Equal(await first.ReadAsByteArrayAsync(TestContext.Current.CancellationToken),
                await retry.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Theory]
    [InlineData("https://api.example/")]
    [InlineData("https://api.example/api/v1/")]
    [InlineData("https://api.example/api/v1")]
    public async Task Configured_api_url_never_duplicates_the_api_prefix(string baseUrl)
    {
        var transport = new RecordingTransport();
        var client = new PodcastApiClient(transport, () => baseUrl);
        await client.GetAsync(Id, TestContext.Current.CancellationToken);
        Assert.Equal($"https://api.example/api/v1/podcasts/{Id}", transport.Path);
    }
    [Fact]
    public void Authenticated_playback_reads_segments_in_order_and_reports_end_of_replay()
    {
        var transport = new RecordingTransport();
        var client = new PodcastApiClient(transport, () => "https://api.example/");
        using var input = new PodcastMediaInput(client, Id, live: false);
        var buffer = Marshal.AllocHGlobal(4);
        try
        {
            Assert.True(input.Open(out var size));
            Assert.Equal(ulong.MaxValue, size);
            Assert.Equal(2, input.Read(buffer, 4));
            Assert.Equal(0x47, Marshal.ReadByte(buffer));
            Assert.Equal(0, input.Read(buffer, 4));
            Assert.EndsWith($"/{Id}/segments/0.ts", transport.DownloadPath);
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }

    [Fact]
    public void Playback_errors_are_reported_without_crossing_the_native_callback_boundary()
    {
        var transport = new RecordingTransport { FailDownload = true };
        var client = new PodcastApiClient(transport, () => "https://api.example/");
        using var input = new PodcastMediaInput(client, Id, live: false);
        string? error = null;
        input.Failed += message => error = message;
        var buffer = Marshal.AllocHGlobal(4);
        try
        {
            Assert.Equal(-1, input.Read(buffer, 4));
            Assert.Contains("401", error);
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }

    [Fact]
    public void Live_playback_joins_three_segments_behind_the_edge_and_cancellation_is_quiet()
    {
        var transport = new RecordingTransport { SegmentCount = 8 };
        var client = new PodcastApiClient(transport, () => "https://api.example/");
        using var input = new PodcastMediaInput(client, Id, live: true);
        string? error = null;
        input.Failed += message => error = message;
        var buffer = Marshal.AllocHGlobal(4);
        try
        {
            Assert.Equal(2, input.Read(buffer, 4));
            Assert.EndsWith($"/{Id}/segments/5.ts", transport.DownloadPath);
            input.Cancel();
            Assert.Equal(-1, input.Read(buffer, 4));
            Assert.Null(error);
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }

    private sealed class RecordingTransport : IYavscApiClient
    {
        public HttpClient Http { get; } = new();
        public HttpMethod? Method { get; private set; }
        public string? Path { get; private set; }
        public string? DownloadPath { get; private set; }
        public Func<HttpContent>? ContentFactory { get; private set; }
        public bool FailDownload { get; init; }
        public int SegmentCount { get; init; } = 1;

        private static PodcastResponse Podcast => new(Id, "Live", "alice", "Alice",
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, PodcastState.Completed, 1, 2, 2);

        public Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            Method = method;
            Path = path;
            object response = typeof(T) == typeof(PodcastManifestResponse)
                ? new PodcastManifestResponse(Podcast with { SegmentCount = SegmentCount },
                    Enumerable.Range(0, SegmentCount).Select(i => new PodcastSegmentResponse(i, 2, 2)).ToArray()) : Podcast;
            return Task.FromResult((T)response);
        }
        public Task<T> CallAsync<T>(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default)
        {
            ContentFactory = contentFactory;
            return CallAsync<T>(method, path, ct: ct);
        }
        public Task CallAsync(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
            => CallAsync<PodcastResponse>(method, path, body, ct);
        public Task CallAsync(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default)
            => CallAsync<PodcastResponse>(method, path, contentFactory, ct);
        public Task<byte[]> DownloadAsync(HttpMethod method, string path, CancellationToken ct = default)
        {
            DownloadPath = path;
            if (FailDownload) throw new HttpRequestException("401 Unauthorized");
            return Task.FromResult(new byte[] { 0x47, 1 });
        }
        public ValueTask DisposeAsync() { Http.Dispose(); return ValueTask.CompletedTask; }
    }
}
