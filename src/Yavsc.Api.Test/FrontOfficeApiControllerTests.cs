using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Api.Test.Fixtures;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Billing;
using Yavsc.Models.Workflow;
using Yavsc.Tests.Shared;

namespace Yavsc.Api.Test;

[Collection("Yavsc Api")]
public sealed class FrontOfficeApiControllerTests : IClassFixture<ApiWebServerFixture>
{
    private readonly ApiWebServerFixture _fixture;

    public FrontOfficeApiControllerTests(ApiWebServerFixture fixture)
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

    // Minimal valid PostIt wire-format signature: one stroke of one
    // point (k=1, then 2*k coords). See PostIt.Models.SignaturePadData.
    private static readonly int[] SampleStrokes = { 1, 5000, 5000 };

    private static HttpContent NoSignature() => JsonBody(new { });

    private static HttpContent WithSignature() =>
        JsonBody(new { strokes = SampleStrokes, coordinateMax = 10_000 });

    private static JsonContent JsonBody(object value) =>
        JsonContent.Create(value, mediaType: new MediaTypeHeaderValue("application/json"));

    /// <summary>
    /// Seeds an Rdv query with distinct parties (provider alice,
    /// client bob) and, when <paramref name="withEstimate"/> is true,
    /// an estimate linked to it via CommandId. Returns the query id
    /// and (when seeded) the estimate id.
    /// </summary>
    private (long queryId, long estimateId) SeedQuery(bool withEstimate)
    {
        WorkflowHelpers.ConfigureBillingService();
        _fixture.ResetAndSeedActivityGraph();

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var location = db.Locations.Single(l => l.Address == "1 rue du Test");

        var query = new RdvQuery
        {
            ActivityCode = "dev",
            ClientId = "bob",
            PerformerId = "alice",
            Consent = true,
            UserCreated = "alice",
            UserModified = "alice",
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow,
            EventDate = DateTime.UtcNow.AddDays(1),
            Location = location,
            Reason = "Initial rendez-vous",
            Status = Yavsc.QueryStatus.Inserted,
        };
        db.RdvQueries.Add(query);
        db.SaveChanges();

        long estimateId = 0;
        if (withEstimate)
        {
            var estimate = new Estimate
            {
                CommandId = query.Id,
                ClientId = "bob",
                OwnerId = "alice",
                CommandType = BillingCodes.Rdv,
                Title = "Devis prestation",
                Description = "Devis de test",
                AttachedFiles = new List<string>(),
                AttachedGraphics = new List<string>(),
                Bill = new List<CommandLine>
                {
                    new() { Name = "Prestation", Description = "Prestation de base", Count = 1, UnitaryCost = 120m, Currency = "EUR" },
                },
            };
            db.Estimates.Add(estimate);
            db.SaveChanges();
            estimateId = estimate.Id;
        }

        return (query.Id, estimateId);
    }

    [Fact]
    public async Task Front_accept_query_updates_status_without_server_error()
    {
        var (queryId, _) = SeedQuery(withEstimate: false);

        using var http = NewClient();
        var response = await http.PostAsync(
            $"/api/v1/front/query/accept?billingCode=Rdv&queryId={queryId}",
            NoSignature(),
            TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.True(response.StatusCode == HttpStatusCode.OK, $"Unexpected status {(int)response.StatusCode} ({response.StatusCode}): {body}");

        using var assertScope = _fixture.Services.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updated = assertDb.RdvQueries.Single(q => q.Id == queryId);

        // Default caller is "alice" (the provider) → ProAccepted.
        Assert.Equal(QueryStatus.ProAccepted, updated.Status);
    }

    [Fact]
    public async Task Provider_accept_with_signature_sets_ProValidationDate_and_Pro_signature()
    {
        var (queryId, estimateId) = SeedQuery(withEstimate: true);

        using var http = NewClient(subject: "alice");
        var response = await http.PostAsync(
            $"/api/v1/front/query/accept?billingCode=Rdv&queryId={queryId}",
            WithSignature(),
            TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.True(response.StatusCode == HttpStatusCode.OK, $"Unexpected status {(int)response.StatusCode} ({response.StatusCode}): {body}");

        using var assertScope = _fixture.Services.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Assert.Equal(QueryStatus.ProAccepted, assertDb.RdvQueries.Single(q => q.Id == queryId).Status);

        var estimate = assertDb.Estimates.Single(e => e.Id == estimateId);
        Assert.True(estimate.ProviderValidationDate != default);
        Assert.True(estimate.ClientValidationDate == default);

        var signature = assertDb.Signatures.Single(s => s.EstimateId == estimateId);
        Assert.Equal(SignatureType.Pro, signature.Type);
        Assert.Equal("alice", signature.SignerId);
    }

    [Fact]
    public async Task Client_accept_with_signature_sets_ClientValidationDate_and_Client_signature()
    {
        var (queryId, estimateId) = SeedQuery(withEstimate: true);

        using var http = NewClient(subject: "bob");
        var response = await http.PostAsync(
            $"/api/v1/front/query/accept?billingCode=Rdv&queryId={queryId}",
            WithSignature(),
            TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.True(response.StatusCode == HttpStatusCode.OK, $"Unexpected status {(int)response.StatusCode} ({response.StatusCode}): {body}");

        using var assertScope = _fixture.Services.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Assert.Equal(QueryStatus.ClientAccepted, assertDb.RdvQueries.Single(q => q.Id == queryId).Status);

        var estimate = assertDb.Estimates.Single(e => e.Id == estimateId);
        Assert.True(estimate.ClientValidationDate != default);
        Assert.True(estimate.ProviderValidationDate == default);

        var signature = assertDb.Signatures.Single(s => s.EstimateId == estimateId);
        Assert.Equal(SignatureType.Client, signature.Type);
        Assert.Equal("bob", signature.SignerId);
    }

    [Fact]
    public async Task Reject_sets_status_Rejected()
    {
        var (queryId, _) = SeedQuery(withEstimate: false);

        using var http = NewClient(subject: "alice");
        var response = await http.PostAsync(
            $"/api/v1/front/query/reject?billingCode=Rdv&queryId={queryId}",
            NoSignature(),
            TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.True(response.StatusCode == HttpStatusCode.OK, $"Unexpected status {(int)response.StatusCode} ({response.StatusCode}): {body}");

        using var assertScope = _fixture.Services.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(QueryStatus.Rejected, assertDb.RdvQueries.Single(q => q.Id == queryId).Status);
    }

    [Fact]
    public async Task Non_party_actor_is_forbidden()
    {
        var (queryId, _) = SeedQuery(withEstimate: false);

        using var http = NewClient(subject: "carol");
        var response = await http.PostAsync(
            $"/api/v1/front/query/accept?billingCode=Rdv&queryId={queryId}",
            NoSignature(),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Accept_with_signature_but_no_estimate_returns_400()
    {
        var (queryId, _) = SeedQuery(withEstimate: false);

        using var http = NewClient(subject: "alice");
        var response = await http.PostAsync(
            $"/api/v1/front/query/accept?billingCode=Rdv&queryId={queryId}",
            WithSignature(),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Reject_with_signature_returns_400()
    {
        var (queryId, _) = SeedQuery(withEstimate: true);

        using var http = NewClient(subject: "alice");
        var response = await http.PostAsync(
            $"/api/v1/front/query/reject?billingCode=Rdv&queryId={queryId}",
            WithSignature(),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}