using System.Net;
using Avalonia.Headless.XUnit;
using Avalonia.Headless;
using Avalonia.Controls;
using PostIt.Services;
using PostIt.ViewModels;
using PostIt.Views;
using Yavsc.Abstract.Streaming;
using Yavsc.Api.Client;

namespace PostIt.Tests;

public sealed class PodcastBroadcasterTests
{
    [Theory]
    [InlineData("stop")]
    [InlineData("leave")]
    [InlineData("logout")]
    [InlineData("failure")]
    [InlineData("broadcast")]
    public Task Camera_can_be_previewed_without_creating_a_broadcast_and_is_released_before_next_capture(string action)
        => HeadlessUnitTestSession.GetOrStartForAssembly(typeof(PodcastBroadcasterTests).Assembly).Dispatch(async () =>
    {
        var path = Path.Combine(Path.GetTempPath(), $"postit-local-preview-{Guid.NewGuid():N}");
        var previousDevices = Platform.GetPodcastCaptureDevices;
        var previousPreview = Platform.CreatePodcastPreviewAsync;
        var previousCapture = Platform.CreatePodcastCaptureAsync;
        var previousPermissions = Platform.RequestPodcastPermissionsAsync;
        var store = new TokenStore(path);
        store.Save(new RefreshTokenRecord("access", "refresh", DateTimeOffset.UtcNow.AddHours(1)));
        await using var api = new YavscApiClient(new Settings(path + ".settings"), store);
        await using var transport = new Transport();
        var preview = new PreviewCapture();
        PodcastsPageViewModel? vm = null;
        try
        {
            Platform.GetPodcastCaptureDevices = () => new([new("Camera", "camera")], [new("Mic", "microphone")]);
            Platform.CreatePodcastPreviewAsync = (camera, _) =>
            {
                Assert.Equal("camera", camera);
                return Task.FromResult<IPodcastCapture>(preview);
            };
            Platform.RequestPodcastPermissionsAsync = (_, _) => Task.CompletedTask;
            Platform.CreatePodcastCaptureAsync = (_, microphone, directory, _) =>
            {
                Assert.True(preview.Disposed);
                Assert.Equal("microphone", microphone);
                return Task.FromResult<IPodcastCapture>(new FakeCapture(directory, new Border()));
            };
            var session = new SessionStatusViewModel { Api = api };
            session.Refresh();
            vm = new PodcastsPageViewModel(new PodcastApiClient(transport, () => "https://api.example/"), api, session);
            vm.DetectDevicesCommand.Execute(null);
            vm.Title = "";
            vm.KeepScreenOnDuringCapture = true;
            Assert.False(Platform.ScreenAwake.IsRequested);
            Assert.True(vm.PreviewCommand.CanExecute(null));
            Assert.False(vm.StartCommand.CanExecute(null));
            await vm.PreviewCommand.ExecuteAsync(null);
            Assert.True(vm.IsPreviewing);
            Assert.True(Platform.ScreenAwake.IsRequested);
            vm.KeepScreenOnDuringCapture = false;
            Assert.False(Platform.ScreenAwake.IsRequested);
            vm.KeepScreenOnDuringCapture = true;
            Assert.True(Platform.ScreenAwake.IsRequested);
            Assert.True(vm.IsCapturing);
            Assert.False(vm.IsBroadcasting);
            Assert.Same(preview.Preview, vm.CapturePreview);
            Assert.False(vm.DetectDevicesCommand.CanExecute(null));
            Assert.True(vm.StopPreviewCommand.CanExecute(null));
            Assert.Equal(0, transport.CreateCount);
            Assert.Empty(transport.Uploads);
            var page = new PodcastsPage { DataContext = vm };
            Assert.Same(preview.Preview, page.FindControl<ContentControl>("CapturePreviewHost")!.Content);
            preview.OnDispose = () => Assert.Null(vm.CapturePreview);
            switch (action)
            {
                case "stop": await vm.StopPreviewCommand.ExecuteAsync(null); break;
                case "leave": await vm.DeactivateAsync(); break;
                case "logout": await session.LogoutAsync(); break;
                case "failure": preview.Fail(); break;
                case "broadcast":
                    vm.Title = "Emission";
                    Assert.True(vm.StartCommand.CanExecute(null));
                    await vm.StartCommand.ExecuteAsync(null);
                    Assert.True(vm.IsBroadcasting);
                    Assert.True(Platform.ScreenAwake.IsRequested);
                    Assert.Equal(1, transport.CreateCount);
                    await vm.StopAsync();
                    break;
            }
            await WaitForUiAsync(() => preview.Disposed && !vm.IsPreviewing && !vm.IsBroadcasting);
            Assert.True(preview.Disposed);
            Assert.False(vm.IsPreviewing);
            Assert.False(Platform.ScreenAwake.IsRequested);
            Assert.Null(vm.CapturePreview);
            if (action == "failure") Assert.Contains("Camera disconnected", vm.Status);
            if (action != "broadcast") Assert.Equal(0, transport.CreateCount);
            return true;
        }
        finally
        {
            if (vm is not null) await vm.DeactivateAsync();
            Platform.GetPodcastCaptureDevices = previousDevices;
            Platform.CreatePodcastPreviewAsync = previousPreview;
            Platform.CreatePodcastCaptureAsync = previousCapture;
            Platform.RequestPodcastPermissionsAsync = previousPermissions;
            File.Delete(path);
            File.Delete(path + ".settings");
        }
    }, TestContext.Current.CancellationToken);

