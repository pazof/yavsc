using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Yavsc.Abstract.Streaming;
using Yavsc.Api.Test.Fixtures;
using Yavsc.Tests.Shared;

namespace Yavsc.Api.Test;

[Trait("Category", "non-reg")]
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
    [InlineData("/api/v1/podcasts/00000000-0000-0000-0000-000000000000/replay.mp4")]
    [InlineData("/api/v1/podcasts/00000000-0000-0000-0000-000000000000/replay/retry")]
    public async Task Metadata_playlists_and_segments_are_never_anonymous(string path)
    {
        using var anonymous = Client(null);
        var method = path.EndsWith("/retry", StringComparison.Ordinal) ? HttpMethod.Post : HttpMethod.Get;
        using var response = await anonymous.SendAsync(new HttpRequestMessage(method, path), _ct);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        using var wrongScope = Client("bob", "profile");
        using var forbidden = await wrongScope.SendAsync(new HttpRequestMessage(method, path), _ct);
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
        await WaitReplayAsync(owner, path, PodcastReplayState.Failed);
        using var retryDenied = await listener.PostAsync($"{path}/replay/retry", null, _ct);
        Assert.Equal(HttpStatusCode.Forbidden, retryDenied.StatusCode);
        using var retry = await owner.PostAsync($"{path}/replay/retry", null, _ct);
        Assert.Equal(HttpStatusCode.Accepted, retry.StatusCode);
        Assert.NotNull(retry.Headers.Location);
        await WaitReplayAsync(owner, path, PodcastReplayState.Failed);
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
    public async Task Worker_publishes_authenticated_seekable_MP4_with_GET_HEAD_and_416_ranges()
    {
        using var owner = Client($"mp4-owner-{Guid.NewGuid():N}");
        using var listener = Client("mp4-listener");
        var directory = Path.Combine(Path.GetTempPath(), $"yavsc-http-replay-{Guid.NewGuid():N}");
        string? path = null;
        try
        {
            var files = await PodcastMediaFixture.CreateAsync(directory, _ct);
            using var created = await owner.PostAsJsonAsync("/api/v1/podcasts", new CreatePodcastRequest { Title = "Synthetic MP4" }, _ct);
            var podcast = await created.Content.ReadFromJsonAsync<PodcastResponse>(_ct);
            Assert.NotNull(podcast);
            path = $"/api/v1/podcasts/{podcast.Id}";
            using var unavailable = await listener.GetAsync($"{path}/replay.mp4", _ct);
            Assert.Equal(HttpStatusCode.Conflict, unavailable.StatusCode);
            for (var i = 0; i < files.Length; i++)
            {
                using var content = new ByteArrayContent(await File.ReadAllBytesAsync(files[i], _ct));
                content.Headers.ContentType = new MediaTypeHeaderValue("video/mp2t");
                using var appended = await owner.PutAsync($"{path}/segments/{i}?duration=1", content, _ct);
                appended.EnsureSuccessStatusCode();
            }
            using var completed = await owner.PostAsync($"{path}/complete", null, _ct);
            completed.EnsureSuccessStatusCode();
            var ready = await WaitReplayAsync(owner, path, PodcastReplayState.Ready);
            using var full = await listener.GetAsync($"{path}/replay.mp4", _ct);
            Assert.Equal(HttpStatusCode.OK, full.StatusCode);
            Assert.Equal("video/mp4", full.Content.Headers.ContentType?.MediaType);
            Assert.True(full.Headers.CacheControl?.NoStore);
            Assert.Contains("bytes", full.Headers.AcceptRanges);
            var bytes = await full.Content.ReadAsByteArrayAsync(_ct);
            Assert.Equal(ready.Replay!.Bytes, bytes.LongLength);
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{path}/replay.mp4");
            request.Headers.Range = new RangeHeaderValue(7, 25);
            using var range = await listener.SendAsync(request, _ct);
            Assert.Equal(HttpStatusCode.PartialContent, range.StatusCode);
            Assert.Equal($"bytes 7-25/{bytes.Length}", range.Content.Headers.ContentRange?.ToString());
            Assert.Equal(bytes[7..26], await range.Content.ReadAsByteArrayAsync(_ct));
            using var head = await listener.SendAsync(new HttpRequestMessage(HttpMethod.Head, $"{path}/replay.mp4"), _ct);
            Assert.Equal(HttpStatusCode.OK, head.StatusCode);
            Assert.Equal(bytes.Length, head.Content.Headers.ContentLength);
            Assert.Empty(await head.Content.ReadAsByteArrayAsync(_ct));
            using var beyond = new HttpRequestMessage(HttpMethod.Get, $"{path}/replay.mp4");
            beyond.Headers.Range = new RangeHeaderValue(bytes.Length, null);
            using var invalid = await listener.SendAsync(beyond, _ct);
            Assert.Equal(HttpStatusCode.RequestedRangeNotSatisfiable, invalid.StatusCode);
            Assert.Equal(bytes.Length, invalid.Content.Headers.ContentRange?.Length);
        }
        finally
        {
            if (path is not null) using (await owner.DeleteAsync(path, _ct)) { }
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    private async Task<PodcastResponse> WaitReplayAsync(HttpClient client, string path, PodcastReplayState expected)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(_ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));
        while (true)
        {
            var manifest = await client.GetFromJsonAsync<PodcastManifestResponse>(path, timeout.Token);
            Assert.NotNull(manifest);
            if (manifest.Podcast.Replay?.State == expected) return manifest.Podcast;
            if (expected == PodcastReplayState.Ready)
                Assert.NotEqual(PodcastReplayState.Failed, manifest.Podcast.Replay?.State);
            await Task.Delay(100, timeout.Token);
        }
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
                extraClaims: new[] { new Claim(Constants.RoleClaimType, Constants.AdminName) }));
        using var response = await admin.GetAsync("/openapi/podcasts.json", _ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(_ct));
        var paths = document.RootElement.GetProperty("paths");
        Assert.True(paths.TryGetProperty("/api/v1/podcasts", out _));
        Assert.True(paths.TryGetProperty("/api/v1/podcasts/{id}/segments/{sequence}", out _));
        Assert.True(paths.TryGetProperty("/api/v1/podcasts/{id}/index.m3u8", out _));
        Assert.True(paths.TryGetProperty("/api/v1/podcasts/{id}/replay.mp4", out var replay));
        Assert.True(replay.TryGetProperty("get", out _));
        Assert.True(replay.TryGetProperty("head", out _));
        Assert.True(paths.TryGetProperty("/api/v1/podcasts/{id}/replay/retry", out _));
    }
}
