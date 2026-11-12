using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Abstract.Resources;
using Yavsc.Api.Test.Fixtures;
using Yavsc.Models;
using Yavsc.Server.Helpers;
using Yavsc.Tests.Shared;

namespace Yavsc.Api.Test;

[Trait("Category", "non-reg")]
[Collection("Yavsc Api")]
public sealed class ResourceUsageReliefApiTests(ApiWebServerFixture fixture) : IClassFixture<ApiWebServerFixture>
{
    private const string Root = "/api/v1/resource-usage/admin";
    private CancellationToken Ct => TestContext.Current.CancellationToken;

    private HttpClient Client(string? role = "Administrator", string subject = "relief-admin")
    {
        var http = new HttpClient(new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true })
        { BaseAddress = new Uri(fixture.BaseAddress) };
        if (role is not null)
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                TestTokenIssuer.Issue(subject, "api", extraClaims: [new Claim(Yavsc.Constants.RoleClaimType, role)]));
        return http;
    }

    private async Task<string> SeedAsync(decimal cpu = 10000m, bool withdrawn = false)
    {
        var id = "relief-" + Guid.NewGuid().ToString("N");
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Users.Add(new ApplicationUser
        {
            Id = id, UserName = id, NormalizedUserName = id.ToUpperInvariant(),
            Email = id + "@example.test", WithdrawalRequestedAt = withdrawn ? DateTimeOffset.UtcNow : null
        });
        await db.SaveChangesAsync(Ct);
        db.ResourceUsageRecords.Add(new ResourceUsageRecord { UserId = id, UserName = id, CpuSeconds = cpu });
        await db.SaveChangesAsync(Ct);
        return id;
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("Moderator", HttpStatusCode.Forbidden)]
    [InlineData("Performer", HttpStatusCode.Forbidden)]
    [InlineData("administrator", HttpStatusCode.Forbidden)]
    public async Task Only_administrators_can_erase_points(string? role, HttpStatusCode expected)
    {
        var id = await SeedAsync();
        using var client = Client(role);
        using var response = await client.PostAsJsonAsync($"{Root}/{id}/erase", new EraseResourceUsageRequest { Points = 100m }, Ct);
        Assert.Equal(expected, response.StatusCode);
        using var scope = fixture.Services.CreateScope();
        Assert.Equal(0m, (await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
            .ResourceUsageRecords.SingleAsync(r => r.UserId == id, Ct)).CreditPoints);
    }

    [Fact]
    public async Task Erasure_works_at_administrator_quota_preserves_counters_and_unblocks_user()
    {
        var id = await SeedAsync();
        using (var seed = fixture.Services.CreateScope())
            await ResourceUsageTracker.RecordAsync(seed.ServiceProvider.GetRequiredService<ApplicationDbContext>(),
                "relief-admin-exhausted", "Admin", cpuSeconds: 20000m, cancellationToken: Ct);
        using var admin = Client(subject: "relief-admin-exhausted");
        using var target = Client("Performer", id);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await target.GetAsync("/api/v1/activity", Ct)).StatusCode);
        var request = new EraseResourceUsageRequest { Points = 200.12545678m, ExpectedCredits = 0m };
        using var response = await admin.PostAsJsonAsync($"{Root}/{id}/erase", request, Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.True(response.Headers.CacheControl?.NoStore);
        var summary = await response.Content.ReadFromJsonAsync<ResourceUsageSummary>(Ct);
        Assert.NotNull(summary);
        Assert.Equal(799.87454322m, summary.Consumption);
        Assert.Equal(0m, summary.CreditBalance);
        Assert.False(summary.IsQuotaReached);
        Assert.Equal(HttpStatusCode.OK, (await target.GetAsync("/api/v1/activity", Ct)).StatusCode);
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var record = await db.ResourceUsageRecords.SingleAsync(r => r.UserId == id, Ct);
        Assert.Equal(10000m, record.CpuSeconds);
        Assert.Equal(request.Points, record.CreditPoints);
        var actor = await db.ResourceUsageRecords.SingleAsync(r => r.UserId == "relief-admin-exhausted", Ct);
        Assert.Equal(0m, actor.ApiCalls);
        Assert.Equal(0m, actor.BandwidthMb);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("0.000000001")]
    [InlineData("100000000000000000000")]
    public async Task Invalid_amount_returns_problem_details_and_does_not_change_credits(string points)
    {
        var id = await SeedAsync();
        using var client = Client();
        using var body = new StringContent("{\"points\":" + points + ",\"expectedCredits\":0}", Encoding.UTF8, "application/json");
        using var response = await client.PostAsync($"{Root}/{id}/erase", body, Ct);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>(Ct);
        Assert.NotNull(problem);
        Assert.Equal((int)response.StatusCode, problem.Status);
        Assert.False(string.IsNullOrWhiteSpace(problem.Title));
        using var scope = fixture.Services.CreateScope();
        Assert.Equal(0m, (await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
            .ResourceUsageRecords.SingleAsync(r => r.UserId == id, Ct)).CreditPoints);
    }

    [Theory]
    [InlineData("excess", HttpStatusCode.Conflict)]
    [InlineData("stale", HttpStatusCode.Conflict)]
    [InlineData("below", HttpStatusCode.Conflict)]
    [InlineData("withdrawn", HttpStatusCode.Conflict)]
    [InlineData("missing", HttpStatusCode.NotFound)]
    public async Task Conflicts_and_unknown_users_do_not_grant_credit(string scenario, HttpStatusCode status)
    {
        var id = scenario == "missing" ? "relief-missing-user" :
            await SeedAsync(scenario == "below" ? 100m : 10000m, scenario == "withdrawn");
        using var client = Client();
        using var response = await client.PostAsJsonAsync($"{Root}/{id}/erase", new EraseResourceUsageRequest
        {
            Points = scenario == "excess" ? 1001m : 100m, ExpectedCredits = scenario == "stale" ? 1m : 0m
        }, Ct);
        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>(Ct);
        Assert.NotNull(problem);
        Assert.Equal((int)status, problem.Status);
        Assert.False(string.IsNullOrWhiteSpace(problem.Title));
    }

    [Fact]
    public async Task Repeated_confirmation_cannot_erase_twice_even_when_quota_remains_reached()
    {
        var id = await SeedAsync(30000m);
        using var client = Client();
        var request = new EraseResourceUsageRequest { Points = 100m };
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync($"{Root}/{id}/erase", request, Ct)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync($"{Root}/{id}/erase", request, Ct)).StatusCode);
        using var scope = fixture.Services.CreateScope();
        Assert.Equal(100m, (await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
            .ResourceUsageRecords.SingleAsync(r => r.UserId == id, Ct)).CreditPoints);
    }

    [Fact]
    public async Task Openapi_documents_the_admin_action_and_its_status_codes()
    {
        using var client = Client();
        var doc = await client.GetFromJsonAsync<JsonElement>("/openapi/administration.json", Ct);
        var operation = doc.GetProperty("paths").GetProperty(Root + "/{userId}/erase").GetProperty("post");
        Assert.False(string.IsNullOrWhiteSpace(operation.GetProperty("summary").GetString()));
        foreach (var status in new[] { "200", "400", "401", "403", "404", "409" })
            Assert.True(operation.GetProperty("responses").TryGetProperty(status, out _));
        Assert.True(operation.GetProperty("responses").GetProperty("200").GetProperty("content")
            .TryGetProperty("application/json", out _));
        foreach (var status in new[] { "400", "404", "409" })
            Assert.True(operation.GetProperty("responses").GetProperty(status).GetProperty("content")
                .TryGetProperty("application/problem+json", out _));
    }
}
