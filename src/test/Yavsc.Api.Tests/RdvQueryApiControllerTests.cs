using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Yavsc;
using Yavsc.Api.Test.Fixtures;
using Yavsc.Models.Workflow;
using Yavsc.Tests.Shared;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Models;
using Yavsc.Services;

namespace Yavsc.Api.Test;

[Collection("Yavsc Api")]
public sealed class RdvQueryApiControllerTests : IClassFixture<ApiWebServerFixture>
{
    private readonly ApiWebServerFixture _fixture;

    public RdvQueryApiControllerTests(ApiWebServerFixture fixture)
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
    public async Task PostQuery_notifies_performer_by_database_email_and_signalr()
    {
        _fixture.ResetAndSeedRdvQueryGraph();
        var manager = _fixture.Services.GetRequiredService<IConnexionManager>();
        manager.OnConnected("rdv-alice", "alice", false);
        try
        {
            using var http = NewClient("bob");
            var response = await http.PostAsJsonAsync("/api/v1/billing/Rdv", new
            {
                ActivityCode = "dev", PerformerId = "alice", ClientId = "spoofed",
                Consent = true, EventDate = DateTime.UtcNow.AddDays(1),
                Location = new { Address = "Notification test", Latitude = 48.0, Longitude = 2.0 },
                Reason = "Demande notifiée", Status = QueryStatus.Inserted
            }, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var query = await response.Content.ReadFromJsonAsync<RdvQuery>(TestContext.Current.CancellationToken);
            using var scope = _fixture.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var notification = Assert.Single(db.Notification);
            Assert.Equal("user/alice", notification.Target);
            Assert.Equal($"rdv/{query!.Id}", notification.tag);
            Assert.Contains("bob", notification.title);
            var delivery = _fixture.Services.GetRequiredService<RecordingMessageDelivery>();
            Assert.Equal("alice@example.test", Assert.Single(delivery.Emails).Recipient);
            var push = Assert.Single(delivery.Pushes);
            Assert.Equal("rdv-alice", push.ConnectionId);
            Assert.Equal("push", push.Method);
            Assert.Equal("/topic/RdvQuery", push.Arguments[0]);
        }
        finally
        {
            manager.OnDisconnected("rdv-alice");
        }
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task PostQuery_respects_performer_contact_preferences(bool acceptsNotifications, bool acceptsPublicContact)
    {
        _fixture.ResetAndSeedRdvQueryGraph();
        using (var scope = _fixture.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var performer = db.Performers.Single(p => p.PerformerId == "alice");
            performer.AcceptNotifications = acceptsNotifications;
            performer.AcceptPublicContact = acceptsPublicContact;
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
        using var http = NewClient("bob");
        var response = await http.PostAsJsonAsync("/api/v1/billing/Rdv", new
        {
            ActivityCode = "dev", PerformerId = "alice", Consent = true,
            EventDate = DateTime.UtcNow.AddDays(1),
            Location = new { Address = "Contact refused", Latitude = 48.0, Longitude = 2.0 },
            Reason = "Demande", Status = QueryStatus.Inserted
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var assertScope = _fixture.Services.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Empty(assertDb.Notification);
        Assert.Empty(_fixture.Services.GetRequiredService<RecordingMessageDelivery>().Emails);
    }

    [Fact]
    public async Task Billing_rdv_route_supports_crud()
    {
        _fixture.ResetAndSeedRdvQueryGraph();
        using var http = NewClient();

        var createPayload = new RdvQuery
        {
            ActivityCode = "dev",
            PerformerId = "alice",
            Consent = true,
            EventDate = DateTime.UtcNow.AddDays(2),
            Location = new Yavsc.Models.Relationship.Location
            {
                Address = "1 rue du Test",
                Latitude = 48.8566,
                Longitude = 2.3522,
            },
            Reason = "Second rendez-vous",
            Status = QueryStatus.Inserted,
        };

        var createResponse = await http.PostAsJsonAsync("/api/v1/billing/Rdv", createPayload, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<RdvQuery>(TestContext.Current.CancellationToken);
        Assert.NotNull(created);
        Assert.NotEqual(0, created!.Id);
        Assert.Equal("alice", created.ClientId);

        var getResponse = await http.GetAsync($"/api/v1/billing/Rdv/{created.Id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<RdvQuery>(TestContext.Current.CancellationToken);
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal("Second rendez-vous", fetched.Reason);

        fetched.Reason = "Rendez-vous modifié";
        var putResponse = await http.PutAsJsonAsync($"/api/v1/billing/Rdv/{fetched.Id}", fetched, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

        var deleteResponse = await http.DeleteAsync($"/api/v1/billing/Rdv/{fetched.Id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var missingResponse = await http.GetAsync($"/api/v1/billing/Rdv/{fetched.Id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
    }

    [Fact]
    public async Task PostQuery_ignores_client_field_and_uses_authenticated_user()
    {
        _fixture.ResetAndSeedRdvQueryGraph();
        using var http = NewClient(subject: "alice");

        var createPayload = new
        {
            ActivityCode = "dev",
            PerformerId = "alice",
            Consent = true,
            EventDate = DateTime.UtcNow.AddDays(2),
            Location = new
            {
                Address = "2 rue du Test",
                Latitude = 48.8567,
                Longitude = 2.3523,
            },
            Reason = "Rendez-vous sans champ client",
            Status = QueryStatus.Inserted,
        };

        var createResponse = await http.PostAsJsonAsync("/api/v1/billing/Rdv", createPayload, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<RdvQuery>(TestContext.Current.CancellationToken);
        Assert.NotNull(created);
        Assert.Equal("alice", created!.ClientId);
    }

    [Fact]
    public async Task PostQuery_accepts_local_datetime_and_persists_as_utc()
    {
        _fixture.ResetAndSeedRdvQueryGraph();
        using var http = NewClient(subject: "alice");

        var localEventDate = DateTime.Now.AddDays(2);
        var createPayload = new
        {
            ActivityCode = "dev",
            PerformerId = "alice",
            Consent = true,
            EventDate = localEventDate,
            Location = new
            {
                Address = "3 rue du Test",
                Latitude = 48.8568,
                Longitude = 2.3524,
            },
            Reason = "Rendez-vous date locale",
            Status = QueryStatus.Inserted,
        };

        var createResponse = await http.PostAsJsonAsync("/api/v1/billing/Rdv", createPayload, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<RdvQuery>(TestContext.Current.CancellationToken);
        Assert.NotNull(created);
        Assert.Equal(DateTimeKind.Utc, created!.EventDate.Kind);
    }

    [Fact]
    public async Task PostQuery_with_unknown_location_id_creates_location_and_succeeds()
    {
        _fixture.ResetAndSeedRdvQueryGraph();
        using var http = NewClient(subject: "alice");

        var createPayload = new
        {
            ActivityCode = "dev",
            PerformerId = "alice",
            Consent = true,
            EventDate = DateTime.UtcNow.AddDays(3),
            Location = new
            {
                Id = 999999L,
                Address = "4 rue du Test",
                Latitude = 48.8569,
                Longitude = 2.3525,
            },
            Reason = "Rendez-vous id location inconnu",
            Status = QueryStatus.Inserted,
        };

        var createResponse = await http.PostAsJsonAsync("/api/v1/billing/Rdv", createPayload, TestContext.Current.CancellationToken);
        var body = await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.True(createResponse.StatusCode == HttpStatusCode.Created, $"Unexpected status {(int)createResponse.StatusCode} ({createResponse.StatusCode}): {body}");

        var created = await createResponse.Content.ReadFromJsonAsync<RdvQuery>(TestContext.Current.CancellationToken);
        Assert.NotNull(created);
        Assert.NotNull(created!.Location);
        Assert.True(created.Location.Id > 0);
        Assert.NotEqual(999999L, created.Location.Id);
        Assert.Equal("alice", created.ClientId);
    }

    [Fact]
    public async Task PostQuery_without_location_returns_bad_request()
    {
        _fixture.ResetAndSeedRdvQueryGraph();
        using var http = NewClient(subject: "alice");

        var createPayload = new
        {
            ActivityCode = "dev",
            PerformerId = "alice",
            Consent = true,
            EventDate = DateTime.UtcNow.AddDays(1),
            Reason = "Rendez-vous sans location",
            Status = QueryStatus.Inserted,
        };

        var createResponse = await http.PostAsJsonAsync("/api/v1/billing/Rdv", createPayload, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, createResponse.StatusCode);
    }

    [Fact]
    public async Task PostQuery_with_unknown_location_id_and_missing_address_returns_bad_request()
    {
        _fixture.ResetAndSeedRdvQueryGraph();
        using var http = NewClient(subject: "alice");

        var createPayload = new
        {
            ActivityCode = "dev",
            PerformerId = "alice",
            Consent = true,
            EventDate = DateTime.UtcNow.AddDays(1),
            Location = new
            {
                Id = 777777L,
                Address = "",
                Latitude = 0.0,
                Longitude = 0.0,
            },
            Reason = "Rendez-vous location invalide",
            Status = QueryStatus.Inserted,
        };

        var createResponse = await http.PostAsJsonAsync("/api/v1/billing/Rdv", createPayload, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, createResponse.StatusCode);
    }
}
