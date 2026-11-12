using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Abstract.Streaming;
using Yavsc.Api.Services;
using Yavsc.Api.Test.Fixtures;
using Yavsc.Models;

namespace Yavsc.Api.Test;

[Trait("Category", "non-reg")]
[Collection("Yavsc Api")]
public sealed class PodcastScheduleTests(ApiWebServerFixture fixture) : IClassFixture<ApiWebServerFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private HttpClient Client(string? user)
    {
        var client = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        }) { BaseAddress = new Uri(fixture.BaseAddress) };
        if (user != null) client.DefaultRequestHeaders.Authorization = new("Bearer", Yavsc.Tests.Shared.TestTokenIssuer.Issue(user, "api"));
        return client;
    }

    private async Task<string> OwnerAsync()
    {
        var id = "schedule-" + Guid.NewGuid().ToString("N");
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Users.Add(new ApplicationUser { Id = id, UserName = id, Email = id + "@example.test" });
        await db.SaveChangesAsync(Ct);
        return id;
    }

    private static PreparePodcastRequest Request(string? announcement = null) => new()
    {
        Title = "Concert prepare", ScheduledStartAt = DateTimeOffset.UtcNow.AddDays(7),
        PlannedDurationMinutes = 90, PublicAnnouncement = announcement
    };

    private async Task<PodcastResponse> PrepareAsync(HttpClient client, PreparePodcastRequest request)
    {
        using var response = await client.PostAsJsonAsync("/api/v1/podcasts/planned", request, Ct);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        return (await response.Content.ReadFromJsonAsync<PodcastResponse>(Ct))!;
    }

    [Fact]
    public async Task Plans_are_persisted_in_LiveFlow_private_editable_and_cancelable()
    {
        var ownerId = await OwnerAsync();
        using var owner = Client(ownerId);
        using var other = Client("other");
        using var anonymous = Client(null);
        var request = Request();
        var plan = await PrepareAsync(owner, request);
        var path = $"/api/v1/podcasts/{plan.Id}";
        Assert.Equal(PodcastState.Planned, plan.State);
        Assert.NotNull(plan.FlowId);
        Assert.Equal(request.ScheduledStartAt, plan.ScheduledStartAt);
        Assert.Equal(90, plan.PlannedDurationMinutes);
        using var obsStatus = await owner.GetAsync(path + "/obs", Ct);
        Assert.Equal(HttpStatusCode.OK, obsStatus.StatusCode);
        Assert.True(obsStatus.Headers.CacheControl?.NoStore);
        var status = await obsStatus.Content.ReadFromJsonAsync<PodcastObsStatus>(Ct);
        Assert.False(status!.Enabled);
        using var obsDenied = await other.GetAsync(path + "/obs", Ct);
        Assert.Equal(HttpStatusCode.Forbidden, obsDenied.StatusCode);
        using var obsAnonymous = await anonymous.GetAsync(path + "/obs", Ct);
        Assert.Equal(HttpStatusCode.Unauthorized, obsAnonymous.StatusCode);
        using (var scope = fixture.Services.CreateScope())
        {
            var flow = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().LiveFlow
                .AsNoTracking().SingleAsync(f => f.Id == plan.FlowId, Ct);
            Assert.Equal(plan.Id, flow.PodcastId);
            var reloaded = await scope.ServiceProvider.GetRequiredService<IPodcastScheduleService>().GetAsync(plan.Id, ownerId, Ct);
            Assert.Equal(plan, reloaded.Podcast);
        }
        var list = await other.GetFromJsonAsync<List<PodcastResponse>>("/api/v1/podcasts", Ct);
        Assert.DoesNotContain(list!, p => p.Id == plan.Id);
        using var missing = await other.GetAsync(path, Ct);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        using var denied = await other.PutAsJsonAsync(path + "/planned", Request("changed"), Ct);
        Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
        using var startDenied = await other.PostAsync(path + "/start", null, Ct);
        Assert.Equal(HttpStatusCode.Forbidden, startDenied.StatusCode);
        using var updated = await owner.PutAsJsonAsync(path + "/planned", request with { Title = "Nouveau titre" }, Ct);
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        Assert.Equal("Nouveau titre", (await owner.GetFromJsonAsync<PodcastManifestResponse>(path, Ct))?.Podcast.Title);
        using var streamDenied = await anonymous.GetAsync(path + "/index.m3u8", Ct);
        Assert.Equal(HttpStatusCode.Unauthorized, streamDenied.StatusCode);
        using var cancelled = await owner.DeleteAsync(path, Ct);
        Assert.Equal(HttpStatusCode.NoContent, cancelled.StatusCode);
        using var cancelledGet = await owner.GetAsync(path, Ct);
        Assert.Equal(HttpStatusCode.NotFound, cancelledGet.StatusCode);
    }

    [Fact]
    public async Task Public_announcements_have_only_presentation_data_and_can_be_unpublished()
    {
        using var owner = Client(await OwnerAsync());
        using var anonymous = Client(null);
        var privatePlan = await PrepareAsync(owner, Request());
        var publicPlan = await PrepareAsync(owner, Request("Venez au concert"));
        using var response = await anonymous.GetAsync("/api/v1/podcasts/announcements", Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<List<PodcastAnnouncementResponse>>(Ct);
        Assert.Contains(items!, p => p.Id == publicPlan.Id && p.Announcement == "Venez au concert");
        Assert.DoesNotContain(items!, p => p.Id == privatePlan.Id);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Ct));
        Assert.All(json.RootElement.EnumerateArray(), item =>
            Assert.Equal(new[] { "announcement", "id", "ownerName", "plannedDurationMinutes", "scheduledStartAt", "title" },
                item.EnumerateObject().Select(p => p.Name).Order().ToArray()));
        using var update = await owner.PutAsJsonAsync($"/api/v1/podcasts/{publicPlan.Id}/planned", Request(), Ct);
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.DoesNotContain((await anonymous.GetFromJsonAsync<List<PodcastAnnouncementResponse>>("/api/v1/podcasts/announcements", Ct))!,
            p => p.Id == publicPlan.Id);
        using (await owner.DeleteAsync($"/api/v1/podcasts/{privatePlan.Id}", Ct)) { }
        using (await owner.DeleteAsync($"/api/v1/podcasts/{publicPlan.Id}", Ct)) { }
    }

    [Fact]
    public async Task Manual_start_retains_id_and_schedule_is_idempotent_and_prevents_later_edits()
    {
        using var owner = Client(await OwnerAsync());
        var plan = await PrepareAsync(owner, Request("Annonce"));
        var path = $"/api/v1/podcasts/{plan.Id}";
        try
        {
            var responses = await Task.WhenAll(owner.PostAsync(path + "/start", null, Ct), owner.PostAsync(path + "/start", null, Ct));
            foreach (var response in responses)
            {
                using (response)
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    var live = await response.Content.ReadFromJsonAsync<PodcastResponse>(Ct);
                    Assert.Equal(plan.Id, live?.Id);
                    Assert.Equal(PodcastState.Live, live?.State);
                    Assert.Equal(plan.FlowId, live?.FlowId);
                    Assert.Equal(plan.ScheduledStartAt, live?.ScheduledStartAt);
                }
            }
            using var edit = await owner.PutAsJsonAsync(path + "/planned", Request(), Ct);
            Assert.Equal(HttpStatusCode.Conflict, edit.StatusCode);
            using var completed = await owner.PostAsync(path + "/complete", null, Ct);
            Assert.Equal(plan.FlowId, (await completed.Content.ReadFromJsonAsync<PodcastResponse>(Ct))?.FlowId);
            using var restart = await owner.PostAsync(path + "/start", null, Ct);
            Assert.Equal(HttpStatusCode.Conflict, restart.StatusCode);
        }
        finally
        {
            using (await owner.PostAsync(path + "/complete", null, Ct)) { }
            using (await owner.DeleteAsync(path, Ct)) { }
        }
    }

    [Theory]
    [InlineData(0, "Concert")]  
    [InlineData(-1, "Concert")]
    [InlineData(60, "   ")]
    public async Task Invalid_plans_are_rejected_without_creating_a_flow(int minutes, string title)
    {
        var id = await OwnerAsync();
        using var owner = Client(id);
        using var response = await owner.PostAsJsonAsync("/api/v1/podcasts/planned",
            Request() with { PlannedDurationMinutes = minutes, Title = title }, Ct);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var scope = fixture.Services.CreateScope();
        Assert.False(await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().LiveFlow.AnyAsync(f => f.OwnerId == id, Ct));
    }
}
