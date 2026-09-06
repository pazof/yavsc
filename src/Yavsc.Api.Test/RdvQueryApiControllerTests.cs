using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Yavsc;
using Yavsc.Api.Test.Fixtures;
using Yavsc.Models.Workflow;
using Yavsc.Tests.Shared;

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
}
