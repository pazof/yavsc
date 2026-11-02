#nullable enable

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Security.Claims;
using IdentityServer8;
using Microsoft.Extensions.Options;
using Yavsc.Services;
using Yavsc.Server.Helpers;

namespace Yavsc.Web.Services;

public sealed class PodcastWebClient(HttpClient http, IOptions<SiteSettings> options,
    IdentityServerTools authority, IHttpContextAccessor contextAccessor)
{
    private string? _accessToken;

    public async Task<T> GetAsync<T>(string path, CancellationToken ct)
    {
        using var response = await SendAsync(HttpMethod.Get, path, null, ct);
        return await response.Content.ReadFromJsonAsync<T>(ct)
            ?? throw new JsonException("Empty podcast API response.");
    }

    public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path,
        RangeHeaderValue? range, CancellationToken ct, bool publicAnnouncement = false)
    {
        var settings = options.Value;
        if (!Uri.TryCreate(settings.ApiUrl, UriKind.Absolute, out var address)
            || address.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(address.UserInfo))
            throw new PodcastException(503, "La connexion aux podcasts n'est pas configuree.");

        if (publicAnnouncement && (method != HttpMethod.Get || path != "announcements"))
            throw new InvalidOperationException("Only podcast announcements may be requested anonymously.");
        if (!publicAnnouncement && _accessToken == null)
        {
            var user = contextAccessor.HttpContext?.User;
            var userId = user?.GetUserId();
            if (user?.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(userId))
                throw new PodcastException(401, "Une connexion est requise pour lire les podcasts.");
            _accessToken = await authority.IssueJwtAsync(300, new List<Claim>
            {
                new("sub", userId),
                new("name", user.Identity.Name ?? userId),
                new("scope", "api"),
                new("aud", "api")
            });
            if (string.IsNullOrWhiteSpace(_accessToken))
                throw new InvalidOperationException("Yavsc.Org did not issue a podcast access token.");
        }

        address = new Uri(settings.ApiUrl.TrimEnd('/') + "/");
        if (!address.AbsolutePath.EndsWith("/" + Constants.APIPrefix + "/", StringComparison.Ordinal))
            address = new Uri(address, Constants.APIPrefix + "/");
        using var request = new HttpRequestMessage(method, new Uri(address, "podcasts/" + path));
        if (!publicAnnouncement)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        request.Headers.Range = range;
        var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.RequestedRangeNotSatisfiable)
        {
            var status = response.StatusCode;
            response.Dispose();
            throw new PodcastException(status switch
            {
                HttpStatusCode.NotFound => 404,
                HttpStatusCode.Conflict => 409,
                _ => 502
            }, status switch
            {
                HttpStatusCode.NotFound => "Podcast introuvable.",
                HttpStatusCode.Conflict => "Ce replay n'est pas encore disponible.",
                _ => "Le service de podcasts est indisponible."
            });
        }
        return response;
    }
}
