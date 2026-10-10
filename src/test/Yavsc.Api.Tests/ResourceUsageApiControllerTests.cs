using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Abstract.Models.Messaging;
using Yavsc.Abstract.Resources;
using Yavsc.Api.Test.Fixtures;
using Yavsc.Server.Helpers;
using Yavsc.Tests.Shared;

namespace Yavsc.Api.Test;

[Collection("Yavsc Api")]
public sealed class ResourceUsageApiControllerTests : IClassFixture<ApiWebServerFixture>
{
    private readonly ApiWebServerFixture _fixture;

    public ResourceUsageApiControllerTests(ApiWebServerFixture fixture)
    {
        _fixture = fixture;
    }

    private static HttpClient NewClient(ApiWebServerFixture fixture, string subject = "alice", string scope = "api", IEnumerable<Claim>? extraClaims = null)
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        var http = new HttpClient(handler)
        {
            BaseAddress = new Uri(fixture.BaseAddress)
        };

        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestTokenIssuer.Issue(subject, scope, extraClaims: extraClaims));

        return http;
    }

    [Fact]
    public async Task GetCurrentUserUsage_persists_snapshot_in_database()
    {
        using var http = NewClient(_fixture, "alice");

        var response = await http.GetAsync("/api/v1/resource-usage/me", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var summary = await response.Content.ReadFromJsonAsync<ResourceUsageSummary>(TestContext.Current.CancellationToken);
        Assert.NotNull(summary);
        Assert.Equal("alice", summary!.UserId);

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Yavsc.Models.ApplicationDbContext>();
        Assert.Contains(db.ResourceUsageRecords, r => r.UserId == "alice");
    }

    [Fact]
    public async Task RecordAsync_persists_cpu_and_storage_usage()
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Yavsc.Models.ApplicationDbContext>();

        await ResourceUsageTracker.RecordAsync(
            db,
            "alice",
            "alice",
            cpuSeconds: 1.5m,
            storageBytes: 2 * 1024 * 1024,
            actorUserId: "alice");

        var record = await db.ResourceUsageRecords
            .SingleAsync(r => r.UserId == "alice");

        Assert.True(record.CpuSeconds >= 1.5m);
        Assert.True(record.StorageMb >= 2m / 1024m / 1024m);
    }

    [Fact]
    public async Task RecordAsync_persists_api_and_bandwidth_usage()
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Yavsc.Models.ApplicationDbContext>();

        await ResourceUsageTracker.RecordAsync(
            db,
            "telemetry-user",
            "telemetry-user",
            apiCalls: 2m,
            bandwidthMb: 3.5m,
            actorUserId: "telemetry-user");

        var record = await db.ResourceUsageRecords
            .SingleAsync(r => r.UserId == "telemetry-user");

        Assert.True(record.ApiCalls >= 2m);
        Assert.True(record.BandwidthMb >= 3.5m);
    }

    [Fact]
    public async Task GetCurrentUserUsage_records_telemetry_values_from_authenticated_requests()
    {
        using var http = NewClient(_fixture, "telemetry-user");

        var response = await http.GetAsync("/api/v1/resource-usage/me", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Yavsc.Models.ApplicationDbContext>();
        var record = await db.ResourceUsageRecords
            .SingleAsync(r => r.UserId == "telemetry-user");

        Assert.True(record.ApiCalls >= 1m);
        Assert.True(record.BandwidthMb > 0m);
    }

    [Fact]
    public async Task GetCurrentUserNotifications_returns_public_and_targeted_notifications()
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Yavsc.Models.ApplicationDbContext>();

        db.Notification.RemoveRange(db.Notification);
        await db.Notification.AddRangeAsync(
            new Notification
            {
                title = "Public",
                body = "Seen by everyone",
                click_action = "/public",
                Target = null,
            },
            new Notification
            {
                title = "User",
                body = "Private to current user",
                click_action = "/profile",
                Target = "user/telemetry-user",
            },
            new Notification
            {
                title = "Admin",
                body = "Restricted to administrators",
                click_action = "/admin",
                Target = "administration",
            });
        await db.SaveChangesAsync();

        using var http = NewClient(_fixture, "telemetry-user");

        var response = await http.GetAsync("/api/v1/notifications/me", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var notifications = await response.Content.ReadFromJsonAsync<List<Notification>>(TestContext.Current.CancellationToken);
        Assert.NotNull(notifications);
        Assert.Contains(notifications!, n => n.title == "Public");
        Assert.Contains(notifications!, n => n.title == "User");
        Assert.DoesNotContain(notifications!, n => n.title == "Admin");
    }

    [Fact]
    public async Task GetAdminOverview_returns_latest_usage_snapshot_for_each_user()
    {
        using var http = NewClient(_fixture, "alice");
        var first = await http.GetAsync("/api/v1/resource-usage/me", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        using var adminHttp = NewClient(
            _fixture,
            "admin-user",
            "api",
            new[]
            {
                new Claim(Yavsc.Constants.RoleClaimType, Yavsc.Constants.AdminGroupName)
            });

        var response = await adminHttp.GetAsync("/api/v1/resource-usage/admin", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<List<ResourceUsageSummary>>(TestContext.Current.CancellationToken);
        Assert.NotNull(payload);
        Assert.Contains(payload!, item => item.UserId == "alice");
    }
}
