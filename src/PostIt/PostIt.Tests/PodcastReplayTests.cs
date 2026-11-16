using System.Net;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using LibVLCSharp.Shared;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Interactivity;
using PostIt.Services;
using Yavsc.Abstract.Streaming;
using Yavsc.Api.Client;

namespace PostIt.Tests;

[Trait("DependsOn", "Native")]
[Trait("Speed", "Slow")]
[Trait("Category", "non-reg")]
[Collection(AvaloniaUiCollection.Name)]
public sealed class PodcastReplayTests(AvaloniaUiFixture ui)
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
        => ui.RunAsync(() =>
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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Desktop_controls_show_seeking_only_for_replay(bool replay)
        => ui.RunAsync(() =>
    {
        var controls = new PostIt.Controls.ReplayControls(replay);
        Assert.NotNull(controls.FindControl<Button>("ToggleButton"));
        Assert.Equal(replay, controls.FindControl<Slider>("PositionSlider")!.IsVisible);
        Assert.Equal(replay, controls.FindControl<TextBlock>("TimeLabel")!.IsVisible);
    }, TestContext.Current.CancellationToken);

    [Fact(Skip = "Set POSTIT_TEST_REPLAY to a local MP4 and install LibVLC.", SkipUnless = nameof(HasNativeFixture))]
    public Task Desktop_live_controls_pause_and_resume_the_player()
        => ui.RunAsync(async () =>
    {
        Core.Initialize();
        using var vlc = new LibVLC("--vout=dummy", "--aout=dummy", "--no-xlib");
        using var player = new MediaPlayer(vlc);
        using var media = new Media(vlc, Environment.GetEnvironmentVariable("POSTIT_TEST_REPLAY")!, FromType.FromPath);
        media.AddOption(":input-repeat=100");
        var controls = new PostIt.Controls.ReplayControls(player, replay: false);
        var toggle = controls.FindControl<Button>("ToggleButton")!;
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(_ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(15));
        async Task WaitForPlayback(bool playing)
        {
            while (player.IsPlaying != playing)
                await Task.Delay(20, timeout.Token);
        }

        Assert.True(player.Play(media));
        try
        {
            await WaitForPlayback(true);
            toggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            await WaitForPlayback(false);
            toggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            await WaitForPlayback(true);
        }
        finally { player.Stop(); }
    }, TestContext.Current.CancellationToken);

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

    [Fact]
    public async Task Draft_client_targets_owner_routes_and_reads_the_draft_source_by_range()
    {
        await using var transport = new RangeTransport();
        var api = new PodcastApiClient(transport, () => "https://api.example/");
        var prefix = $"https://api.example/api/v1/podcasts";
        await api.CreateAsync(new CreatePodcastRequest { Title = "Live", RefineReplay = true, RetentionDays = 0 }, _ct);
        var save = new SavePodcastDraftRequest { Revision = 2, Marks = [1000], RemovedRanges = [new(0, 500)] };
        await api.SaveDraftAsync(Id, save, _ct);
        await api.PublishDraftAsync(Id, 3, _ct);
        await api.DeleteOriginalsAsync(Id, _ct);
        Assert.Equal([$"POST {prefix}", $"PUT {prefix}/{Id}/draft", $"POST {prefix}/{Id}/draft/publish",
            $"DELETE {prefix}/{Id}/originals"], transport.Calls.Select(call => call.Call));
        var create = Assert.IsType<CreatePodcastRequest>(transport.Calls[0].Body);
        Assert.True(create.RefineReplay);
        Assert.Equal(0, create.RetentionDays);
        Assert.Same(save, transport.Calls[1].Body);
        Assert.Equal(3, Assert.IsType<PublishPodcastDraftRequest>(transport.Calls[2].Body).Revision);
        Assert.Null(transport.Calls[3].Body);

        Assert.Equal(transport.Bytes.Skip(5).Take(4), await api.DownloadDraftSourceRangeAsync(Id, 5, 4, transport.Bytes.Length, _ct));
        Assert.Equal($"{prefix}/{Id}/draft/source.mp4", transport.Url);
        Assert.Equal("bytes=5-8", transport.Range);
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

    [Fact]
    public void Drag_preview_keeps_only_the_latest_new_position()
    {
        var state = new ReplayControlsState();
        Assert.False(state.TryTakePreview(out _));
        state.RequestPreview(1000);
        state.RequestPreview(2000);
        Assert.True(state.TryTakePreview(out var time));
        Assert.Equal(2000, time);
        Assert.False(state.TryTakePreview(out _));
        state.RequestPreview(2000);
        Assert.False(state.TryTakePreview(out _));
        state.EndPreview();
        state.RequestPreview(2000);
        Assert.True(state.TryTakePreview(out time));
        Assert.Equal(2000, time);
    }

    public static bool HasFfmpeg => (Environment.GetEnvironmentVariable("PATH") ?? "")
        .Split(Path.PathSeparator).Any(dir => File.Exists(Path.Combine(dir, "ffmpeg")));

    [Fact(Skip = "Install ffmpeg and LibVLC.", SkipUnless = nameof(HasFfmpeg))]
    public async Task LibVLC_shows_the_exact_frame_after_seeking_while_paused()
    {
        const int Fps = 25, Size = 64;
        var path = Path.Combine(Path.GetTempPath(), $"postit-frames-{Guid.NewGuid():N}.mp4");
        try
        {
            // Each frame N has luma 2N; keyframes every 2 s so seeks land between keyframes.
            var source = "color=black:s=" + Size + "x" + Size + ":r=" + Fps + ":d=4,format=yuv420p,geq=lum='N*2':cb=128:cr=128";
            var start = new System.Diagnostics.ProcessStartInfo("ffmpeg");
            foreach (var argument in new[] { "-v", "error", "-f", "lavfi", "-i", source,
                         "-c:v", "libx264", "-qp", "0", "-g", "50", "-pix_fmt", "yuv420p", path })
                start.ArgumentList.Add(argument);
            using (var ffmpeg = System.Diagnostics.Process.Start(start))
            {
                Assert.NotNull(ffmpeg);
                await ffmpeg.WaitForExitAsync(_ct);
                Assert.Equal(0, ffmpeg.ExitCode);
            }

            var frame = -1;
            var buffer = Marshal.AllocHGlobal(Size * Size * 4);
            try
            {
                using var vlc = new LibVLC("--no-audio");
                using var player = new MediaPlayer(vlc);
                using var media = new Media(vlc, path, FromType.FromPath);
                player.SetVideoFormat("RV32", Size, Size, Size * 4);
                player.SetVideoCallbacks((_, planes) => { Marshal.WriteIntPtr(planes, buffer); return IntPtr.Zero; }, null,
                    (_, _) =>
                    {
                        // Undo the limited-range YUV to RGB conversion on the red channel.
                        var red = Marshal.ReadByte(buffer, (Size * (Size / 2) + Size / 2) * 4 + 2);
                        Volatile.Write(ref frame, (int)Math.Round((red * 219.0 / 255 + 16) / 2));
                    });
                Assert.True(player.Play(media));
                await WaitFor(() => player.IsPlaying && Volatile.Read(ref frame) >= 0);
                player.SetPause(true);
                await WaitFor(() => !player.IsPlaying);

                foreach (var target in new[] { 37, 73, 12, 61 })
                {
                    player.Time = target * 1000L / Fps + 20;
                    await WaitFor(() => Volatile.Read(ref frame) == target);
                    Assert.Equal(target, Volatile.Read(ref frame));
                    Assert.False(player.IsPlaying);
                }
                player.Stop();
            }
            finally { Marshal.FreeHGlobal(buffer); }
        }
        finally { File.Delete(path); }

        async Task WaitFor(Func<bool> condition)
        {
            for (var i = 0; i < 100 && !condition(); i++) await Task.Delay(50, _ct);
        }
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
        public List<(string Call, object? Body)> Calls { get; } = [];
        public Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            Calls.Add(($"{method} {path}", body));
            return Task.FromResult(default(T)!);
        }
        public Task<T> CallAsync<T>(HttpMethod method, string path, Func<HttpContent> factory, CancellationToken ct = default) => throw new NotSupportedException();
        public Task CallAsync(HttpMethod method, string path, object? body = null, CancellationToken ct = default) => throw new NotSupportedException();
        public Task CallAsync(HttpMethod method, string path, Func<HttpContent> factory, CancellationToken ct = default) => throw new NotSupportedException();
        public ValueTask DisposeAsync() { Dispose(); return ValueTask.CompletedTask; }
        public void Dispose() => Http.Dispose();
    }
}
