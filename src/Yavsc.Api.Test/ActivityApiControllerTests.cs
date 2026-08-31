using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Abstract.Workflow;
using Yavsc.Api.Test.Fixtures;
using Yavsc.Tests.Shared;

namespace Yavsc.Api.Test;

[Collection("Yavsc Api")]
public sealed class ActivityApiControllerTests : IClassFixture<ApiWebServerFixture>
{
    private readonly ApiWebServerFixture _fixture;

    public ActivityApiControllerTests(ApiWebServerFixture fixture)
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
    public async Task GetUsers_returns_declared_user_for_exact_activity_code()
    {
        _fixture.ResetAndSeedActivityGraph();
        using var http = NewClient();

        var response = await http.GetAsync("/api/v1/activity/dev/users", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<List<ActivityPerformerDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(payload);
        Assert.Single(payload!);
        Assert.Equal("alice", payload[0].PerformerId);
        Assert.Equal("alice", payload[0].UserName);
        Assert.True(payload[0].HasPerformerProfile);
        Assert.True(payload[0].Active);
        Assert.Equal("dev", payload[0].ActivityCode);
    }

    [Fact]
    public async Task GetUsers_returns_user_even_when_performer_inactive()
    {
        _fixture.ResetAndSeedActivityGraph();
        using (var scope = _fixture.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Yavsc.Models.ApplicationDbContext>();
            var performer = db.Performers.Single(p => p.PerformerId == "alice");
            performer.Active = false;
            db.SaveChanges();
        }

        using var http = NewClient();
        var response = await http.GetAsync("/api/v1/activity/dev/users", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<List<ActivityPerformerDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(payload);
        Assert.Single(payload!);
        Assert.Equal("alice", payload[0].PerformerId);
        Assert.False(payload[0].Active);
    }

    [Fact]
    public async Task Catalog_and_Performers_are_consistent_for_dev_activity()
    {
        _fixture.ResetAndSeedActivityGraph();
        using var http = NewClient();

        var catalogResponse = await http.GetAsync("/api/v1/activity/catalog", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, catalogResponse.StatusCode);

        var catalog = await catalogResponse.Content.ReadFromJsonAsync<List<ActivityBrowseItemDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(catalog);

        var dev = catalog!.Single(a => a.Code == "dev");
        Assert.True(dev.PerformerCount > 0);

        var performersResponse = await http.GetAsync("/api/v1/activity/dev/users", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, performersResponse.StatusCode);

        var performers = await performersResponse.Content.ReadFromJsonAsync<List<ActivityPerformerDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(performers);
        Assert.Equal(dev.PerformerCount, performers!.Count);
        Assert.Contains(performers, p => p.PerformerId == "alice");
    }

    [Fact]
    public async Task Catalog_does_not_list_activity_without_declaration()
    {
        _fixture.ResetAndSeedActivityGraph();
        using var http = NewClient();

        var catalogResponse = await http.GetAsync("/api/v1/activity/catalog", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, catalogResponse.StatusCode);

        var catalog = await catalogResponse.Content.ReadFromJsonAsync<List<ActivityBrowseItemDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(catalog);
        Assert.DoesNotContain(catalog!, a => a.Code == "ghost");
        Assert.Contains(catalog!, a => a.Code == "dev");
    }

    [Fact]
    public async Task Catalog_lists_declared_activity_even_when_performer_inactive()
    {
        _fixture.ResetAndSeedActivityGraph();
        using var http = NewClient();

        var catalogResponse = await http.GetAsync("/api/v1/activity/catalog", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, catalogResponse.StatusCode);

        var catalog = await catalogResponse.Content.ReadFromJsonAsync<List<ActivityBrowseItemDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(catalog);
        Assert.Contains(catalog!, a => a.Code == "declared-only");

        var response = await http.GetAsync("/api/v1/activity/declared-only/users", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<List<ActivityPerformerDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(payload);
        Assert.Single(payload!);
        Assert.Equal("bob", payload[0].PerformerId);
        Assert.Equal("bob", payload[0].UserName);
        Assert.True(payload[0].HasPerformerProfile);
        Assert.False(payload[0].Active);
    }

    [Fact]
    public async Task Performers_alias_returns_same_payload_as_users_endpoint()
    {
        _fixture.ResetAndSeedActivityGraph();
        using var http = NewClient();

        var usersResponse = await http.GetAsync("/api/v1/activity/dev/users", TestContext.Current.CancellationToken);
        var performersResponse = await http.GetAsync("/api/v1/activity/dev/performers", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, usersResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, performersResponse.StatusCode);

        var usersPayload = await usersResponse.Content.ReadFromJsonAsync<List<ActivityPerformerDto>>(TestContext.Current.CancellationToken);
        var performersPayload = await performersResponse.Content.ReadFromJsonAsync<List<ActivityPerformerDto>>(TestContext.Current.CancellationToken);

        Assert.NotNull(usersPayload);
        Assert.NotNull(performersPayload);
        Assert.Equal(usersPayload!.Count, performersPayload!.Count);
        Assert.Equal(usersPayload[0].PerformerId, performersPayload[0].PerformerId);
        Assert.Equal(usersPayload[0].UserName, performersPayload[0].UserName);
    }
}
