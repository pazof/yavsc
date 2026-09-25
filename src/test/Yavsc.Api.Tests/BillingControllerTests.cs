using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Api.Test.Fixtures;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Billing;
using Yavsc.Models.Haircut;
using Yavsc.Models.Workflow;
using Yavsc.Tests.Shared;

namespace Yavsc.Api.Test;

[Collection("Yavsc Api")]
public sealed class BillingControllerTests : IClassFixture<ApiWebServerFixture>
{
    private readonly ApiWebServerFixture _fixture;

    public BillingControllerTests(ApiWebServerFixture fixture)
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

    [Fact]
    public async Task GetProviderOngoingCommands_returns_current_provider_requests()
    {
        WorkflowHelpers.ConfigureBillingService();
        _fixture.ResetAndSeedActivityGraph();

        using (var scope = _fixture.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var location = db.Locations.Single();

            db.RdvQueries.Add(new RdvQuery
            {
                ActivityCode = "dev",
                ClientId = "bob",
                PerformerId = "alice",
                Consent = true,
                UserCreated = "alice",
                UserModified = "alice",
                DateCreated = DateTime.UtcNow.AddMinutes(-10),
                DateModified = DateTime.UtcNow.AddMinutes(-8),
                EventDate = DateTime.UtcNow.AddDays(1),
                Location = location,
                Reason = "Rendez-vous fournisseur",
                Status = QueryStatus.InProgress,
                Description = "Commande fournisseur en cours",
            });

            db.RdvQueries.Add(new RdvQuery
            {
                ActivityCode = "dev",
                ClientId = "alice",
                PerformerId = "bob",
                Consent = true,
                UserCreated = "bob",
                UserModified = "bob",
                DateCreated = DateTime.UtcNow.AddMinutes(-20),
                DateModified = DateTime.UtcNow.AddMinutes(-20),
                EventDate = DateTime.UtcNow.AddDays(2),
                Location = location,
                Reason = "Commande d'un autre prestataire",
                Status = QueryStatus.Accepted,
                Description = "Autre prestataire",
            });

            db.SaveChanges();
        }

        using var http = NewClient();

        var response = await http.GetAsync("/api/v1/bill/provider/ongoing", TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.True(response.StatusCode == HttpStatusCode.OK, $"Unexpected status {(int)response.StatusCode} ({response.StatusCode}): {body}");

        var payload = await response.Content.ReadFromJsonAsync<List<ProviderOngoingCommandDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(payload);
        Assert.NotEmpty(payload!);
        Assert.All(payload!, item => Assert.Equal("alice", item.PerformerId));
        Assert.Contains(payload!, item => item.BillingCode == BillingCodes.Rdv);
    }

    [Fact]
    public async Task GetProviderOngoingCommands_ignores_rows_with_invalid_discriminator()
    {
        WorkflowHelpers.ConfigureBillingService();
        _fixture.ResetAndSeedActivityGraph();

        using (var scope = _fixture.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            db.Database.ExecuteSqlInterpolated($@"
INSERT INTO ""NominativeServiceCommands""
(""ActivityCode"", ""ClientId"", ""Consent"", ""DateCreated"", ""DateModified"", ""Description"", ""Discriminator"", ""PerformerId"", ""Status"", ""UserCreated"", ""UserModified"")
VALUES
({"dev"}, {"bob"}, {true}, {DateTime.UtcNow.AddMinutes(-5)}, {DateTime.UtcNow.AddMinutes(-4)}, {"Legacy malformed row"}, {""}, {"alice"}, {(int)QueryStatus.Accepted}, {"alice"}, {"alice"});
");

            var location = db.Locations.Single();
            db.RdvQueries.Add(new RdvQuery
            {
                ActivityCode = "dev",
                ClientId = "bob",
                PerformerId = "alice",
                Consent = true,
                UserCreated = "alice",
                UserModified = "alice",
                DateCreated = DateTime.UtcNow.AddMinutes(-3),
                DateModified = DateTime.UtcNow.AddMinutes(-2),
                EventDate = DateTime.UtcNow.AddDays(1),
                Location = location,
                Reason = "Commande valide",
                Status = QueryStatus.InProgress,
                Description = "Commande fournisseur valide",
            });

            db.SaveChanges();
        }

        using var http = NewClient();

        var response = await http.GetAsync("/api/v1/bill/provider/ongoing", TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.True(response.StatusCode == HttpStatusCode.OK, $"Unexpected status {(int)response.StatusCode} ({response.StatusCode}): {body}");

        var payload = await response.Content.ReadFromJsonAsync<List<ProviderOngoingCommandDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(payload);
        Assert.NotEmpty(payload!);
        Assert.Contains(payload!, item => item.BillingCode == BillingCodes.Rdv && item.PerformerId == "alice");
        Assert.DoesNotContain(payload!, item => string.IsNullOrWhiteSpace(item.BillingCode));
    }

    [Fact]
    public async Task GetProviderOngoingCommands_returns_haircut_and_grouped_haircut_requests()
    {
        WorkflowHelpers.ConfigureBillingService();
        _fixture.ResetAndSeedHaircutGraph();

        using (var scope = _fixture.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.UserActivities.Add(new UserActivity
            {
                UserId = "alice",
                DoesCode = "brush",
                Weight = 50,
            });
            db.UserActivities.Add(new UserActivity
            {
                UserId = "alice",
                DoesCode = "mbrush",
                Weight = 50,
            });
            db.CommandForm.Add(new CommandForm
            {
                ActivityCode = "brush",
                ActionName = BillingCodes.Brush,
                Title = "Brush",
            });
            db.CommandForm.Add(new CommandForm
            {
                ActivityCode = "mbrush",
                ActionName = BillingCodes.MBrush,
                Title = "MBrush",
            });
            db.SaveChanges();
        }

        using var http = NewClient();

        var response = await http.GetAsync("/api/v1/bill/provider/ongoing", TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.True(response.StatusCode == HttpStatusCode.OK, $"Unexpected status {(int)response.StatusCode} ({response.StatusCode}): {body}");

        var payload = await response.Content.ReadFromJsonAsync<List<ProviderOngoingCommandDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(payload);
        Assert.Contains(payload!, item => item.BillingCode == BillingCodes.Brush && item.PerformerId == "alice");
        Assert.Contains(payload!, item => item.BillingCode == BillingCodes.MBrush && item.PerformerId == "alice");
    }

    [Fact]
    public async Task GetProviderOngoingCommands_excludes_requests_outside_performer_declared_activities()
    {
        WorkflowHelpers.ConfigureBillingService();
        _fixture.ResetAndSeedHaircutGraph();

        using var http = NewClient();

        var response = await http.GetAsync("/api/v1/bill/provider/ongoing", TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.True(response.StatusCode == HttpStatusCode.OK, $"Unexpected status {(int)response.StatusCode} ({response.StatusCode}): {body}");

        var payload = await response.Content.ReadFromJsonAsync<List<ProviderOngoingCommandDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(payload);
        Assert.DoesNotContain(payload!, item => item.BillingCode == BillingCodes.Brush);
        Assert.DoesNotContain(payload!, item => item.BillingCode == BillingCodes.MBrush);
    }

    private sealed class ProviderOngoingCommandDto
    {
        public long Id { get; set; }
        public string BillingCode { get; set; } = string.Empty;
        public string ActivityCode { get; set; } = string.Empty;
        public string PerformerId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public QueryStatus Status { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