    [Fact]
    public Task Leaving_the_page_during_preview_start_disposes_the_late_camera_without_showing_it()
        => HeadlessUnitTestSession.GetOrStartForAssembly(typeof(PodcastBroadcasterTests).Assembly).Dispatch(async () =>
    {
        var path = Path.Combine(Path.GetTempPath(), $"postit-preview-race-{Guid.NewGuid():N}");
        var previous = Platform.CreatePodcastPreviewAsync;
        var previousDevices = Platform.GetPodcastCaptureDevices;
        var store = new TokenStore(path);
        store.Save(new RefreshTokenRecord("access", "refresh", DateTimeOffset.UtcNow.AddHours(1)));
        await using var api = new YavscApiClient(new Settings(path + ".settings"), store);
        var preview = new PreviewCapture();
        var pending = new TaskCompletionSource<IPodcastCapture>(TaskCreationOptions.RunContinuationsAsynchronously);
        try
        {
            Platform.GetPodcastCaptureDevices = () => new([new("Camera", "camera")], []);
            Platform.CreatePodcastPreviewAsync = (_, _) => pending.Task;
            var session = new SessionStatusViewModel { Api = api };
            session.Refresh();
            var vm = new PodcastsPageViewModel(new PodcastApiClient(api, () => "https://api.example/"), api, session);
            vm.DetectDevicesCommand.Execute(null);
            var start = vm.PreviewCommand.ExecuteAsync(null);
            var leave = vm.DeactivateAsync();
            pending.SetResult(preview);
            await WaitForUiAsync(() => start.IsCompleted && leave.IsCompleted);
            await start;
            await leave;
            Assert.True(preview.Disposed);
            Assert.Null(vm.CapturePreview);
            Assert.False(vm.IsPreviewing);
            return true;
        }
        finally
        {
            Platform.CreatePodcastPreviewAsync = previous;
            Platform.GetPodcastCaptureDevices = previousDevices;
            File.Delete(path);
            File.Delete(path + ".settings");
        }
    }, TestContext.Current.CancellationToken);

