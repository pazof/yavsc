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

[Trait("Category", "non-reg")]
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
    public async Task Resource_alert_is_rolled_back_with_its_usage_update()
    {
        var ct = TestContext.Current.CancellationToken;
        var userId = "resource-alert-rollback-" + Guid.NewGuid().ToString("N");
        using (var scope = _fixture.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Yavsc.Models.ApplicationDbContext>();
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            await ResourceUsageTracker.RecordAsync(db, userId, userId, cpuSeconds: 5000m, cancellationToken: ct);
            Assert.Single(db.Notification, note => note.Target == $"user/{userId}");
            await transaction.RollbackAsync(ct);
        }
        using var verification = _fixture.Services.CreateScope();
        var persisted = verification.ServiceProvider.GetRequiredService<Yavsc.Models.ApplicationDbContext>();
        Assert.False(await persisted.ResourceUsageRecords.AnyAsync(record => record.UserId == userId, ct));
        Assert.False(await persisted.Notification.AnyAsync(note => note.Target == $"user/{userId}", ct));
    }

    [Fact]
    public async Task Resource_alerts_are_private_persistent_dismissible_at_quota_and_reappear_on_a_new_crossing()
    {
        var ct = TestContext.Current.CancellationToken;
        var userId = "resource-alert-" + Guid.NewGuid().ToString("N");
        using var seed = _fixture.Services.CreateScope();
        var db = seed.ServiceProvider.GetRequiredService<Yavsc.Models.ApplicationDbContext>();
        db.Users.Add(new Yavsc.Models.ApplicationUser
        {
            Id = userId, UserName = userId, Email = userId + "@example.test"
        });
        await db.SaveChangesAsync(ct);
        await ResourceUsageTracker.RecordAsync(db, userId, userId, cpuSeconds: 5000m, cancellationToken: ct);
        var first = await db.Notification.SingleAsync(n => n.Target == $"user/{userId}", ct);
        Assert.Equal("resource-usage/50", first.tag);
        await ResourceUsageTracker.RecordAsync(db, userId, userId, cpuSeconds: 5000m, cancellationToken: ct);

        using var owner = NewClient(_fixture, userId);
        using var other = NewClient(_fixture, "other-resource-user");
        using var anonymous = NewClient(_fixture);
        anonymous.DefaultRequestHeaders.Authorization = null;
        var ownerNotes = await owner.GetFromJsonAsync<List<Notification>>("/api/v1/notifications/me", ct);
        Assert.NotNull(ownerNotes);
        Assert.Equal(2, ownerNotes.Count(n => n.Target == $"user/{userId}"));
        Assert.DoesNotContain((await other.GetFromJsonAsync<List<Notification>>("/api/v1/notifications/me", ct))!,
            n => n.Target == $"user/{userId}");
        Assert.DoesNotContain((await anonymous.GetFromJsonAsync<List<Notification>>("/api/v1/notifications", ct))!,
            n => n.Target == $"user/{userId}");
        using var dismissed = await owner.PostAsync($"/api/v1/dismiss/click/{first.Id}", null, ct);
        Assert.Equal(HttpStatusCode.OK, dismissed.StatusCode);
        Assert.DoesNotContain((await owner.GetFromJsonAsync<List<Notification>>("/api/v1/notifications/me", ct))!,
            n => n.Id == first.Id);
        var summary = await owner.GetFromJsonAsync<ResourceUsageSummary>("/api/v1/resource-usage/me", ct);
        Assert.NotNull(summary);
        Assert.True(summary.IsQuotaReached);
        Assert.Equal(0m, summary.ApiCalls);
        Assert.Equal(0m, summary.BandwidthMb);

        await seed.ServiceProvider.GetRequiredService<IResourceUsageService>()
            .EraseAsync(userId, "admin", new() { Points = 501m }, ct);
        await ResourceUsageTracker.RecordAsync(db, userId, userId, cpuSeconds: 10m, cancellationToken: ct);
        var visible = await owner.GetFromJsonAsync<List<Notification>>("/api/v1/notifications/me", ct);
        Assert.NotNull(visible);
        Assert.DoesNotContain(visible, n => n.Id == first.Id);
        Assert.Single(visible, n => n.Target == $"user/{userId}" && n.tag == "resource-usage/50" && n.Id != first.Id);
    }

    [Theory]
    [InlineData(true, 5)]
    [InlineData(false, 2)]
    public async Task Profile_reports_effective_paid_quota_then_base_quota_after_expiry(bool premium, long multiplier)
    {
        var ct = TestContext.Current.CancellationToken;
        var userId = premium ? "premium-disk-profile" : "discovery-disk-profile";
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Yavsc.Models.ApplicationDbContext>();
        var user = new Yavsc.Models.ApplicationUser
        {
            Id = userId, UserName = userId, NormalizedUserName = userId.ToUpperInvariant(),
            Email = $"{userId}@example.test", DiskQuota = 1234L,
            PremiumDiskQuotaUntil = premium ? DateTimeOffset.UtcNow.AddDays(1) : null,
            DiscoveryDiskQuotaUntil = premium ? null : DateTimeOffset.UtcNow.AddDays(1)
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        using var http = NewClient(_fixture, userId);
        var profile = await http.GetFromJsonAsync<Yavsc.Abstract.Identity.Me>("/api/v1/account/me", ct);
        Assert.NotNull(profile);
        Assert.Equal(Yavsc.Constants.DefaultFSQ * multiplier, profile.DiskQuota);
        user.PremiumDiskQuotaUntil = DateTimeOffset.UtcNow.AddSeconds(-1);
        user.DiscoveryDiskQuotaUntil = DateTimeOffset.UtcNow.AddSeconds(-1);
        await db.SaveChangesAsync(ct);
        profile = await http.GetFromJsonAsync<Yavsc.Abstract.Identity.Me>("/api/v1/account/me", ct);
        Assert.NotNull(profile);
        Assert.Equal(1234L, profile.DiskQuota);
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
    public async Task GetCurrentUserUsage_does_not_consume_quota()
    {
        using var http = NewClient(_fixture, "quota-consultation-user");

        var response = await http.GetAsync("/api/v1/resource-usage/me", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Yavsc.Models.ApplicationDbContext>();
        var record = await db.ResourceUsageRecords
            .SingleAsync(r => r.UserId == "quota-consultation-user");

        Assert.Equal(0m, record.ApiCalls);
        Assert.Equal(0m, record.BandwidthMb);
    }

    [Fact]
    public async Task Exhausted_quota_blocks_operations_but_not_usage_consultation()
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Yavsc.Models.ApplicationDbContext>();
        await ResourceUsageTracker.RecordAsync(db, "exhausted-quota-user", "exhausted-quota-user", cpuSeconds: 10000m);
        using var http = NewClient(_fixture, "exhausted-quota-user");

        var refused = await http.GetAsync("/api/v1/activity", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.TooManyRequests, refused.StatusCode);
        Assert.Equal("application/problem+json", refused.Content.Headers.ContentType?.MediaType);
        Assert.Contains("resource_quota_exceeded",
            await refused.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        var consulted = await http.GetAsync("/api/v1/resource-usage/me", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, consulted.StatusCode);
        var summary = await consulted.Content.ReadFromJsonAsync<ResourceUsageSummary>(TestContext.Current.CancellationToken);
        Assert.NotNull(summary);
        Assert.Equal(1000m, summary.Consumption);
        Assert.Equal(1000m, summary.ConsumptionLimit);
        Assert.Equal(0m, summary.RemainingConsumption);
        Assert.True(summary.IsQuotaReached);
        Assert.Equal(0m, summary.ApiCalls);
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
                Target = Constants.Administration,
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
                new Claim(Yavsc.Constants.RoleClaimType, Yavsc.Constants.AdminName)
            });

        var response = await adminHttp.GetAsync("/api/v1/resource-usage/admin", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<List<ResourceUsageSummary>>(TestContext.Current.CancellationToken);
        Assert.NotNull(payload);
        Assert.Contains(payload!, item => item.UserId == "alice");
    }

    [Theory]
    [InlineData("Moderator", HttpStatusCode.Forbidden)]
    [InlineData("Administrator", HttpStatusCode.OK)]
    [InlineData("Performer", HttpStatusCode.Forbidden)]
    [InlineData("moderator", HttpStatusCode.Forbidden)]
    [InlineData("", HttpStatusCode.Forbidden)]
    public async Task Administration_read_endpoints_enforce_roles(string role, HttpStatusCode expected)
    {
        using var http = NewClient(_fixture, "reader", extraClaims:
            [new Claim(Yavsc.Constants.RoleClaimType, role)]);
        foreach (var path in new[] { "/api/v1/users", "/api/v1/users/search/alice", "/api/v1/resource-usage/admin" })
        {
            var response = await http.GetAsync(path, TestContext.Current.CancellationToken);
            Assert.Equal(expected, response.StatusCode);
        }
    }

    [Fact]
    public async Task Moderator_cannot_access_administrator_only_user_operations()
    {
        using var http = NewClient(_fixture, "moderator", extraClaims:
            [new Claim(Yavsc.Constants.RoleClaimType, Yavsc.Constants.BlogModeratorGroupName)]);
        foreach (var method in new[] { HttpMethod.Get, HttpMethod.Put, HttpMethod.Delete, HttpMethod.Post })
        {
            using var request = new HttpRequestMessage(method,
                method == HttpMethod.Post ? "/api/v1/users" : "/api/v1/users/alice");
            using var response = await http.SendAsync(request, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    [Fact]
    public async Task Anonymous_user_cannot_read_administration_data()
    {
        using var http = NewClient(_fixture);
        http.DefaultRequestHeaders.Authorization = null;
        foreach (var path in new[] { "/api/v1/users", "/api/v1/users/search/alice", "/api/v1/resource-usage/admin" })
        {
            using var response = await http.GetAsync(path, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
