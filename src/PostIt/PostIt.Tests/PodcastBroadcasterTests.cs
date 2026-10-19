using System.Net;
using Avalonia.Headless.XUnit;
using PostIt.Services;
using PostIt.ViewModels;
using Yavsc.Abstract.Streaming;
using Yavsc.Api.Client;

namespace PostIt.Tests;

public sealed class PodcastBroadcasterTests
{
    [AvaloniaFact]
    public async Task Shared_page_uses_platform_devices_and_requests_permissions_before_creating_server_broadcast()
    {
        var tokenPath = Path.Combine(Path.GetTempPath(), $"postit-podcast-{Guid.NewGuid():N}.json");
        var devices = Platform.GetPodcastCaptureDevices;
        var permissions = Platform.RequestPodcastPermissionsAsync;
        var factory = Platform.CreatePodcastCaptureAsync;
        var store = new TokenStore(tokenPath);
        store.Save(new RefreshTokenRecord("test-access-token", "test-refresh-token", DateTimeOffset.UtcNow.AddHours(1), null));
        await using var api = new YavscApiClient(new Settings(), store);
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
        }
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
        public Task Completion => _completion.Task;
        public FakeCapture(string directory)
        {
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
        public ValueTask DisposeAsync() { Disposed = true; return ValueTask.CompletedTask; }
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
        public TaskCompletionSource FirstUpload { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Task<T> CallAsync<T>(HttpMethod method, string path, object? body = null, CancellationToken ct = default)
        {
            if (path.EndsWith("/complete", StringComparison.Ordinal)) Events.Add("complete");
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
