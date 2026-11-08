using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Yavsc.Models;
using Yavsc.Server.Helpers;
using Yavsc.Settings;

namespace Yavsc.Server.Tests;

public sealed class ResourceUsageTests
{
    private static ServiceProvider NewProvider(Action<ResourceUsageOptions>? configure = null)
    {
        var services = new ServiceCollection();
        var databaseName = Guid.NewGuid().ToString();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(databaseName));
        services.AddResourceUsage(new ConfigurationBuilder().Build());
        services.Configure<ResourceUsageOptions>(o =>
        {
            o.ApiCallWeight = 0.02m;
            o.CpuSecondWeight = 1m;
            o.BandwidthMbWeight = 0.1m;
            o.StorageMbWeight = 0.3m;
            o.DefaultLimit = 10000m;
        });
        services.Configure<SiteSettings>(o => o.Authority = "https://instance.example");
        if (configure is not null) services.Configure(configure);
        return services.BuildServiceProvider();
    }

    private static DefaultHttpContext NewContext(IServiceProvider services)
        => new()
        {
            RequestServices = services,
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "alice")], "Bearer")),
            Response = { Body = new MemoryStream() }
        };

    [Fact]
    public void Configured_formula_uses_exact_decimal_points()
    {
        var rule = new ResourceUsageOptions
        {
            ApiCallWeight = 0.2m, CpuSecondWeight = 0.1m,
            BandwidthMbWeight = 0.05m, StorageMbWeight = 0.5m, DefaultLimit = 1000m
        };
        Assert.Equal(29.34m, rule.Calculate(12.5m, 3.2m, 18.4m, 51.2m));
        Assert.Equal(0m, rule.Calculate(0m, 0m, 0m, 0m));
        Assert.Equal(0.002m, rule.Calculate(0.01m, 0m, 0m, 0m));
        Assert.Equal(1000m, rule.GetLimit("unknown-user"));
    }

    [Fact]
    public async Task Summary_uses_latest_snapshot_and_individual_limit_without_summing_snapshots()
    {
        using var provider = NewProvider(o => o.UserLimits["alice"] = 100m);
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.ResourceUsageRecords.AddRange(
            new ResourceUsageRecord { UserId = "alice", UserName = "old", CpuSeconds = 500m, RecordedAtUtc = DateTime.UtcNow.AddDays(-1) },
            new ResourceUsageRecord { UserId = "alice", UserName = "alice", ApiCalls = 10m, CpuSeconds = 3m, BandwidthMb = 2m, StorageMb = 4m });
        await db.SaveChangesAsync();
        var usage = scope.ServiceProvider.GetRequiredService<IResourceUsageService>();
        var summary = await usage.GetCurrentAsync("alice", "alice", TestContext.Current.CancellationToken);
        Assert.Equal(4.6m, summary.Consumption);
        Assert.Equal(100m, summary.ConsumptionLimit);
        Assert.Equal(95.4m, summary.RemainingConsumption);
        Assert.False(summary.IsQuotaReached);
        var overview = await usage.GetOverviewAsync(TestContext.Current.CancellationToken);
        Assert.Single(overview);
        Assert.Equal(summary.Consumption, overview[0].Consumption);
    }

    [Fact]
    public async Task Concurrent_admissions_allow_exact_limit_without_lost_updates_or_duplicate_snapshots()
    {
        using var provider = NewProvider(o => o.DefaultLimit = 0.04m);
        var results = await Task.WhenAll(Enumerable.Range(0, 20).Select(async _ =>
        {
            using var scope = provider.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<IResourceUsageService>()
                .TryAdmitAsync("alice", "alice", 1m, 0m, 0, TestContext.Current.CancellationToken);
        }));
        Assert.Equal(2, results.Count(accepted => accepted));
        using var finalScope = provider.CreateScope();
        var db = finalScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var record = Assert.Single(await db.ResourceUsageRecords.ToListAsync(TestContext.Current.CancellationToken));
        Assert.Equal(2m, record.ApiCalls);
        var summary = await finalScope.ServiceProvider.GetRequiredService<IResourceUsageService>()
            .GetCurrentAsync("alice", "alice", TestContext.Current.CancellationToken);
        Assert.True(summary.IsQuotaReached);
        Assert.Equal(0m, summary.RemainingConsumption);
    }

    [Fact]
    public async Task Concurrent_accounting_preserves_all_resource_deltas()
    {
        using var provider = NewProvider();
        await Task.WhenAll(Enumerable.Range(0, 20).Select(async _ =>
        {
            using var scope = provider.CreateScope();
            await ResourceUsageTracker.RecordAsync(
                scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(),
                "alice", "alice", cpuSeconds: 0.1m, apiCalls: 1m,
                storageBytes: 1048576, bandwidthMb: 0.5m);
        }));
        using var finalScope = provider.CreateScope();
        var record = Assert.Single(await finalScope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
            .ResourceUsageRecords.ToListAsync(TestContext.Current.CancellationToken));
        Assert.Equal(20m, record.ApiCalls);
        Assert.Equal(2m, record.CpuSeconds);
        Assert.Equal(20m, record.StorageMb);
        Assert.Equal(10m, record.BandwidthMb);
    }

    [Fact]
    public async Task Known_ingress_or_storage_cost_is_refused_before_consumption()
    {
        using var provider = NewProvider(o => o.DefaultLimit = 0.11m);
        using var scope = provider.CreateScope();
        var usage = scope.ServiceProvider.GetRequiredService<IResourceUsageService>();
        Assert.False(await usage.TryAdmitAsync("alice", "alice", 1m, 1m, 0, TestContext.Current.CancellationToken));
        Assert.False(await usage.TryAdmitAsync("alice", "alice", 0m, 0m, 1048576, TestContext.Current.CancellationToken));
        Assert.Empty(await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
            .ResourceUsageRecords.ToListAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Observed_cost_can_exceed_limit_then_new_operations_are_refused_until_limit_is_increased()
    {
        using var provider = NewProvider(o => o.DefaultLimit = 1m);
        using var scope = provider.CreateScope();
        var usage = scope.ServiceProvider.GetRequiredService<IResourceUsageService>();
        Assert.True(await usage.TryAdmitAsync("alice", "alice", 1m, 0m, 0, TestContext.Current.CancellationToken));
        await ResourceUsageTracker.RecordAsync(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(),
            "alice", "alice", cpuSeconds: 2m, cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(await usage.TryAdmitAsync("alice", "alice", 1m, 0m, 0, TestContext.Current.CancellationToken));
        var summary = await usage.GetCurrentAsync("alice", "alice", TestContext.Current.CancellationToken);
        Assert.Equal(2.02m, summary.Consumption);
        Assert.Equal(0m, summary.RemainingConsumption);
        Assert.True(summary.IsQuotaReached);

        var raised = new ResourceUsageService(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(),
            Options.Create(new ResourceUsageOptions
            {
                DefaultLimit = 3m, ApiCallWeight = 0.02m, CpuSecondWeight = 1m,
                BandwidthMbWeight = 0.1m, StorageMbWeight = 0.3m
            }));
        Assert.True(await raised.TryAdmitAsync("alice", "alice", 1m, 0m, 0, TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Middleware_counts_streamed_bodies_once_with_or_without_declared_ingress(bool declared)
    {
        using var provider = NewProvider();
        using var scope = provider.CreateScope();
        var context = NewContext(scope.ServiceProvider);
        using var originalRequest = new MemoryStream(new byte[1048576]);
        using var originalResponse = new MemoryStream();
        context.Request.Body = originalRequest;
        context.Response.Body = originalResponse;
        if (declared) context.Request.ContentLength = 1048576;
        var middleware = new ResourceUsageTelemetryMiddleware(
            async ctx =>
            {
                await ctx.Request.Body.CopyToAsync(Stream.Null);
                await ctx.Response.BodyWriter.WriteAsync(new byte[2 * 1048576]);
            },
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<ResourceUsageTelemetryMiddleware>.Instance);
        await middleware.InvokeAsync(context, scope.ServiceProvider.GetRequiredService<IResourceUsageService>());
        Assert.Same(originalRequest, context.Request.Body);
        Assert.Same(originalResponse, context.Response.Body);
        Assert.True(originalRequest.CanRead);
        Assert.True(originalResponse.CanWrite);
        var summary = await scope.ServiceProvider.GetRequiredService<IResourceUsageService>()
            .GetCurrentAsync("alice", "alice", TestContext.Current.CancellationToken);
        Assert.Equal(1m, summary.ApiCalls);
        Assert.Equal(3m, summary.BandwidthMb);
        Assert.Equal(0.32m, summary.Consumption);
    }

    [Fact]
    public async Task Failed_execution_is_accounted_and_original_streams_are_restored()
    {
        using var provider = NewProvider();
        using var scope = provider.CreateScope();
        var context = NewContext(scope.ServiceProvider);
        var originalResponse = context.Response.Body;
        var middleware = new ResourceUsageTelemetryMiddleware(
            async ctx =>
            {
                await ctx.Response.Body.WriteAsync(new byte[1048576]);
                throw new InvalidOperationException("Expected test failure");
            }, provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<ResourceUsageTelemetryMiddleware>.Instance);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            middleware.InvokeAsync(context, scope.ServiceProvider.GetRequiredService<IResourceUsageService>()));
        Assert.Same(originalResponse, context.Response.Body);
        var summary = await scope.ServiceProvider.GetRequiredService<IResourceUsageService>()
            .GetCurrentAsync("alice", "alice", TestContext.Current.CancellationToken);
        Assert.Equal(0.12m, summary.Consumption);
    }

    [Theory]
    [InlineData("/login", false)]
    [InlineData("/refresh", false)]
    [InlineData("/logout", false)]
    [InlineData("/api/v1/resource-usage/me", true)]
    public async Task Exempt_endpoints_remain_available_without_consumption(string path, bool metadata)
    {
        using var provider = NewProvider(o => o.DefaultLimit = 0.01m);
        using var scope = provider.CreateScope();
        var context = NewContext(scope.ServiceProvider);
        context.Request.Path = path;
        if (metadata)
            context.SetEndpoint(new Endpoint(_ => Task.CompletedTask,
                new EndpointMetadataCollection(new ResourceUsageExemptAttribute()), "usage"));
        var ran = false;
        var middleware = new ResourceUsageTelemetryMiddleware(
            _ => { ran = true; return Task.CompletedTask; },
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<ResourceUsageTelemetryMiddleware>.Instance);
        await middleware.InvokeAsync(context, scope.ServiceProvider.GetRequiredService<IResourceUsageService>());
        Assert.True(ran);
        Assert.Empty(await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
            .ResourceUsageRecords.ToListAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Rejected_request_returns_problem_details_without_running_or_charging()
    {
        using var provider = NewProvider(o => o.DefaultLimit = 0.01m);
        using var scope = provider.CreateScope();
        var context = NewContext(scope.ServiceProvider);
        var middleware = new ResourceUsageTelemetryMiddleware(
            _ => throw new InvalidOperationException("Must not run"),
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<ResourceUsageTelemetryMiddleware>.Instance);
        await middleware.InvokeAsync(context, scope.ServiceProvider.GetRequiredService<IResourceUsageService>());
        Assert.Equal(429, context.Response.StatusCode);
        Assert.StartsWith("application/problem+json", context.Response.ContentType);
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var payload = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
        Assert.Contains("resource_quota_exceeded", payload);
        Assert.Contains("https://instance.example/Home/CGU", payload);
        Assert.Empty(await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
            .ResourceUsageRecords.ToListAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Tracked_storage_replacement_and_deletion_refund_only_their_original_volume()
    {
        using var provider = NewProvider();
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var ct = TestContext.Current.CancellationToken;
        await ResourceUsageTracker.RecordAsync(db, "alice", "alice", cpuSeconds: 5m, storageBytes: 1048576, cancellationToken: ct);
        await ResourceUsageTracker.RecordStorageAsync(db, "alice", "alice", "personal/file", 2 * 1048576, ct);
        await ResourceUsageTracker.RecordStorageAsync(db, "alice", "alice", "personal/file", 1048576, ct);
        var record = await db.ResourceUsageRecords.SingleAsync(ct);
        Assert.Equal(2m, record.StorageMb);
        var usage = scope.ServiceProvider.GetRequiredService<IResourceUsageService>();
        Assert.Equal(5.6m, (await usage.GetCurrentAsync("alice", "alice", ct)).Consumption);
        await ResourceUsageTracker.RecordStorageAsync(db, "alice", "alice", "personal/legacy", 0, ct);
        Assert.Equal(2m, record.StorageMb);
        await ResourceUsageTracker.RecordStorageAsync(db, "alice", "alice", "personal/file", 0, ct);
        await ResourceUsageTracker.RecordStorageAsync(db, "alice", "alice", "personal/file", 0, ct);
        Assert.Equal(1m, record.StorageMb);
        Assert.Equal(5m, record.CpuSeconds);
        Assert.Equal(5.3m, (await usage.GetCurrentAsync("alice", "alice", ct)).Consumption);
        Assert.Empty(await db.ResourceStorageRecords.ToListAsync(ct));
    }

    [Fact]
    public async Task Concurrent_deletions_do_not_refund_the_same_receipt_twice()
    {
        using var provider = NewProvider();
        var ct = TestContext.Current.CancellationToken;
        using (var setup = provider.CreateScope())
        {
            var db = setup.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await ResourceUsageTracker.RecordStorageAsync(db, "alice", "alice", "file", 1048576, ct);
            await ResourceUsageTracker.RecordAsync(db, "alice", "alice", storageBytes: 1048576, cancellationToken: ct);
        }
        await Task.WhenAll(Enumerable.Range(0, 5).Select(async _ =>
        {
            using var scope = provider.CreateScope();
            await ResourceUsageTracker.RecordStorageAsync(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(),
                "alice", "alice", "file", 0, ct);
        }));
        using var verify = provider.CreateScope();
        Assert.Equal(1m, (await verify.ServiceProvider.GetRequiredService<ApplicationDbContext>()
            .ResourceUsageRecords.SingleAsync(ct)).StorageMb);
    }

    [Fact]
    public async Task Repeated_pdf_storage_accounting_is_idempotent_and_renamed_files_remain_refundable()
    {
        using var provider = NewProvider();
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var ct = TestContext.Current.CancellationToken;
        await ResourceUsageTracker.RecordStorageAsync(db, "alice", "alice", "pdf/file", 1048576, ct);
        await ResourceUsageTracker.RecordStorageAsync(db, "alice", "alice", "pdf/file", 1048576, ct);
        await ResourceUsageTracker.MoveStorageAsync(db, "alice", "pdf", "renamed", ct);
        Assert.Equal("renamed/file", (await db.ResourceStorageRecords.SingleAsync(ct)).Path);
        await ResourceUsageTracker.RecordStorageAsync(db, "alice", "alice", "renamed/file", 0, ct);
        Assert.Equal(0m, (await db.ResourceUsageRecords.SingleAsync(ct)).StorageMb);
    }

    [Fact]
    public async Task Relief_request_runs_at_the_limit_but_still_counts_api_and_transfer_costs()
    {
        using var provider = NewProvider(o => o.DefaultLimit = 0.01m);
        using var scope = provider.CreateScope();
        var context = NewContext(scope.ServiceProvider);
        context.SetEndpoint(new Endpoint(_ => Task.CompletedTask,
            new EndpointMetadataCollection(new ResourceUsageReliefAttribute()), "delete"));
        var ran = false;
        var middleware = new ResourceUsageTelemetryMiddleware(
            async ctx =>
            {
                ran = true;
                await ctx.Response.Body.WriteAsync(new byte[1048576], TestContext.Current.CancellationToken);
            },
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<ResourceUsageTelemetryMiddleware>.Instance);
        await middleware.InvokeAsync(context, scope.ServiceProvider.GetRequiredService<IResourceUsageService>());
        Assert.True(ran);
        var record = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
            .ResourceUsageRecords.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(1m, record.ApiCalls);
        Assert.Equal(1m, record.BandwidthMb);
    }

    [Fact]
    public async Task Negative_counter_deltas_are_explicitly_rejected()
    {
        using var provider = NewProvider();
        using var scope = provider.CreateScope();
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            ResourceUsageTracker.RecordAsync(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(),
                "alice", "alice", cpuSeconds: -1m, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("ResourceUsage:ApiCallWeight", "0")]
    [InlineData("ResourceUsage:BandwidthMbWeight", "-1")]
    [InlineData("ResourceUsage:DefaultLimit", "0")]
    [InlineData("ResourceUsage:UserLimits:alice", "-1")]
    public void Invalid_configuration_is_rejected(string key, string value)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { [key] = value }).Build();
        var services = new ServiceCollection().AddResourceUsage(configuration);
        using var provider = services.BuildServiceProvider();
        Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<ResourceUsageOptions>>().Value);
    }
}
