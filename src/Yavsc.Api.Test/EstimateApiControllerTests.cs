using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Api.Test.Fixtures;
using Yavsc.Models;
using Yavsc.Models.Billing;
using Yavsc.Models.Workflow;
using Yavsc.Tests.Shared;

namespace Yavsc.Api.Test;

[Collection("Yavsc Api")]
public sealed class EstimateApiControllerTests : IClassFixture<ApiWebServerFixture>
{
    private readonly ApiWebServerFixture _fixture;

    public EstimateApiControllerTests(ApiWebServerFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpClient NewClient(string subject = "alice", string scope = "api")
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        var http = new HttpClient(handler)
        {
            BaseAddress = new Uri(_fixture.BaseAddress)
        };

        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestTokenIssuer.Issue(subject, scope));

        return http;
    }

    /// <summary>
    /// Seed a provider (alice) and a client (bob) with a pending
    /// <see cref="RdvQuery"/> from bob to alice, and return the
    /// command id.
    /// </summary>
    private long SeedPendingCommand()
    {
        _fixture.ResetAndSeedActivityGraph();

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var location = db.Locations.Single();

        var query = new RdvQuery
        {
            ActivityCode = "dev",
            ClientId = "bob",
            PerformerId = "alice",
            Consent = true,
            UserCreated = "bob",
            UserModified = "bob",
            DateCreated = DateTime.UtcNow.AddMinutes(-10),
            DateModified = DateTime.UtcNow.AddMinutes(-10),
            EventDate = DateTime.UtcNow.AddDays(1),
            Location = location,
            Reason = "Demande de devis",
            Status = QueryStatus.InProgress,
            Description = "Demande en attente de devis",
        };
        db.RdvQueries.Add(query);
        db.SaveChanges();

        return query.Id;
    }

    private static object NewEstimatePayload(long? commandId, string clientId, string? ownerId = null)
        => new
        {
            CommandId = commandId,
            ClientId = clientId,
            OwnerId = ownerId,
            CommandType = BillingCodes.Rdv,
            Title = "Devis prestation",
            Description = "Devis détaillé",
            AttachedFiles = Array.Empty<string>(),
            AttachedGraphics = Array.Empty<string>(),
            Bill = new[]
            {
                new { Name = "Prestation", Description = "Prestation de base", Count = 1, UnitaryCost = 120m, Currency = "EUR" },
                new { Name = "Remise", Description = "Remise fidélité", Count = 1, UnitaryCost = -20m, Currency = "EUR" },
            },
        };

    [Fact]
    public async Task PostEstimate_creates_the_estimate_and_validates_the_linked_command()
    {
        var commandId = SeedPendingCommand();
        using var http = NewClient("alice");

        var response = await http.PostAsJsonAsync(
            "/api/v1/estimate",
            NewEstimatePayload(commandId, clientId: "bob"),
            TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.True(response.StatusCode == HttpStatusCode.OK, $"Unexpected status {(int)response.StatusCode} ({response.StatusCode}): {body}");

        using var doc = JsonDocument.Parse(body);
        var estimateId = doc.RootElement.GetProperty("id").GetInt64();
        Assert.True(estimateId > 0);
        Assert.Equal(2, doc.RootElement.GetProperty("bill").GetArrayLength());

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var estimate = db.Estimates.Include(e => e.Bill).Single(e => e.Id == estimateId);
        Assert.Equal("alice", estimate.OwnerId);
        Assert.Equal("bob", estimate.ClientId);
        Assert.Equal(commandId, estimate.CommandId);
        Assert.Equal(BillingCodes.Rdv, estimate.CommandType);
        Assert.Equal(2, estimate.Bill.Count);
        Assert.Contains(estimate.Bill, line => line.UnitaryCost == -20m);

        // PostEstimate stamps the linked command as validated.
        var query = db.RdvQueries.Single(q => q.Id == commandId);
        Assert.NotNull(query.ValidationDate);
    }

    [Fact]
    public async Task PostEstimate_without_command_creates_a_standalone_estimate()
    {
        _fixture.ResetAndSeedActivityGraph();
        using var http = NewClient("alice");

        var response = await http.PostAsJsonAsync(
            "/api/v1/estimate",
            NewEstimatePayload(commandId: null, clientId: "bob"),
            TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.True(response.StatusCode == HttpStatusCode.OK, $"Unexpected status {(int)response.StatusCode} ({response.StatusCode}): {body}");

        using var doc = JsonDocument.Parse(body);
        var estimateId = doc.RootElement.GetProperty("id").GetInt64();

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var estimate = db.Estimates.Single(e => e.Id == estimateId);
        Assert.Null(estimate.CommandId);
        Assert.Equal("alice", estimate.OwnerId);
    }

    [Fact]
    public async Task PostEstimate_for_another_owner_is_rejected()
    {
        _fixture.ResetAndSeedActivityGraph();
        using var http = NewClient("alice");

        var response = await http.PostAsJsonAsync(
            "/api/v1/estimate",
            NewEstimatePayload(commandId: null, clientId: "bob", ownerId: "bob"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Empty(db.Estimates);
    }

    [Fact]
    public async Task PostEstimate_with_an_unknown_command_id_is_rejected()
    {
        _fixture.ResetAndSeedActivityGraph();
        using var http = NewClient("alice");

        var response = await http.PostAsJsonAsync(
            "/api/v1/estimate",
            NewEstimatePayload(commandId: 999999, clientId: "bob"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Empty(db.Estimates);
    }

    [Fact]
    public async Task PostEstimate_without_token_is_unauthorized()
    {
        _fixture.ResetAndSeedActivityGraph();

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
        using var http = new HttpClient(handler) { BaseAddress = new Uri(_fixture.BaseAddress) };

        var response = await http.PostAsJsonAsync(
            "/api/v1/estimate",
            NewEstimatePayload(commandId: null, clientId: "bob"),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
