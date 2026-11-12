using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using IdentityServer8;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Yavsc.Abstract.Streaming;
using Yavsc.Models;
using Yavsc.Models.Workflow;
using Yavsc.Models.Relationship;
using Yavsc.Web.Services;
using Xunit;

namespace Yavsc.Org.Tests.Controllers;

[Trait("Category", "non-reg")]
public sealed class PodcastWebTests : IClassFixture<PodcastWebTests.Factory>
{
    private readonly Factory _factory;
    private static readonly Guid PodcastId = Guid.Parse("bac67914-b844-4a15-94bf-49d82b0b0212");
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public PodcastWebTests(Factory factory)
    {
        _factory = factory;
        factory.Api.Reset();
        factory.Services.GetRequiredService<IOptions<SiteSettings>>().Value.ApiUrl = "https://api.example/";
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (!db.Users.Any(u => u.Id == "author"))
        {
            db.Users.AddRange(new ApplicationUser { Id = "author", UserName = "Author", Email = "author@example.test" },
                new ApplicationUser { Id = "listener", UserName = "Listener", Email = "listener@example.test" });
        }
        if (!db.Performers.Any(p => p.PerformerId == "author"))
        {
            db.Performers.Add(new PerformerProfile
            {
                PerformerId = "author", Active = true, DonationPayPalEmail = "author@payments.example",
                SIREN = "123456789", WebSite = "", OrganizationAddress = new Location { Address = "Test" }
            });
        }
        db.SaveChanges();
    }

