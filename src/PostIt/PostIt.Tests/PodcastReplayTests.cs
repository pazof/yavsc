using System.Net;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using LibVLCSharp.Shared;
using Avalonia.Controls;
using Avalonia.Headless;
using PostIt.Services;
using Yavsc.Api.Client;

namespace PostIt.Tests;

[Trait("DependsOn", "Native")]
[Trait("Speed", "Slow")]
[Trait("Category", "non-reg")]
public sealed class PodcastReplayTests
{
    private static readonly Guid Id = Guid.NewGuid();
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;
    public static bool HasNativeFixture => File.Exists(Environment.GetEnvironmentVariable("POSTIT_TEST_REPLAY"));

    [Fact(Skip = "Set POSTIT_TEST_REPLAY to a local MP4 (8+ seconds) and install LibVLC.", SkipUnless = nameof(HasNativeFixture))]
    public async Task LibVLC_decodes_authenticated_MP4_ranges_and_seeks_in_both_directions()
    {
        using var transport = new RangeTransport
        {
            Bytes = await File.ReadAllBytesAsync(Environment.GetEnvironmentVariable("POSTIT_TEST_REPLAY")!, _ct)
        };
        var api = new PodcastApiClient(transport, () => "https://api.example/");
        Core.Initialize();
        using var vlc = new LibVLC("--vout=dummy", "--aout=dummy", "--no-xlib");
        using var input = new PodcastReplayMediaInput(api, Id, transport.Bytes.Length);
        using var media = new Media(vlc, input);
        using var player = new MediaPlayer(vlc);
        string? failure = null;
        input.Failed += message => failure = message;
        var ended = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        player.EndReached += (_, _) => ended.TrySetResult();
        player.EncounteredError += (_, _) => ended.TrySetException(new IOException("LibVLC MP4 decoding failed."));
        Assert.True(player.Play(media));
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(_ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(30));
            while (player.Length <= 0 || !player.IsSeekable || player.Time <= 0)
                await Task.Delay(20, timeout.Token);
            Assert.InRange(player.Length, 8000, 30000);
            var forward = player.Length / 2;
            player.Time = forward;
            while (player.Time < forward - 300)
                await Task.Delay(20, timeout.Token);
            Assert.InRange(player.Time, forward - 300, forward + 1000);
            player.Time = 1000;
            while (player.Time > 2500)
                await Task.Delay(20, timeout.Token);
            Assert.InRange(player.Time, 500, 2500);
            await ended.Task.WaitAsync(timeout.Token);
            Assert.Null(failure);
            Assert.Contains(media.Tracks, t => t.TrackType == TrackType.Video && t.Codec == 0x34363268);
            Assert.Contains(media.Tracks, t => t.TrackType == TrackType.Audio && t.Codec == 0x6134706d);
        }
        finally
        {
            input.Cancel();
            player.Stop();
        }
    }

    [Fact]
    public Task Desktop_overlay_has_real_position_pause_and_duration_controls()
        => HeadlessUnitTestSession.GetOrStartForAssembly(typeof(PodcastReplayTests).Assembly).Dispatch(() =>
        {
            var overlay = new PostIt.Controls.ReplayControls();
            var window = new Window { Content = overlay, Width = 640, Height = 360 };
            try
            {
                window.Show();
                Assert.NotNull(overlay.FindControl<Slider>("PositionSlider"));
                Assert.NotNull(overlay.FindControl<Button>("ToggleButton"));
                Assert.Equal("0:00 / 0:00", overlay.FindControl<TextBlock>("TimeLabel")?.Text);
            }
            finally { window.Close(); }
        }, _ct);

    [Fact]
    public async Task Ranges_use_authenticated_factory_with_bounded_exact_content_and_no_token_in_URL()
    {
        await using var transport = new RangeTransport();
        var api = new PodcastApiClient(transport, () => "https://api.example/");
        Assert.Equal(transport.Bytes.Skip(12).Take(3), await api.DownloadReplayRangeAsync(Id, 12, 3, transport.Bytes.Length, _ct));
        Assert.Equal("bytes=12-14", transport.Range);
        Assert.Equal($"https://api.example/api/v1/podcasts/{Id}/replay.mp4", transport.Url);
        Assert.Equal(transport.Bytes.Skip(transport.Bytes.Length - 2),
            await api.DownloadReplayRangeAsync(Id, transport.Bytes.Length - 2, 256 * 1024, transport.Bytes.Length, _ct));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            api.DownloadReplayRangeAsync(Id, 0, 256 * 1024 + 1, transport.Bytes.Length, _ct));
    }

    [Theory]
    [InlineData("full")]
    [InlineData("range")]
    [InlineData("size")]
    [InlineData("extra")]
    public async Task Untrusted_or_unbounded_responses_fail_instead_of_playing_wrong_bytes(string failure)
    {
        await using var transport = new RangeTransport { Failure = failure };
        var api = new PodcastApiClient(transport, () => "https://api.example/");
        await Assert.ThrowsAsync<InvalidDataException>(() => api.DownloadReplayRangeAsync(Id, 0, 8, transport.Bytes.Length, _ct));
    }

    [Fact]
    public void Native_input_supports_backward_and_forward_seek_EOF_and_cancellation()
    {
        using var transport = new RangeTransport();
        var api = new PodcastApiClient(transport, () => "https://api.example/");
        using var input = new PodcastReplayMediaInput(api, Id, transport.Bytes.Length);
        Assert.True(input.CanSeek);
        var buffer = Marshal.AllocHGlobal(16);
        try
        {
            Assert.True(input.Open(out var size));
            Assert.Equal((ulong)transport.Bytes.Length, size);
            Assert.True(input.CanSeek);
            Assert.Equal(16, input.Read(buffer, 16));
            Assert.Equal(0, Marshal.ReadByte(buffer));
            Assert.True(input.Seek(300000));
            Assert.Equal(16, input.Read(buffer, 16));
            Assert.Equal(transport.Bytes[300000], Marshal.ReadByte(buffer));
            Assert.True(input.Seek(2));
            Assert.Equal(16, input.Read(buffer, 16));
            Assert.Equal(2, Marshal.ReadByte(buffer));
            Assert.True(input.Seek(size - 1));
            Assert.Equal(1, input.Read(buffer, 16));
            Assert.Equal(0, input.Read(buffer, 16));
            Assert.False(input.Seek(size + 1));
            string? error = null;
            input.Failed += message => error = message;
            input.Cancel();
            Assert.Equal(-1, input.Read(buffer, 16));
            Assert.Null(error);
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }

    [Fact]
    public void Controls_hide_at_three_seconds_but_not_during_drag_pause_or_keyboard_focus()
    {
        var clock = new Clock();
        var controls = new ReplayControlsState(clock);
        Assert.True(controls.IsVisible(true, false));
        clock.Now += TimeSpan.FromMilliseconds(2999);
        Assert.True(controls.IsVisible(true, false));
        clock.Now += TimeSpan.FromMilliseconds(1);
        Assert.False(controls.IsVisible(true, false));
        Assert.True(controls.IsVisible(false, false));
        Assert.True(controls.IsVisible(true, true));
        controls.IsInteracting = true;
        Assert.True(controls.IsVisible(true, false));
        controls.IsInteracting = false;
        controls.Show();
        Assert.True(controls.IsVisible(true, false));
        Assert.Equal("0:00", ReplayControlsState.FormatTime(-1));
        Assert.Equal("1:02", ReplayControlsState.FormatTime(62000));
        Assert.Equal("1:01:02", ReplayControlsState.FormatTime(3662000));
    }

    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class RangeTransport : IYavscApiClient, IDisposable
    {
        public byte[] Bytes { get; init; } = Enumerable.Range(0, 600000).Select(i => (byte)i).ToArray();
        public string? Failure { get; init; }
        public string? Range { get; private set; }
        public string? Url { get; private set; }
        public HttpClient Http { get; } = new();
        public Task<HttpResponseMessage> SendAuthenticatedAsync(Func<HttpRequestMessage> factory, CancellationToken ct = default)
        {
            using var request = factory();
            Range = request.Headers.Range?.ToString();
            Url = request.RequestUri!.AbsoluteUri;
            var item = request.Headers.Range!.Ranges.Single();
            var from = item.From!.Value;
            var to = item.To!.Value;
            var response = new HttpResponseMessage(Failure == "full" ? HttpStatusCode.OK : HttpStatusCode.PartialContent)
            {
                Content = new ByteArrayContent(Bytes[(int)from..(int)(to + 1 + (Failure == "extra" ? 1 : 0))])
            };
            response.Content.Headers.ContentRange = new ContentRangeHeaderValue(
                Failure == "range" ? from + 1 : from, to, Failure == "size" ? Bytes.Length + 1 : Bytes.Length);
            return Task.FromResult(response);
        }
        public Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<T> CallAsync<T>(HttpMethod method, string path, Func<HttpContent> factory, CancellationToken ct = default) => throw new NotSupportedException();
        public Task CallAsync(HttpMethod method, string path, object? body = null, CancellationToken ct = default) => throw new NotSupportedException();
        public Task CallAsync(HttpMethod method, string path, Func<HttpContent> factory, CancellationToken ct = default) => throw new NotSupportedException();
        public ValueTask DisposeAsync() { Dispose(); return ValueTask.CompletedTask; }
        public void Dispose() => Http.Dispose();
    }
}
