using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Api.Test.Fixtures;
using Yavsc.Models;
using Yavsc.Models.Haircut;
using Yavsc.Models.Relationship;
using Yavsc.Tests.Shared;

namespace Yavsc.Api.Test;

[Collection("Yavsc Api")]
public sealed class HairMultiCutQueryApiControllerTests : IClassFixture<ApiWebServerFixture>
{
    private readonly ApiWebServerFixture _fixture;

    public HairMultiCutQueryApiControllerTests(ApiWebServerFixture fixture)
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
    public async Task Billing_mbrush_route_supports_crud()
    {
        _fixture.ResetAndSeedHaircutGraph();
        using var http = NewClient();

        var prestationIds = await GetPrestationIdsAsync();

        var createPayload = new HairMultiCutQuery
        {
            ActivityCode = "mbrush",
            PerformerId = "alice",
            Consent = true,
            EventDate = DateTime.UtcNow.AddDays(6),
            Location = new Location
            {
                Address = "3 rue du Groupe",
                Latitude = 48.8580,
                Longitude = 2.3530,
            },
            Prestations = prestationIds.Select(id => new HairPrestationCollectionItem { PrestationId = id }).ToList(),
            Status = QueryStatus.Inserted,
        };

        var createResponse = await http.PostAsJsonAsync("/api/v1/billing/MBrush", createPayload, TestContext.Current.CancellationToken);
        if (createResponse.StatusCode != HttpStatusCode.Created)
        {
            var body = await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.Fail($"Unexpected status {createResponse.StatusCode}: {body}");
        }

        var created = await createResponse.Content.ReadFromJsonAsync<HairMultiCutQuery>(TestContext.Current.CancellationToken);
        Assert.NotNull(created);
        Assert.NotEqual(0, created!.Id);
        Assert.Equal("alice", created.ClientId);
        Assert.Equal(2, created.Prestations.Count);

        var getResponse = await http.GetAsync($"/api/v1/billing/MBrush/{created.Id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<HairMultiCutQuery>(TestContext.Current.CancellationToken);
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal(2, fetched.Prestations.Count);

        fetched.Status = QueryStatus.Accepted;
        var putResponse = await http.PutAsJsonAsync($"/api/v1/billing/MBrush/{fetched.Id}", fetched, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

        var deleteResponse = await http.DeleteAsync($"/api/v1/billing/MBrush/{fetched.Id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var missingResponse = await http.GetAsync($"/api/v1/billing/MBrush/{fetched.Id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
    }

    [Fact]
    public async Task Billing_mbrush_route_exposes_prestation_catalog()
    {
        _fixture.ResetAndSeedHaircutGraph();
        using var http = NewClient();

        var response = await http.GetAsync("/api/v1/billing/MBrush/prestations", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var catalog = await response.Content.ReadFromJsonAsync<List<HairPrestationDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(catalog);
        Assert.NotEmpty(catalog!);
        Assert.All(catalog!, item => Assert.False(string.IsNullOrWhiteSpace(item.Details)));
        Assert.Contains(catalog!, item => item.Title == "Femme · Cheveux mi-longs"
            && item.Details == "Coupe · Brushing · Aucune technique spécifique · Shampoing · Sans soins");
    }

    private async Task<List<long>> GetPrestationIdsAsync()
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.HairPrestation
            .OrderBy(p => p.Id)
            .Select(p => p.Id)
            .Take(2)
            .ToListAsync(TestContext.Current.CancellationToken);
    }
}