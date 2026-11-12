using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Yavsc.Abstract.Streaming;
using Yavsc.Api.Services;
using Yavsc.Models;
using Yavsc.Server.Helpers;
using Yavsc.Services;
using Yavsc.Settings;

namespace Yavsc.Api.Test;

[Trait("Category", "non-reg")]
public sealed class PodcastObsTests : IAsyncDisposable
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private readonly string root = Path.Combine(Path.GetTempPath(), "yavsc-srt-" + Guid.NewGuid().ToString("N"));
    private readonly SqliteConnection connection;
    private readonly ServiceProvider provider;
    private readonly PodcastObsOptions settings = new()
    {
        Enabled = true, ServerUrl = "srt://127.0.0.1", ListenAddress = "127.0.0.1",
        FirstPort = FreePort(), PortCount = 1, MaxConcurrentBroadcasts = 1, MaxDurationMinutes = 1
    };
    private readonly Clock clock = new();
    private sealed class Clock : TimeProvider
    {
        private readonly object gate = new();
        private DateTimeOffset now = DateTimeOffset.UtcNow;
        public DateTimeOffset Now { get { lock (gate) return now; } set { lock (gate) now = value; } }
        public override DateTimeOffset GetUtcNow() => Now;
    }

    public PodcastObsTests()
    {
        var connectionString = $"Data Source=srt-{Guid.NewGuid():N};Mode=Memory;Cache=Shared;Default Timeout=30";
        connection = new SqliteConnection(connectionString);
        connection.Open();
        var services = new ServiceCollection();
        services.AddLogging(logging => logging.AddConsole().AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning));
        services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(connectionString));
        services.AddSingleton<TimeProvider>(clock);
        services.AddSingleton<IOptions<PodcastObsOptions>>(Options.Create(settings));
        services.AddSingleton<IOptions<PodcastOptions>>(Options.Create(new PodcastOptions { RootPath = root }));
        services.AddSingleton<IOptions<ResourceUsageOptions>>(Options.Create(new ResourceUsageOptions()));
        services.AddSingleton<ILiveProcessor, LiveProcessor>();
        services.AddScoped<IPodcastScheduleService, PodcastScheduleService>();
        services.AddScoped<IPodcastObsService, PodcastObsService>();
        services.AddSingleton<PodcastObsWorker>();
        provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();
    }

    private async Task<PodcastResponse> PlanAsync(IServiceProvider services)
    {
        await provider.GetRequiredService<PodcastObsWorker>().StartAsync(Ct);
        var db = services.GetRequiredService<ApplicationDbContext>();
        db.Users.Add(new() { Id = "owner", UserName = "owner", Email = "owner@example.test" });
        await db.SaveChangesAsync(Ct);
        return await services.GetRequiredService<IPodcastScheduleService>().PrepareAsync("owner", new()
        {
            Title = "OBS programme", ScheduledStartAt = clock.Now.AddDays(7), PlannedDurationMinutes = 1
        }, null, Ct);
    }

    [Fact]
    public async Task Activation_is_owned_rotatable_expiring_and_keeps_secrets_only_in_memory()
    {
        using var scope = provider.CreateScope();
        var plan = await PlanAsync(scope.ServiceProvider);
        var obs = scope.ServiceProvider.GetRequiredService<IPodcastObsService>();
        Assert.Equal(403, (await Assert.ThrowsAsync<PodcastException>(() => obs.ActivateAsync(plan.Id, "other", Ct))).StatusCode);
        var credentials = await obs.ActivateAsync(plan.Id, "owner", Ct);
        var secret = QueryHelpers.ParseQuery(new Uri(credentials.ServerUrl).Query)["passphrase"].ToString();
        Assert.Empty(credentials.StreamKey);
        Assert.Equal(64, secret.Length);
        Assert.Equal(clock.Now.AddMinutes(30), credentials.ExpiresAt);
        Assert.Contains("pbkeylen=32", credentials.ServerUrl);
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var flow = await db.LiveFlow.AsNoTracking().SingleAsync(Ct);
        Assert.DoesNotContain(secret, flow.ObsKeyHash!);
        Assert.NotNull(flow.ObsConnectionId);
        Assert.DoesNotContain(secret, JsonSerializer.Serialize(flow));
        Assert.Empty(await provider.GetRequiredService<ILiveProcessor>().ListAsync(Ct));
        var renewed = await obs.ActivateAsync(plan.Id, "owner", Ct);
        Assert.NotEqual(credentials.ServerUrl, renewed.ServerUrl);
        clock.Now = renewed.ExpiresAt;
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(Ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));
        await WaitAsync(async () => !(await obs.StatusAsync(plan.Id, "owner", timeout.Token)).Enabled, timeout.Token);
        await WaitAsync(async () => (await obs.StatusAsync(plan.Id, "owner", timeout.Token)).Error is not null, timeout.Token);
        Assert.Empty(await provider.GetRequiredService<ILiveProcessor>().ListAsync(Ct));
    }

    [Fact]
    public async Task Activation_blocks_exhausted_quotas_direct_capture_and_excess_listeners()
    {
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;
        var plan = await PlanAsync(services);
        var obs = services.GetRequiredService<IPodcastObsService>();
        var db = services.GetRequiredService<ApplicationDbContext>();
        var owner = await db.Users.SingleAsync(Ct);
        owner.DiskUsage = owner.DiskQuota;
        await db.SaveChangesAsync(Ct);
        Assert.Equal(413, (await Assert.ThrowsAsync<PodcastException>(() => obs.ActivateAsync(plan.Id, "owner", Ct))).StatusCode);
        owner.DiskUsage = 0;
        await db.SaveChangesAsync(Ct);
        await obs.ActivateAsync(plan.Id, "owner", Ct);
        Assert.Equal(409, (await Assert.ThrowsAsync<PodcastException>(() =>
            services.GetRequiredService<IPodcastScheduleService>().StartAsync(plan.Id, "owner", Ct))).StatusCode);
        var another = await services.GetRequiredService<IPodcastScheduleService>().PrepareAsync("owner", new()
        { Title = "Other", ScheduledStartAt = clock.Now.AddDays(1), PlannedDurationMinutes = 1 }, null, Ct);
        Assert.Equal(503, (await Assert.ThrowsAsync<PodcastException>(() => obs.ActivateAsync(another.Id, "owner", Ct))).StatusCode);
        await ResourceUsageTracker.RecordAsync(db, "owner", "owner", cpuSeconds: 20000, cancellationToken: Ct);
        Assert.Equal(429, (await Assert.ThrowsAsync<PodcastException>(() => obs.ActivateAsync(plan.Id, "owner", Ct))).StatusCode);
        await obs.DeactivateAsync(plan.Id, "owner", Ct);
        Assert.False((await obs.StatusAsync(plan.Id, "owner", Ct)).Enabled);
        AssertPortFree(settings.FirstPort);
    }

    [Fact]
    public async Task Unavailable_FFmpeg_and_occupied_ports_fail_activation_without_leaving_a_key()
    {
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;
        var plan = await PlanAsync(services);
        var obs = services.GetRequiredService<IPodcastObsService>();
        using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        {
            socket.Bind(new IPEndPoint(IPAddress.Loopback, settings.FirstPort));
            Assert.Equal(503, (await Assert.ThrowsAsync<PodcastException>(() => obs.ActivateAsync(plan.Id, "owner", Ct))).StatusCode);
        }
        var media = services.GetRequiredService<IOptions<PodcastOptions>>().Value;
        media.FfmpegPath = Path.Combine(root, "ffmpeg-does-not-exist");
        Assert.Equal(503, (await Assert.ThrowsAsync<PodcastException>(() => obs.ActivateAsync(plan.Id, "owner", Ct))).StatusCode);
        Assert.False((await obs.StatusAsync(plan.Id, "owner", Ct)).Enabled);
        await obs.DeactivateAsync(plan.Id, "owner", Ct);
        AssertPortFree(settings.FirstPort);
    }

    [Fact]
    public async Task Restart_invalidates_persisted_activations_and_requires_a_new_secret()
    {
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;
        var plan = await PlanAsync(services);
        var obs = services.GetRequiredService<IPodcastObsService>();
        var original = await obs.ActivateAsync(plan.Id, "owner", Ct);
        await provider.GetRequiredService<PodcastObsWorker>().StopAsync(Ct);
        var db = services.GetRequiredService<ApplicationDbContext>();
        db.ChangeTracker.Clear();
        var flow = await db.LiveFlow.SingleAsync(Ct);
        flow.ObsKeyHash = new string('A', 64);
        flow.ObsEnabledUntil = clock.Now.AddMinutes(30);
        flow.ObsConnectionId = Guid.NewGuid();
        await db.SaveChangesAsync(Ct);
        using var restarted = new PodcastObsWorker(provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(settings), services.GetRequiredService<IOptions<PodcastOptions>>(), clock,
            services.GetRequiredService<ILogger<PodcastObsWorker>>());
        await restarted.StartAsync(Ct);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(Ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        await WaitAsync(async () => !(await obs.StatusAsync(plan.Id, "owner", timeout.Token)).Enabled, timeout.Token);
        Assert.Contains("redemarre", (await obs.StatusAsync(plan.Id, "owner", Ct)).Error);
        var rearmed = new PodcastObsService(db, Options.Create(settings),
            services.GetRequiredService<IOptions<ResourceUsageOptions>>(), restarted, clock,
            services.GetRequiredService<ILogger<PodcastObsService>>());
        var replacement = await rearmed.ActivateAsync(plan.Id, "owner", Ct);
        Assert.NotEqual(original.ServerUrl, replacement.ServerUrl);
        await restarted.StopAsync(Ct);
        AssertPortFree(settings.FirstPort);
    }

    [Fact]
    public void Configuration_requires_SRT_and_a_bounded_UDP_range()
    {
        settings.Validate();
        settings.ServerUrl = "rtmp://obs.example.test";
        Assert.Throws<InvalidOperationException>(settings.Validate);
        settings.ServerUrl = "srt://obs.example.test?passphrase=unsafe";
        Assert.Throws<InvalidOperationException>(settings.Validate);
        settings.ServerUrl = "srt://obs.example.test";
        settings.FirstPort = 65535;
        settings.PortCount = 2;
        Assert.Throws<InvalidOperationException>(settings.Validate);
    }

    [Fact]
    public async Task Wrong_passphrase_never_starts_a_podcast_and_closed_accounts_cannot_arm()
    {
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;
        var plan = await PlanAsync(services);
        var obs = services.GetRequiredService<IPodcastObsService>();
        var credentials = await obs.ActivateAsync(plan.Id, "owner", Ct);
        var uri = new UriBuilder(credentials.ServerUrl) { Query = "mode=caller&passphrase=incorrect-test-passphrase&pbkeylen=32&enforced_encryption=1&connect_timeout=1000" };
        using (var publisher = Publisher(uri.Uri.AbsoluteUri))
        {
            var diagnostics = publisher.StandardError.ReadToEndAsync(Ct);
            await publisher.WaitForExitAsync(Ct).WaitAsync(TimeSpan.FromSeconds(10), Ct);
            Assert.NotEqual(0, publisher.ExitCode);
            await diagnostics;
        }
        Assert.Empty(await provider.GetRequiredService<ILiveProcessor>().ListAsync(Ct));
        var db = services.GetRequiredService<ApplicationDbContext>();
        var user = await db.Users.SingleAsync(Ct);
        user.WithdrawalRequestedAt = clock.Now;
        await db.SaveChangesAsync(Ct);
        await obs.DeactivateAsync(plan.Id, "owner", Ct);
        Assert.Equal(403, (await Assert.ThrowsAsync<PodcastException>(() => obs.ActivateAsync(plan.Id, "owner", Ct))).StatusCode);
        Assert.Empty(await db.ResourceStorageRecords.ToListAsync(Ct));
    }

    [Fact]
    public Task Real_encrypted_SRT_disconnect_produces_HLS_and_MP4_with_refund() => RealMediaAsync("disconnect");

    [Fact]
    public Task Real_encrypted_SRT_revocation_stops_ingestion_and_finalizes_replay() => RealMediaAsync("revoke");

    [Fact]
    public Task Real_encrypted_SRT_withdrawal_stops_ingestion() => RealMediaAsync("withdraw");

    [Fact]
    public Task Real_encrypted_SRT_consumption_limit_stops_ingestion_and_allows_refund() => RealMediaAsync("limit");

    [Fact]
    public Task Real_encrypted_SRT_disk_limit_stops_ingestion_and_allows_refund() => RealMediaAsync("disk");

    [Fact]
    public Task Real_encrypted_SRT_duration_limit_stops_ingestion_and_finalizes_replay() => RealMediaAsync("duration");

    private async Task RealMediaAsync(string end)
    {
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;
        var plan = await PlanAsync(services);
        var obs = services.GetRequiredService<IPodcastObsService>();
        var credentials = await obs.ActivateAsync(plan.Id, "owner", Ct);
        var processor = services.GetRequiredService<ILiveProcessor>();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(Ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(60));
        using var publisher = Publisher(credentials.ServerUrl);
        var diagnostics = publisher.StandardError.ReadToEndAsync(Ct);
        try
        {
            await WaitAsync(async () => (await processor.ListAsync(timeout.Token))
                .Any(p => p.Id == plan.Id && p.SegmentCount >= 2), timeout.Token);
            if (end == "revoke") await obs.DeactivateAsync(plan.Id, "owner", timeout.Token);
            else if (end == "withdraw")
            {
                using var dbScope = provider.CreateScope();
                var withdrawalDb = dbScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                (await withdrawalDb.Users.SingleAsync(timeout.Token)).WithdrawalRequestedAt = clock.Now;
                await withdrawalDb.SaveChangesAsync(timeout.Token);
            }
            else if (end == "limit")
            {
                using var dbScope = provider.CreateScope();
                var usageDb = dbScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await ResourceUsageTracker.RecordAsync(usageDb, "owner", "owner", cpuSeconds: 20000, cancellationToken: timeout.Token);
            }
            else if (end == "disk")
            {
                using var dbScope = provider.CreateScope();
                var diskDb = dbScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var user = await diskDb.Users.SingleAsync(timeout.Token);
                user.DiskQuota = user.DiskUsage;
                await diskDb.SaveChangesAsync(timeout.Token);
            }
            else if (end == "duration") clock.Now = clock.Now.AddMinutes(settings.MaxDurationMinutes);
            await WaitAsync(async () => !(await obs.StatusAsync(plan.Id, "owner", timeout.Token)).Enabled, timeout.Token);
            if (end == "withdraw")
            {
                await publisher.WaitForExitAsync(timeout.Token);
                return;
            }
            await WaitAsync(async () => (await processor.GetAsync(plan.Id, timeout.Token)).Podcast.State == PodcastState.Completed, timeout.Token);
            var manifest = await processor.GetAsync(plan.Id, Ct);
            Assert.True(manifest.Podcast.DurationSeconds >= 2);
            Assert.True(manifest.Podcast.Bytes > 0);
            using (var probe = StartProcess("ffprobe", "-v", "error", "-show_streams", "-of", "json",
                Path.Combine(root, plan.Id.ToString("N"), "00000000.ts")))
            {
                var output = await probe.StandardOutput.ReadToEndAsync(Ct);
                var probeError = await probe.StandardError.ReadToEndAsync(Ct);
                await probe.WaitForExitAsync(Ct);
                Assert.Equal(0, probe.ExitCode);
                Assert.Empty(probeError);
                using var document = JsonDocument.Parse(output);
                var audio = Assert.Single(document.RootElement.GetProperty("streams").EnumerateArray(),
                    s => s.GetProperty("codec_type").GetString() == "audio");
                Assert.Equal("aac", audio.GetProperty("codec_name").GetString());
                Assert.Equal("48000", audio.GetProperty("sample_rate").GetString());
            }
            var db = services.GetRequiredService<ApplicationDbContext>();
            var receipt = await db.ResourceStorageRecords.AsNoTracking().SingleAsync(Ct);
            Assert.Equal(manifest.Podcast.Bytes, receipt.Bytes);
            Assert.Equal(receipt.Bytes, (await db.Users.AsNoTracking().SingleAsync(Ct)).DiskUsage);
            var usage = await db.ResourceUsageRecords.AsNoTracking().SingleAsync(Ct);
            Assert.Equal((decimal)receipt.Bytes / (1024m * 1024m), usage.StorageMb);
            Assert.True(usage.BandwidthMb >= usage.StorageMb);
            var state = await obs.StatusAsync(plan.Id, "owner", Ct);
            if (end == "limit") Assert.Contains("seuil", state.Error);
            else if (end == "disk") Assert.Contains("Quota disque", state.Error);
            else Assert.Null(state.Error);
            Assert.True(await processor.PrepareNextReplayAsync(new FfmpegPodcastReplayRemuxer(
                services.GetRequiredService<IOptions<PodcastOptions>>()), Ct));
            Assert.Equal(PodcastReplayState.Ready, (await processor.GetAsync(plan.Id, Ct)).Podcast.Replay?.State);
            db.ChangeTracker.Clear();
            await services.GetRequiredService<IPodcastScheduleService>().DeleteAsync(plan.Id, "owner", Ct);
            Assert.Empty(db.ResourceStorageRecords);
            Assert.Equal(0, (await db.Users.AsNoTracking().SingleAsync(Ct)).DiskUsage);
            var refunded = await db.ResourceUsageRecords.AsNoTracking().SingleAsync(Ct);
            Assert.Equal(0m, refunded.StorageMb);
            Assert.Equal(usage.BandwidthMb, refunded.BandwidthMb);
        }
        finally
        {
            if (!publisher.HasExited) publisher.Kill(entireProcessTree: true);
            await publisher.WaitForExitAsync(CancellationToken.None);
            await diagnostics;
        }
    }

    private static Process Publisher(string url) => StartProcess("ffmpeg",
        "-nostdin", "-hide_banner", "-loglevel", "error", "-re",
        "-f", "lavfi", "-i", "testsrc=size=160x120:rate=10", "-re", "-f", "lavfi", "-i", "sine=frequency=440:sample_rate=48000",
        "-t", "12", "-c:v", "libx264", "-pix_fmt", "yuv420p", "-preset", "ultrafast", "-g", "20",
        "-c:a", "aac", "-f", "mpegts", url + "&timeout=3000000");

    private static int FreePort()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)socket.LocalEndPoint!).Port;
    }

    private static void AssertPortFree(int port)
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(new IPEndPoint(IPAddress.Loopback, port));
    }

    private static Process StartProcess(string executable, params string[] arguments)
    {
        var start = new ProcessStartInfo(executable)
        { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var argument in arguments) start.ArgumentList.Add(argument);
        return Process.Start(start) ?? throw new InvalidOperationException("Test process could not start.");
    }

    private static async Task WaitAsync(Func<Task<bool>> condition, CancellationToken ct)
    {
        while (!await condition()) await Task.Delay(100, ct);
    }

    public async ValueTask DisposeAsync()
    {
        await provider.GetRequiredService<PodcastObsWorker>().StopAsync(CancellationToken.None);
        await provider.DisposeAsync();
        await connection.DisposeAsync();
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }
}