    [Theory]
    [InlineData("/Podcast")]
    [InlineData("/Podcast/bac67914-b844-4a15-94bf-49d82b0b0212/index.m3u8")]
    [InlineData("/Podcast/bac67914-b844-4a15-94bf-49d82b0b0212/segments/0.ts")]
    [InlineData("/Podcast/bac67914-b844-4a15-94bf-49d82b0b0212/replay.mp4")]
    public async Task Anonymous_requests_require_login_before_contacting_api(string path)
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        using var response = await client.GetAsync(path, Ct);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/signin", response.Headers.Location?.ToString());
        Assert.Empty(_factory.Api.Requests);
    }

    [Fact]
    public async Task Announcements_are_public_escaped_and_requested_without_a_user_token()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        using var response = await client.GetAsync("/Podcast/Announcements", Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync(Ct);
        Assert.Contains("&lt;script&gt;public&lt;/script&gt;", html);
        Assert.DoesNotContain("<script>public</script>", html);
        Assert.DoesNotContain("data-podcast-player", html);
        Assert.DoesNotContain("replay.mp4", html);
        var request = Assert.Single(_factory.Api.Requests);
        Assert.Equal("/api/v1/podcasts/announcements", request.Path);
        Assert.Null(request.Authorization);
    }

    [Fact]
    public async Task Catalogue_uses_the_authority_user_token_without_an_api_client_or_external_donation_link()
    {
        using var client = SignedInClient();
        using var response = await client.GetAsync("/Podcast", Ct);
        var html = await response.Content.ReadAsStringAsync(Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($"/Podcast/{PodcastId}/replay.mp4", html);
        Assert.Contains("/Donation/Create/author", html);
        Assert.Contains("/Donation/Settings", html);
        Assert.DoesNotContain("<script>unsafe-title</script>", html);
        Assert.Contains("&lt;script&gt;unsafe-title&lt;/script&gt;", html);
        Assert.Contains("hls.light.min.js", html);
        Assert.True(response.Headers.CacheControl?.NoStore);
        var requests = _factory.Api.Requests.ToArray();
        var request = Assert.Single(requests);
        Assert.Equal("/api/v1/podcasts/", request.Path);
        Assert.StartsWith("Bearer ", request.Authorization);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(request.Authorization![7..]);
        Assert.Contains(token.Claims, c => c.Type == "sub" && c.Value == "listener");
        Assert.Contains(token.Claims, c => c.Type == "scope" && c.Value == "api");
        Assert.Contains("api", token.Audiences);
        Assert.True(token.ValidTo - token.ValidFrom <= TimeSpan.FromMinutes(5));
        Assert.DoesNotContain(request.Authorization[7..], html);
    }

    [Fact]
    public async Task Empty_catalogue_is_not_an_api_failure()
    {
        _factory.Api.Empty = true;
        using var client = SignedInClient();
        using var response = await client.GetAsync("/Podcast", Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Aucun podcast", await response.Content.ReadAsStringAsync(Ct));
    }

    [Fact]
    public async Task Upstream_failure_returns_explicit_error_not_empty_catalogue()
    {
        _factory.Api.Status = HttpStatusCode.ServiceUnavailable;
        using var client = SignedInClient();
        using var response = await client.GetAsync("/Podcast", Ct);
        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync(Ct);
        Assert.Contains("alert-danger", html);
        Assert.DoesNotContain("Aucun podcast", html);
    }

    [Fact]
    public async Task Playlist_uses_authenticated_same_origin_segment_routes()
    {
        using var client = SignedInClient();
        using var response = await client.GetAsync($"/Podcast/{PodcastId}/index.m3u8", Ct);
        var playlist = await response.Content.ReadAsStringAsync(Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/vnd.apple.mpegurl", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("#EXTINF:2.5,\n", playlist);
        Assert.Contains($"/Podcast/{PodcastId}/segments/0.ts", playlist);
        Assert.EndsWith("#EXT-X-ENDLIST\n", playlist);
        Assert.DoesNotContain("access_token", playlist);
    }

    [Fact]
    public async Task Live_catalogue_without_donation_keeps_playlist_open()
    {
        _factory.Api.Live = true;
        using var client = SignedInClient();
        var html = await client.GetStringAsync("/Podcast", Ct);
        Assert.Contains($"/Podcast/{PodcastId}/index.m3u8", html);
        Assert.DoesNotContain("/Donation/Create/other-author", html);
        Assert.Contains("PayPal", html);
        var playlist = await client.GetStringAsync($"/Podcast/{PodcastId}/index.m3u8", Ct);
        Assert.DoesNotContain("#EXT-X-ENDLIST", playlist);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("HEAD")]
    public async Task Replay_preserves_byte_range_status_headers_and_head_semantics(string method)
    {
        using var client = SignedInClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), $"/Podcast/{PodcastId}/replay.mp4");
        request.Headers.Range = new RangeHeaderValue(2, 5);
        using var response = await client.SendAsync(request, Ct);
        Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
        Assert.Equal("bytes 2-5/10", response.Content.Headers.ContentRange?.ToString());
        Assert.Equal(4, response.Content.Headers.ContentLength);
        Assert.Contains("bytes", response.Headers.AcceptRanges);
        Assert.Equal("video/mp4", response.Content.Headers.ContentType?.MediaType);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.Equal(method == "HEAD" ? [] : new byte[] { 2, 3, 4, 5 },
            await response.Content.ReadAsByteArrayAsync(Ct));
        var forwarded = _factory.Api.Requests.Last();
        Assert.Equal(method, forwarded.Method);
        Assert.Equal("bytes=2-5", forwarded.Range);
    }

    [Fact]
    public async Task Unsatisfiable_replay_range_preserves_416_and_total_size()
    {
        using var client = SignedInClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/Podcast/{PodcastId}/replay.mp4");
        request.Headers.Range = new RangeHeaderValue(100, 200);
        using var response = await client.SendAsync(request, Ct);
        Assert.Equal(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
        Assert.Equal("bytes */10", response.Content.Headers.ContentRange?.ToString());
    }

    [Fact]
    public async Task Missing_media_returns_404()
    {
        _factory.Api.Status = HttpStatusCode.NotFound;
        using var client = SignedInClient();
        using var response = await client.GetAsync($"/Podcast/{PodcastId}/segments/0.ts", Ct);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Local_hls_asset_is_served_without_a_cdn()
    {
        using var client = SignedInClient();
        using var response = await client.GetAsync("/js/hls.light.min.js", Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Hls", await response.Content.ReadAsStringAsync(Ct));
    }

    [Theory]
    [InlineData("https://api.example/")]
    [InlineData("https://api.example/api/v1")]
    public async Task Api_base_url_preserves_the_existing_versioned_prefix(string address)
    {
        _factory.Services.GetRequiredService<IOptions<SiteSettings>>().Value.ApiUrl = address;
        using var client = SignedInClient();
        using var response = await client.GetAsync("/Podcast", Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("/api/v1/podcasts/", _factory.Api.Requests.Last().Path);
    }

    [Theory]
    [InlineData("")]
    [InlineData("http://api.example/")]
    public async Task Invalid_api_configuration_returns_503_without_network_access(string address)
    {
        _factory.Services.GetRequiredService<IOptions<SiteSettings>>().Value.ApiUrl = address;
        using var client = SignedInClient();
        using var response = await client.GetAsync("/Podcast", Ct);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Empty(_factory.Api.Requests);
    }

    private HttpClient SignedInClient() => _factory.SignedInClient();

    public class Factory : TestWebApplicationFactory
    {
        public FakeApi Api { get; } = new();
        private readonly string _databaseName = "InMemory-PodcastWeb-" + Guid.NewGuid().ToString("N");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
                new Dictionary<string, string?> { ["ConnectionStrings:YavscConnection"] = _databaseName }));
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>();
                services.Configure<SiteSettings>(options =>
                {
                    options.ApiUrl = "https://api.example/";
                    options.ExternalUrl = "https://org.example/";
                });
                services.AddTransient(provider => new PodcastWebClient(
                    new HttpClient(Api, disposeHandler: false),
                    provider.GetRequiredService<IOptions<SiteSettings>>(),
                    provider.GetRequiredService<IdentityServerTools>(),
                    provider.GetRequiredService<IHttpContextAccessor>()));
            });
        }

        public HttpClient SignedInClient(string userId = "listener")
        {
            var client = CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            var options = Services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
                .Get(IdentityConstants.ApplicationScheme);
            var principal = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userId)
            ], IdentityConstants.ApplicationScheme));
            var ticket = new AuthenticationTicket(principal, new AuthenticationProperties
            {
                IssuedUtc = DateTimeOffset.UtcNow,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(5)
            }, IdentityConstants.ApplicationScheme);
            client.DefaultRequestHeaders.Add("Cookie", $"{options.Cookie.Name}={options.TicketDataFormat.Protect(ticket)}");
            return client;
        }
    }

    public sealed class FakeApi : HttpMessageHandler
    {
        public sealed record Request(string Path, string Method, string? Authorization, string? Range);
        public ConcurrentQueue<Request> Requests { get; } = new();
        public bool Empty { get; set; }
        public bool Live { get; set; }
        public HttpStatusCode Status { get; set; }

        public void Reset()
        {
            Requests.Clear();
            Empty = false;
            Live = false;
            Status = HttpStatusCode.OK;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var path = request.RequestUri!.AbsolutePath;
            Requests.Enqueue(new(path, request.Method.Method,
                request.Headers.Authorization?.ToString(), request.Headers.Range?.ToString()));
            if (Status != HttpStatusCode.OK)
                return Task.FromResult(new HttpResponseMessage(Status));
            if (path == "/api/v1/podcasts/announcements")
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new[] { new PodcastAnnouncementResponse(
                        PodcastId, "Concert", "Author", DateTimeOffset.UtcNow.AddDays(1), 60, "<script>public</script>") })
                });
            var podcast = new PodcastResponse(PodcastId, "<script>unsafe-title</script>",
                Live ? "other-author" : "author", "Author", DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow, Live ? PodcastState.Live : PodcastState.Completed, 1, 188, 2.5)
            {
                Replay = Live ? null : new(PodcastReplayState.Ready, 10)
            };
            if (path == "/api/v1/podcasts/")
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(Empty ? Array.Empty<PodcastResponse>() : [podcast])
                });
            if (path == $"/api/v1/podcasts/{PodcastId}")
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new PodcastManifestResponse(podcast, [new(0, 2.5, 188)]))
                });
            var range = request.Headers.Range?.Ranges.SingleOrDefault();
            var unsatisfiable = range?.From >= 10;
            var content = new ByteArrayContent(unsatisfiable ? [] : [2, 3, 4, 5]);
            if (range != null)
                content.Headers.ContentRange = unsatisfiable
                    ? new ContentRangeHeaderValue(10) : new ContentRangeHeaderValue(2, 5, 10);
            var result = new HttpResponseMessage(unsatisfiable ? HttpStatusCode.RequestedRangeNotSatisfiable
                : range != null ? HttpStatusCode.PartialContent : HttpStatusCode.OK) { Content = content };
            result.Headers.AcceptRanges.Add("bytes");
            return Task.FromResult(result);
        }
    }
}
