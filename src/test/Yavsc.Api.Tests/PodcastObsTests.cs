using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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

public sealed class PodcastObsTests : IDisposable
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private readonly string root = Path.Combine(Path.GetTempPath(), "yavsc-obs-" + Guid.NewGuid().ToString("N"));
    private readonly SqliteConnection connection;
    private readonly ServiceProvider provider;
    private readonly PodcastObsOptions settings = new()
    {
        Enabled = true, ServerUrl = "rtmps://obs.example.test:1936",
        RelayPassword = new string('x', 48), MaxDurationMinutes = 1
    };
    private readonly Clock clock = new();
    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    public PodcastObsTests()
    {
        var connectionString = $"Data Source=obs-{Guid.NewGuid():N};Mode=Memory;Cache=Shared;Default Timeout=30";
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
        services.AddHttpClient<IPodcastObsRelay, PodcastObsRelay>(c => c.Timeout = TimeSpan.FromSeconds(5));
        provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();
    }

    private async Task<PodcastResponse> PlanAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        db.Users.Add(new() { Id = "owner", UserName = "owner", Email = "owner@example.test" });
        await db.SaveChangesAsync(Ct);
        return await services.GetRequiredService<IPodcastScheduleService>().PrepareAsync("owner", new()
        {
            Title = "OBS programme", ScheduledStartAt = clock.Now.AddDays(7), PlannedDurationMinutes = 1
        }, null, Ct);
    }

    private static PodcastRelayAuthentication Publish(PodcastObsCredentials credentials)
    {
        var split = credentials.StreamKey.Split('?');
        return new() { Action = "publish", Protocol = "rtmp", Path = split[0], Query = split[1], Id = Guid.NewGuid().ToString() };
    }

    [Fact]
    public async Task Activation_is_owned_rotatable_expiring_and_never_persists_the_plain_key()
    {
        using var scope = provider.CreateScope();
        var plan = await PlanAsync(scope.ServiceProvider);
        var obs = scope.ServiceProvider.GetRequiredService<IPodcastObsService>();
        var denied = await Assert.ThrowsAsync<PodcastException>(() => obs.ActivateAsync(plan.Id, "other", Ct));
        Assert.Equal(403, denied.StatusCode);
        var credentials = await obs.ActivateAsync(plan.Id, "owner", Ct);
        Assert.Equal(settings.ServerUrl, credentials.ServerUrl);
        Assert.Equal(clock.Now.AddMinutes(30), credentials.ExpiresAt);
        var request = Publish(credentials);
        var flow = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().LiveFlow.SingleAsync(Ct);
        Assert.DoesNotContain(request.Query[4..], flow.ObsKeyHash!);
        Assert.NotNull(await obs.AuthenticateAsync(request, Ct));
        Assert.Null(await obs.AuthenticateAsync(request with { Id = Guid.NewGuid().ToString() }, Ct));
        Assert.Null(await obs.AuthenticateAsync(request with { Path = "obs/" + Guid.NewGuid().ToString("N") }, Ct));
        Assert.Null(await obs.AuthenticateAsync(request with { Query = request.Query + "&" + request.Query }, Ct));
        var renewed = await obs.ActivateAsync(plan.Id, "owner", Ct);
        Assert.Null(await obs.AuthenticateAsync(request, Ct));
        Assert.NotNull(await obs.AuthenticateAsync(Publish(renewed), Ct));
        clock.Now = renewed.ExpiresAt;
        Assert.Null(await obs.AuthenticateAsync(Publish(renewed), Ct));
        Assert.False((await obs.StatusAsync(plan.Id, "owner", Ct)).Enabled);
    }

    [Fact]
    public async Task Publisher_cannot_read_media_or_control_relay_and_closed_accounts_cannot_publish()
    {
        using var scope = provider.CreateScope();
        var plan = await PlanAsync(scope.ServiceProvider);
        var obs = scope.ServiceProvider.GetRequiredService<IPodcastObsService>();
        var publish = Publish(await obs.ActivateAsync(plan.Id, "owner", Ct));
        Assert.Null(await obs.AuthenticateAsync(publish with { Action = "read", Protocol = "rtsp" }, Ct));
        Assert.Null(await obs.AuthenticateAsync(publish with { Action = "api" }, Ct));
        Assert.NotNull(await obs.AuthenticateAsync(new()
        { Action = "api", User = settings.RelayUser, Password = settings.RelayPassword }, Ct));
        Assert.NotNull(await obs.AuthenticateAsync(new()
        { Action = "read", Protocol = "rtmp", Path = publish.Path, User = settings.RelayUser, Password = settings.RelayPassword }, Ct));
        Assert.Null(await obs.AuthenticateAsync(new()
        { Action = "read", Protocol = "hls", Path = publish.Path, User = settings.RelayUser, Password = settings.RelayPassword }, Ct));
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.Users.SingleAsync(Ct)).WithdrawalRequestedAt = clock.Now;
        await db.SaveChangesAsync(Ct);
        Assert.Null(await obs.AuthenticateAsync(publish, Ct));
    }

    [Fact]
    public async Task Activation_blocks_exhausted_quotas_and_direct_capture_cannot_steal_an_armed_emission()
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
        await ResourceUsageTracker.RecordAsync(db, "owner", "owner", cpuSeconds: 20000, cancellationToken: Ct);
        Assert.Equal(429, (await Assert.ThrowsAsync<PodcastException>(() => obs.ActivateAsync(plan.Id, "owner", Ct))).StatusCode);
    }

    [Fact]
    public void Public_transport_must_be_encrypted_unless_explicitly_opted_out()
    {
        settings.ServerUrl = "rtmp://obs.example.test:1935";
        Assert.Throws<InvalidOperationException>(settings.Validate);
        settings.AllowUnencryptedRtmp = true;
        settings.Validate();
        settings.ServerUrl = "rtmp://obs.example.test/live";
        Assert.Throws<InvalidOperationException>(settings.Validate);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Real_rtmp_publisher_becomes_authenticated_HLS_and_MP4_with_storage_refund(bool revoke, bool secure)
    {
        var binary = Environment.GetEnvironmentVariable("YAVSC_TEST_MEDIAMTX");
        if (string.IsNullOrWhiteSpace(binary)) Assert.Skip("Set YAVSC_TEST_MEDIAMTX to the MediaMTX v1.21.3 binary to run the real OBS-compatible media test.");
        var apiPort = FreePort();
        var rtmpPort = FreePort();
        var rtmpsPort = FreePort();
        var authPort = FreePort();
        settings.ServerUrl = secure ? $"rtmps://127.0.0.1:{rtmpsPort}" : $"rtmp://127.0.0.1:{rtmpPort}";
        settings.AllowUnencryptedRtmp = !secure;
        settings.RelayApiUrl = $"http://127.0.0.1:{apiPort}/";
        settings.RelayReadUrl = $"rtmp://127.0.0.1:{rtmpPort}/";
        Directory.CreateDirectory(root);
        var tlsConfig = "";
        if (secure)
        {
            using var key = RSA.Create(2048);
            var request = new CertificateRequest("CN=localhost", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddHours(1));
            var keyPath = Path.Combine(root, "server.key");
            var certPath = Path.Combine(root, "server.crt");
            await File.WriteAllTextAsync(keyPath, key.ExportPkcs8PrivateKeyPem(), Ct);
            await File.WriteAllTextAsync(certPath, certificate.ExportCertificatePem(), Ct);
            tlsConfig = $"rtmpEncryption: optional\nrtmpsAddress: 127.0.0.1:{rtmpsPort}\n"
                + $"rtmpServerKey: {JsonSerializer.Serialize(keyPath)}\nrtmpServerCert: {JsonSerializer.Serialize(certPath)}";
        }
        var config = Path.Combine(root, "mediamtx.yml");
        await File.WriteAllTextAsync(config, $$"""
            logLevel: warn
            authMethod: http
            authHTTPAddress: http://127.0.0.1:{{authPort}}/auth
            authHTTPExclude: []
            api: true
            apiAddress: 127.0.0.1:{{apiPort}}
            rtsp: false
            rtmpAddress: 127.0.0.1:{{rtmpPort}}
            {{tlsConfig}}
            hls: false
            webrtc: false
            srt: false
            playback: false
            pathDefaults:
              overridePublisher: false
              maxReaders: 1
            paths:
              "~^obs/[0-9a-f]{32}$":
            """, Ct);
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls($"http://127.0.0.1:{authPort}");
        await using var app = builder.Build();
        app.MapPost("/auth", async (PodcastRelayAuthentication request) =>
        {
            using var scope = provider.CreateScope();
            var result = await scope.ServiceProvider.GetRequiredService<IPodcastObsService>().AuthenticateAsync(request, Ct);
            return result is null ? Results.Unauthorized() : Results.Ok(result);
        });
        await app.StartAsync(Ct);
        using var relayProcess = StartProcess(binary!, config);
        var relayOutput = relayProcess.StandardOutput.ReadToEndAsync();
        var relayErrors = relayProcess.StandardError.ReadToEndAsync();
        var worker = new PodcastObsWorker(provider.GetRequiredService<IServiceScopeFactory>(), Options.Create(settings),
            provider.GetRequiredService<ILogger<PodcastObsWorker>>());
        Process? publisher = null;
        Task<string>? publisherErrors = null;
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(Ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(75));
            using var scope = provider.CreateScope();
            var services = scope.ServiceProvider;
            var plan = await PlanAsync(services);
            var credentials = await services.GetRequiredService<IPodcastObsService>().ActivateAsync(plan.Id, "owner", Ct);
            using var http = new HttpClient();
            // Probe the listener independently of media authorization before starting the publisher.
            await WaitAsync(async () =>
            {
                try { using var response = await http.GetAsync(settings.RelayApiUrl, timeout.Token); return true; }
                catch (HttpRequestException) { return false; }
            }, timeout.Token);
            await worker.StartAsync(Ct);
            publisher = StartProcess("ffmpeg", "-nostdin", "-hide_banner", "-loglevel", "error", "-re",
                "-f", "lavfi", "-i", "testsrc=size=160x120:rate=10", "-re", "-f", "lavfi", "-i", "sine=frequency=440:sample_rate=48000",
                "-t", "20", "-c:v", "libx264", "-pix_fmt", "yuv420p", "-preset", "ultrafast", "-g", "20",
                "-c:a", "aac", "-b:v", "180k", "-b:a", "32k", "-f", "flv", $"{credentials.ServerUrl}/{credentials.StreamKey}");
            publisherErrors = publisher.StandardError.ReadToEndAsync();
            var processor = services.GetRequiredService<ILiveProcessor>();
            if (revoke)
            {
                await WaitAsync(async () => (await processor.ListAsync(timeout.Token))
                    .Any(p => p.Id == plan.Id && p.SegmentCount >= 2), timeout.Token);
                await services.GetRequiredService<IPodcastObsService>().DeactivateAsync(plan.Id, "owner", timeout.Token);
            }
            await WaitAsync(async () =>
            {
                if (!revoke && publisher.HasExited && publisher.ExitCode != 0)
                    throw new InvalidOperationException("Synthetic publisher failed: " +
                        (await publisherErrors).Replace(credentials.StreamKey, "[redacted]", StringComparison.Ordinal));
                var status = await services.GetRequiredService<IPodcastObsService>().StatusAsync(plan.Id, "owner", timeout.Token);
                if (status.Error is not null) throw new InvalidOperationException(status.Error);
                return (await processor.ListAsync(timeout.Token)).Any(p =>
                    p.Id == plan.Id && p.State == PodcastState.Completed && p.SegmentCount > 0);
            }, timeout.Token);
            var manifest = await processor.GetAsync(plan.Id, Ct);
            Assert.True(manifest.Podcast.DurationSeconds >= (revoke ? 2 : 8));
            Assert.True(manifest.Podcast.Bytes > 0);
            using (var probe = StartProcess("ffprobe", "-v", "error", "-show_streams", "-of", "json",
                Path.Combine(root, plan.Id.ToString("N"), "00000000.ts")))
            {
                var probeOutput = await probe.StandardOutput.ReadToEndAsync(Ct);
                var probeError = await probe.StandardError.ReadToEndAsync(Ct);
                await probe.WaitForExitAsync(Ct);
                Assert.Equal(0, probe.ExitCode);
                using var probeDocument = JsonDocument.Parse(probeOutput);
                var audio = Assert.Single(probeDocument.RootElement.GetProperty("streams").EnumerateArray(),
                    s => s.GetProperty("codec_type").GetString() == "audio");
                Assert.Equal("aac", audio.GetProperty("codec_name").GetString());
                Assert.True(string.IsNullOrEmpty(probeError), probeError);
                Assert.Equal("48000", audio.GetProperty("sample_rate").GetString());
            }
            var db = services.GetRequiredService<ApplicationDbContext>();
            var receipt = await db.ResourceStorageRecords.AsNoTracking().SingleAsync(Ct);
            Assert.Equal(manifest.Podcast.Bytes, receipt.Bytes);
            var user = await db.Users.AsNoTracking().SingleAsync(Ct);
            Assert.Equal(receipt.Bytes, user.DiskUsage);
            var usage = await db.ResourceUsageRecords.AsNoTracking().SingleAsync(Ct);
            Assert.Equal((decimal)receipt.Bytes / (1024m * 1024m), usage.StorageMb);
            Assert.True(usage.BandwidthMb >= usage.StorageMb);
            if (!revoke) Assert.Equal(usage.StorageMb, usage.BandwidthMb);
            Assert.False((await services.GetRequiredService<IPodcastObsService>().StatusAsync(plan.Id, "owner", Ct)).Enabled);
            Assert.Null((await services.GetRequiredService<IPodcastObsService>().StatusAsync(plan.Id, "owner", Ct)).Error);
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
            await worker.StopAsync(CancellationToken.None);
            worker.Dispose();
            if (publisher is not null)
            {
                if (!publisher.HasExited) publisher.Kill(entireProcessTree: true);
                await publisher.WaitForExitAsync(CancellationToken.None);
                await publisherErrors!;
                publisher.Dispose();
            }
            if (!relayProcess.HasExited) relayProcess.Kill(entireProcessTree: true);
            await relayProcess.WaitForExitAsync(CancellationToken.None);
            await relayOutput;
            await relayErrors;
            await app.StopAsync(CancellationToken.None);
        }
    }

    private static int FreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private static Process StartProcess(string file, params string[] args)
    {
        var start = new ProcessStartInfo(file) { UseShellExecute = false, RedirectStandardError = true, RedirectStandardOutput = true };
        foreach (var arg in args) start.ArgumentList.Add(arg);
        return Process.Start(start) ?? throw new IOException("Could not start test media process.");
    }

    private static async Task WaitAsync(Func<Task<bool>> condition, CancellationToken ct)
    {
        while (!await condition()) await Task.Delay(200, ct);
    }

    public void Dispose()
    {
        provider.Dispose();
        connection.Dispose();
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }
}
