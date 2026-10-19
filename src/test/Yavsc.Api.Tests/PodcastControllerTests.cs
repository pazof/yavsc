using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Yavsc.Abstract.Streaming;
using Yavsc.Api.Test.Fixtures;
using Yavsc.Tests.Shared;

namespace Yavsc.Api.Test;

[Collection("Yavsc Api")]
public sealed class PodcastControllerTests(ApiWebServerFixture fixture) : IClassFixture<ApiWebServerFixture>
{
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    private HttpClient Client(string? user = "alice", string scope = "api")
    {
        var client = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        }) { BaseAddress = new Uri(fixture.BaseAddress) };
        if (user is not null)
            client.DefaultRequestHeaders.Authorization = new("Bearer", TestTokenIssuer.Issue(user, scope));
        return client;
    }

    [Theory]
    [InlineData("/api/v1/podcasts")]
    [InlineData("/api/v1/podcasts/00000000-0000-0000-0000-000000000000")]
    [InlineData("/api/v1/podcasts/00000000-0000-0000-0000-000000000000/index.m3u8")]
    [InlineData("/api/v1/podcasts/00000000-0000-0000-0000-000000000000/segments/0.ts")]
    public async Task Metadata_playlists_and_segments_are_never_anonymous(string path)
    {
        using var anonymous = Client(null);
        using var response = await anonymous.GetAsync(path, _ct);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        using var wrongScope = Client("bob", "profile");
        using var forbidden = await wrongScope.GetAsync(path, _ct);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }

    [Fact]
    public async Task Live_ingestion_playback_completion_and_deletion_work_over_http()
    {
        using var owner = Client($"podcast-owner-{Guid.NewGuid():N}");
        using var listener = Client("podcast-listener");
        using var created = await owner.PostAsJsonAsync("/api/v1/podcasts", new CreatePodcastRequest { Title = "Concert" }, _ct);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var podcast = await created.Content.ReadFromJsonAsync<PodcastResponse>(_ct);
        Assert.NotNull(podcast);
        Assert.NotNull(created.Headers.Location);
        var path = $"/api/v1/podcasts/{podcast.Id}";
        using var content = new ByteArrayContent(LiveProcessorTests.Packets());
        content.Headers.ContentType = new MediaTypeHeaderValue("video/mp2t");
        using var appended = await owner.PutAsync($"{path}/segments/0?duration=2.5", content, _ct);
        Assert.Equal(HttpStatusCode.OK, appended.StatusCode);
        var live = await listener.GetStringAsync($"{path}/index.m3u8", _ct);
        Assert.StartsWith("#EXTM3U", live);
        Assert.Contains("#EXTINF:2.5,\nsegments/0.ts", live);
        Assert.DoesNotContain("#EXT-X-ENDLIST", live);
        Assert.Equal(LiveProcessorTests.Packets(), await listener.GetByteArrayAsync($"{path}/segments/0.ts", _ct));
        using var denied = await listener.PostAsync($"{path}/complete", null, _ct);
        Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
        using var completed = await owner.PostAsync($"{path}/complete", null, _ct);
        Assert.Equal(HttpStatusCode.OK, completed.StatusCode);
        var replay = await listener.GetStringAsync($"{path}/index.m3u8", _ct);
        Assert.Contains("#EXT-X-ENDLIST", replay);
        using var deleteDenied = await listener.DeleteAsync(path, _ct);
        Assert.Equal(HttpStatusCode.Forbidden, deleteDenied.StatusCode);
        using var deleted = await owner.DeleteAsync(path, _ct);
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        using var missing = await listener.GetAsync(path, _ct);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task Invalid_title_is_a_validation_problem()
    {
        using var owner = Client();
        using var response = await owner.PostAsJsonAsync("/api/v1/podcasts", new { title = " " }, _ct);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("application/problem+json", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task OpenApi_exposes_podcast_routes_without_breaking_document_generation()
    {
        using var admin = Client("podcast-admin");
        admin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            TestTokenIssuer.Issue("podcast-admin", "api",
                extraClaims: new[] { new Claim(Constants.RoleClaimType, Constants.AdminGroupName) }));
        using var response = await admin.GetAsync("/openapi/podcasts.json", _ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(_ct));
        var paths = document.RootElement.GetProperty("paths");
        Assert.True(paths.TryGetProperty("/api/v1/podcasts", out _));
        Assert.True(paths.TryGetProperty("/api/v1/podcasts/{id}/segments/{sequence}", out _));
        Assert.True(paths.TryGetProperty("/api/v1/podcasts/{id}/index.m3u8", out _));
    }
}