    private static async Task WaitForUiAsync(Func<bool> done)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        while (!done())
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            await Task.Delay(10, timeout.Token);
        }
    }

    private sealed class PreviewCapture : IPodcastCapture
    {
        private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Task Completion => _completion.Task;
        public Control? Preview { get; } = new Border();
        public bool Disposed { get; private set; }
        public Action? OnDispose { get; set; }
        public void Fail() => _completion.TrySetException(new IOException("Camera disconnected"));
        public Task StopAsync() { _completion.TrySetResult(); return Task.CompletedTask; }
        public async ValueTask DisposeAsync()
        {
            OnDispose?.Invoke();
            await StopAsync();
            Disposed = true;
        }
    }

    [Fact]
    public Task Shared_page_exposes_running_capture_preview_and_clears_it_before_disposal()
        => HeadlessUnitTestSession.GetOrStartForAssembly(typeof(PodcastBroadcasterTests).Assembly).Dispatch(async () =>
        {
            var tokenPath = Path.Combine(Path.GetTempPath(), $"postit-preview-{Guid.NewGuid():N}.json");
            var devices = Platform.GetPodcastCaptureDevices;
            var permissions = Platform.RequestPodcastPermissionsAsync;
            var factory = Platform.CreatePodcastCaptureAsync;
            var store = new TokenStore(tokenPath);
            store.Save(new RefreshTokenRecord("test-access-token", "test-refresh-token", DateTimeOffset.UtcNow.AddHours(1), null));
            await using var api = new YavscApiClient(new Settings(tokenPath + ".settings"), store);
            await using var transport = new Transport();
            FakeCapture? capture = null;
            PodcastsPageViewModel? vm = null;
            try
            {
                var preview = new Border();
                var disposed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                Platform.GetPodcastCaptureDevices = () => new([new("Camera", "camera")], [new("None", "")]);
                Platform.RequestPodcastPermissionsAsync = (_, _) => Task.CompletedTask;
                Platform.CreatePodcastCaptureAsync = (_, _, directory, _) =>
                {
                    capture = new FakeCapture(directory, preview)
                    {
                        OnDispose = () =>
                        {
                            Assert.Null(vm!.CapturePreview);
                            disposed.TrySetResult();
                        }
                    };
                    return Task.FromResult<IPodcastCapture>(capture);
                };
                var session = new SessionStatusViewModel { Api = api };
                session.Refresh();
                vm = new PodcastsPageViewModel(new PodcastApiClient(transport, () => "https://api.example/"), api, session)
                {
                    Title = "Preview"
                };
                vm.DetectDevicesCommand.Execute(null);
                await vm.StartCommand.ExecuteAsync(null);
                Assert.Same(preview, vm.CapturePreview);
                Assert.True(vm.IsBroadcasting);
                var page = new PodcastsPage { DataContext = vm };
                var host = page.FindControl<ContentControl>("CapturePreviewHost");
                Assert.NotNull(host);
                Assert.Same(preview, host.Content);
                await vm.DeactivateAsync();
                await disposed.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
                Assert.Null(vm.CapturePreview);
                Assert.Null(host.Content);
                Assert.True(capture!.Stopped);
                return true;
            }
            finally
            {
                if (vm?.IsBroadcasting == true) await vm.StopAsync();
                Platform.GetPodcastCaptureDevices = devices;
                Platform.RequestPodcastPermissionsAsync = permissions;
                Platform.CreatePodcastCaptureAsync = factory;
                File.Delete(tokenPath);
                File.Delete(tokenPath + ".settings");
            }
        }, TestContext.Current.CancellationToken);

    [AvaloniaFact]
    public async Task Shared_page_uses_platform_devices_and_requests_permissions_before_creating_server_broadcast()
    {
        var tokenPath = Path.Combine(Path.GetTempPath(), $"postit-podcast-{Guid.NewGuid():N}.json");
        var devices = Platform.GetPodcastCaptureDevices;
        var permissions = Platform.RequestPodcastPermissionsAsync;
        var factory = Platform.CreatePodcastCaptureAsync;
        var store = new TokenStore(tokenPath);
        store.Save(new RefreshTokenRecord("test-access-token", "test-refresh-token", DateTimeOffset.UtcNow.AddHours(1), null));
        await using var api = new YavscApiClient(new Settings(tokenPath + ".settings"), store);
        await using var transport = new Transport();
        try
        {
            Platform.GetPodcastCaptureDevices = () => new(
                [new("Front", "front"), new("Rear", "rear")],
                [new("None", ""), new("Android microphone", "microphone")]);
            Platform.RequestPodcastPermissionsAsync = (_, _) =>
                Task.FromException(new UnauthorizedAccessException("Camera denied"));
            Platform.CreatePodcastCaptureAsync = (_, _, _, _) =>
                throw new InvalidOperationException("Capture must not start after permission denial.");
            var session = new SessionStatusViewModel { Api = api };
            session.Refresh();
            var vm = new PodcastsPageViewModel(new PodcastApiClient(transport, () => "https://api.example/"), api, session)
            {
                Title = "Android live"
            };
            Assert.True(vm.CanBroadcast);
            vm.DetectDevicesCommand.Execute(null);
            Assert.Equal(2, vm.Cameras.Count);
            Assert.Equal("front", vm.SelectedCamera?.Value);
            Assert.Equal("microphone", vm.SelectedMicrophone?.Value);
            Assert.True(vm.StartCommand.CanExecute(null));
            await vm.StartCommand.ExecuteAsync(null);
            Assert.Contains("Camera denied", vm.Status);
            Assert.Equal(0, transport.CreateCount);
            Assert.False(vm.IsBusy);
            Assert.False(vm.IsBroadcasting);
        }
        finally
        {
            Platform.GetPodcastCaptureDevices = devices;
            Platform.RequestPodcastPermissionsAsync = permissions;
            Platform.CreatePodcastCaptureAsync = factory;
            File.Delete(tokenPath);
            File.Delete(tokenPath + ".settings");
        }
    }

    [Fact]
    public async Task Prepared_broadcast_starts_the_existing_emission_instead_of_creating_another()
    {
        await using var transport = new Transport();
        var client = new PodcastApiClient(transport, () => "https://api.example/");
        FakeCapture? capture = null;
        await using var broadcaster = await PodcastBroadcaster.StartAsync(client, "Prepared",
            (directory, _) => Task.FromResult<IPodcastCapture>(capture = new FakeCapture(directory)),
            TestContext.Current.CancellationToken, transport.PodcastId);
        Assert.Equal(transport.PodcastId, broadcaster.Podcast.Id);
        Assert.Equal(transport.PodcastId, transport.StartedId);
        Assert.Equal(0, transport.CreateCount);
        await capture!.StopAsync();
        await broadcaster.Completion.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Native_stop_flushes_final_segment_before_completing_server_replay()
    {
        await using var transport = new Transport();
        var client = new PodcastApiClient(transport, () => "https://api.example/");
        FakeCapture? capture = null;
        await using var broadcaster = await PodcastBroadcaster.StartAsync(client, "Android",
            (directory, _) =>
            {
                capture = new FakeCapture(directory);
                return Task.FromResult<IPodcastCapture>(capture);
            }, TestContext.Current.CancellationToken);
        await transport.FirstUpload.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await capture!.StopAsync();
        await broadcaster.Completion.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal(new[] { 0, 1 }, transport.Uploads);
        Assert.Equal(new[] { "upload:0", "upload:1", "complete" }, transport.Events);
        Assert.True(capture.Stopped);
    }

    [Fact]
    public async Task Native_failure_stops_capture_and_preserves_unacknowledged_segments()
    {
        await using var transport = new Transport { FailUpload = true };
        var client = new PodcastApiClient(transport, () => "https://api.example/");
        FakeCapture? capture = null;
        var broadcaster = await PodcastBroadcaster.StartAsync(client, "Android",
            (directory, _) =>
            {
                capture = new FakeCapture(directory);
                return Task.FromResult<IPodcastCapture>(capture);
            }, TestContext.Current.CancellationToken);
        try
        {
            await Assert.ThrowsAsync<HttpRequestException>(() => broadcaster.Completion);
            Assert.True(capture!.Stopped);
            Assert.True(File.Exists(Path.Combine(capture.Directory, "segment-00000000.ts")));
            Assert.DoesNotContain("complete", transport.Events);
            await Assert.ThrowsAsync<HttpRequestException>(() => broadcaster.DisposeAsync().AsTask());
            Assert.True(capture.Disposed);
        }
        finally
        {
            if (capture is not null) Directory.Delete(capture.Directory, recursive: true);
        }
    }

    [Fact]
    public async Task Startup_failure_finalizes_empty_server_broadcast()
    {
        await using var transport = new Transport();
        var client = new PodcastApiClient(transport, () => "https://api.example/");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => PodcastBroadcaster.StartAsync(client, "Denied",
            (_, _) => Task.FromException<IPodcastCapture>(new UnauthorizedAccessException("Camera denied")),
            TestContext.Current.CancellationToken));
        Assert.Equal(new[] { "complete" }, transport.Events);
    }

    private sealed class FakeCapture : IPodcastCapture
    {
        private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public string Directory { get; }
        public bool Stopped { get; private set; }
        public bool Disposed { get; private set; }
        public Control? Preview { get; }
        public Action? OnDispose { get; init; }
        public Task Completion => _completion.Task;
        public FakeCapture(string directory, Control? preview = null)
        {
            Preview = preview;
            Directory = directory;
            File.WriteAllBytes(Path.Combine(directory, "segment-00000000.ts"), [0x47, 1]);
            File.WriteAllText(Path.Combine(directory, "index.m3u8"), "#EXTM3U\n#EXTINF:2,\nsegment-00000000.ts\n");
        }
        public Task StopAsync()
        {
            if (Stopped) return Task.CompletedTask;
            File.WriteAllBytes(Path.Combine(Directory, "segment-00000001.ts"), [0x47, 2]);
            File.AppendAllText(Path.Combine(Directory, "index.m3u8"), "#EXTINF:0.5,\nsegment-00000001.ts\n");
            Stopped = true;
            _completion.TrySetResult();
            return Task.CompletedTask;
        }
        public ValueTask DisposeAsync() { Disposed = true; OnDispose?.Invoke(); return ValueTask.CompletedTask; }
    }

    private sealed class Transport : IYavscApiClient
    {
        private readonly PodcastResponse _podcast = new(Guid.NewGuid(), "Test", "alice", "Alice",
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, PodcastState.Live, 0, 0, 0);
        public HttpClient Http { get; } = new();
        public List<int> Uploads { get; } = [];
        public List<string> Events { get; } = [];
        public bool FailUpload { get; init; }
        public int CreateCount { get; private set; }
        public Guid PodcastId => _podcast.Id;
        public Guid? StartedId { get; private set; }
        public TaskCompletionSource FirstUpload { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            if (path.EndsWith("/complete", StringComparison.Ordinal)) Events.Add("complete");
            else if (path.EndsWith("/start", StringComparison.Ordinal))
                StartedId = Guid.Parse(new Uri(path).Segments[^2].TrimEnd('/'));
            else CreateCount++;
            return Task.FromResult((T)(object)_podcast);
        }
        public Task<T> CallAsync<T>(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default)
        {
            if (FailUpload) throw new HttpRequestException("Denied", null, HttpStatusCode.Forbidden);
            var sequence = int.Parse(new Uri(path).Segments[^1], System.Globalization.CultureInfo.InvariantCulture);
            Uploads.Add(sequence);
            Events.Add($"upload:{sequence}");
            FirstUpload.TrySetResult();
            return Task.FromResult((T)(object)_podcast);
        }
        public Task CallAsync(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
            => CallAsync<PodcastResponse>(method, path, body, ct);
        public Task CallAsync(HttpMethod method, string path, Func<HttpContent> contentFactory, CancellationToken ct = default)
            => CallAsync<PodcastResponse>(method, path, contentFactory, ct);
        public Task<byte[]> DownloadAsync(HttpMethod method, string path, CancellationToken ct = default)
            => throw new NotSupportedException();
        public ValueTask DisposeAsync() { Http.Dispose(); return ValueTask.CompletedTask; }
    }
}
