using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Yavsc.Api.Test.Fixtures;
using Yavsc.Models;
using Yavsc.Models.Haircut;
using Yavsc.Models.Relationship;
using Yavsc.Tests.Shared;

namespace Yavsc.Api.Test;

[Collection("Yavsc Api")]
public sealed class HairCutQueryApiControllerTests : IClassFixture<ApiWebServerFixture>
{
    private readonly ApiWebServerFixture _fixture;

    public HairCutQueryApiControllerTests(ApiWebServerFixture fixture)
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
    public async Task Billing_brush_route_supports_crud()
    {
        _fixture.ResetAndSeedHaircutGraph();
        using var http = NewClient();

        var prestationId = await GetPrestationIdAsync();

        var createPayload = new HairCutQuery
        {
            ActivityCode = "brush",
            PerformerId = "alice",
            Consent = true,
            EventDate = DateTime.UtcNow.AddDays(5),
            Location = new Location
            {
                Address = "2 rue de la Coupe",
                Latitude = 48.8570,
                Longitude = 2.3525,
            },
            PrestationId = prestationId,
            AdditionalInfo = "Brushing test",
            Status = QueryStatus.Inserted,
            Description = "Haircut create",
        };

        var createResponse = await http.PostAsJsonAsync("/api/v1/billing/Brush", createPayload, TestContext.Current.CancellationToken);
        if (createResponse.StatusCode != HttpStatusCode.Created)
        {
            var body = await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.Fail($"Unexpected status {createResponse.StatusCode}: {body}");
        }

        var created = await createResponse.Content.ReadFromJsonAsync<HairCutQuery>(TestContext.Current.CancellationToken);
        Assert.NotNull(created);
        Assert.NotEqual(0, created!.Id);
        Assert.Equal("alice", created.ClientId);

        var getResponse = await http.GetAsync($"/api/v1/billing/Brush/{created.Id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<HairCutQuery>(TestContext.Current.CancellationToken);
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal("Brushing test", fetched.AdditionalInfo);

        fetched.AdditionalInfo = "Brushing modifié";
        var putResponse = await http.PutAsJsonAsync($"/api/v1/billing/Brush/{fetched.Id}", fetched, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

        var deleteResponse = await http.DeleteAsync($"/api/v1/billing/Brush/{fetched.Id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var missingResponse = await http.GetAsync($"/api/v1/billing/Brush/{fetched.Id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
    }

    [Fact]
    public async Task Billing_brush_route_exposes_prestation_catalog()
    {
        _fixture.ResetAndSeedHaircutGraph();
        using var http = NewClient();

        var response = await http.GetAsync("/api/v1/billing/Brush/prestations", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var catalog = await response.Content.ReadFromJsonAsync<List<HairPrestationDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(catalog);
        Assert.NotEmpty(catalog!);
        Assert.All(catalog!, item => Assert.False(string.IsNullOrWhiteSpace(item.Title)));
        Assert.Contains(catalog!, item => item.Title == "Femme · Cheveux mi-longs"
            && item.Details == "Coupe · Brushing · Aucune technique spécifique · Shampoing · Sans soins");
    }

    private async Task<long> GetPrestationIdAsync()
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.HairPrestation.Select(p => p.Id).FirstAsync(TestContext.Current.CancellationToken);
    }
}