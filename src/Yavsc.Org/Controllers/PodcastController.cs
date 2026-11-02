#nullable enable

using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Yavsc.Abstract.Streaming;
using Yavsc.Services;
using Yavsc.Models;
using Yavsc.ViewModels.Podcast;
using Yavsc.Web.Services;

namespace Yavsc.Controllers;

[Authorize(Policy = "WebUser")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class PodcastController(
    PodcastWebClient api,
    ApplicationDbContext db,
    ILogger<PodcastController> logger) : Controller
{
    [AllowAnonymous, HttpGet]
    public async Task<IActionResult> Announcements(CancellationToken ct)
    {
        using var response = await api.SendAsync(HttpMethod.Get, "announcements", null, ct, publicAnnouncement: true);
        var announcements = await response.Content.ReadFromJsonAsync<List<PodcastAnnouncementResponse>>(ct)
            ?? throw new JsonException("Empty podcast announcements response.");
        return View(announcements);
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var podcasts = await api.GetAsync<List<PodcastResponse>>("", ct);
        var ownerIds = podcasts.Select(p => p.OwnerId).Distinct().ToArray();
        var performers = await db.Performers.AsNoTracking()
            .Where(p => ownerIds.Contains(p.PerformerId) && p.Active && p.DonationPayPalEmail != null
                && p.DonationPayPalEmail != "")
            .Select(p => p.PerformerId).ToArrayAsync(ct);
        return View(new PodcastCatalogueViewModel(
            podcasts.OrderByDescending(p => p.CreatedAt).ToArray(), performers.ToHashSet(StringComparer.Ordinal)));
    }

    [HttpGet("Podcast/{id:guid}/index.m3u8")]
    public async Task<IActionResult> Playlist(Guid id, CancellationToken ct)
    {
        var manifest = await api.GetAsync<PodcastManifestResponse>(id.ToString(), ct);
        var text = new StringBuilder("#EXTM3U\n#EXT-X-VERSION:3\n#EXT-X-TARGETDURATION:15\n")
            .Append("#EXT-X-MEDIA-SEQUENCE:0\n#EXT-X-PLAYLIST-TYPE:EVENT\n");
        foreach (var segment in manifest.Segments)
        {
            text.Append(CultureInfo.InvariantCulture, $"#EXTINF:{segment.DurationSeconds:0.######},\n");
            text.Append(Url.Action(nameof(Segment), new { id, sequence = segment.Sequence })).Append('\n');
        }
        if (manifest.Podcast.State != PodcastState.Live) text.Append("#EXT-X-ENDLIST\n");
        return Content(text.ToString(), "application/vnd.apple.mpegurl", Encoding.UTF8);
    }

    [HttpGet("Podcast/{id:guid}/segments/{sequence:int:min(0)}.ts")]
    public Task<IActionResult> Segment(Guid id, int sequence, CancellationToken ct)
        => ProxyAsync($"{id}/segments/{sequence}.ts", "video/mp2t", ct);

    [HttpGet("Podcast/{id:guid}/replay.mp4")]
    [HttpHead("Podcast/{id:guid}/replay.mp4")]
    public Task<IActionResult> Replay(Guid id, CancellationToken ct)
        => ProxyAsync($"{id}/replay.mp4", "video/mp4", ct);

    private async Task<IActionResult> ProxyAsync(string path, string contentType, CancellationToken ct)
    {
        RangeHeaderValue? range = null;
        if (Request.Headers.TryGetValue("Range", out var value)
            && !RangeHeaderValue.TryParse(value.ToString(), out range))
            return BadRequest(new ProblemDetails { Status = 400, Title = "Plage de lecture invalide." });

        using var response = await api.SendAsync(HttpMethods.IsHead(Request.Method)
            ? HttpMethod.Head : HttpMethod.Get, path, range, ct);
        Response.StatusCode = (int)response.StatusCode;
        Response.ContentType = contentType;
        Response.Headers.CacheControl = "private, no-store";
        Response.ContentLength = response.Content.Headers.ContentLength;
        if (response.Content.Headers.ContentRange != null)
            Response.Headers.ContentRange = response.Content.Headers.ContentRange.ToString();
        if (response.Headers.AcceptRanges.Count > 0)
            Response.Headers.AcceptRanges = string.Join(", ", response.Headers.AcceptRanges);
        if (!HttpMethods.IsHead(Request.Method))
            await response.Content.CopyToAsync(Response.Body, ct);
        return new EmptyResult();
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();
        var error = executed.Exception;
        if (error == null || Response.HasStarted || HttpContext.RequestAborted.IsCancellationRequested)
            return;
        if (error is not (PodcastException or HttpRequestException or JsonException or TaskCanceledException))
            return;

        logger.LogError(error, "Podcast web request failed for {Action}", context.ActionDescriptor.DisplayName);
        var status = error is PodcastException podcastError ? podcastError.StatusCode : 502;
        var message = error is PodcastException known
            ? known.Message : "Le service de podcasts est indisponible. Reessayez plus tard.";
        Response.StatusCode = status;
        executed.ExceptionHandled = true;
        executed.Result = context.ActionDescriptor.RouteValues["action"] == nameof(Index)
            ? View("Index", new PodcastCatalogueViewModel([], new HashSet<string>(), message))
            : StatusCode(status, new ProblemDetails { Status = status, Title = message });
    }
}
