#nullable enable

using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yavsc.Abstract.Streaming;
using Yavsc.Api.Middleware;
using Yavsc.Server.Helpers;
using Yavsc.Services;

namespace Yavsc.ApiControllers;

[ApiController]
[ApiExplorerSettings(GroupName = "podcasts")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Authorize(Policy = "ApiScope")]
[Route(Constants.APIPrefix + "/podcasts")]
[TypeFilter(typeof(PodcastExceptionFilter))]
public sealed class PodcastController(ILiveProcessor processor) : ControllerBase
{
    /// <summary>Lists live broadcasts and retained replays for signed-in listeners.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PodcastResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PodcastResponse>>> List(CancellationToken ct)
        => Ok(await processor.ListAsync(ct));

    /// <summary>Starts a broadcast owned by the current user.</summary>
    [HttpPost]
    [ProducesResponseType<PodcastResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PodcastResponse>> Create(CreatePodcastRequest request, CancellationToken ct)
    {
        var ownerId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(ownerId)) return Unauthorized();
        var response = await processor.CreateAsync(ownerId, User.Identity?.Name ?? ownerId, request.Title, ct);
        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }

    /// <summary>Returns metadata and ordered committed segments.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<PodcastManifestResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PodcastManifestResponse>> Get(Guid id, CancellationToken ct)
        => Ok(await processor.GetAsync(id, ct));

    /// <summary>Commits an MPEG-TS segment. Identical retries are idempotent.</summary>
    [HttpPut("{id:guid}/segments/{sequence:int}")]
    [Consumes("video/mp2t")]
    [RequestSizeLimit(8 * 1024 * 1024)]
    [ProducesResponseType<PodcastResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status413PayloadTooLarge)]
    public async Task<ActionResult<PodcastResponse>> Append(Guid id, int sequence,
        [FromQuery] double duration, CancellationToken ct)
    {
        var ownerId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(ownerId)) return Unauthorized();
        return Ok(await processor.AppendAsync(id, ownerId, sequence, duration, Request.Body, ct));
    }

    /// <summary>Ends the broadcast and publishes its retained HLS replay.</summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType<PodcastResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PodcastResponse>> Complete(Guid id, CancellationToken ct)
    {
        var ownerId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(ownerId)) return Unauthorized();
        return Ok(await processor.CompleteAsync(id, ownerId, ct));
    }

    /// <summary>Serves the authenticated event playlist, terminated for completed or interrupted replays.</summary>
    [HttpGet("{id:guid}/index.m3u8")]
    [Produces("application/vnd.apple.mpegurl")]
    public async Task<IActionResult> Playlist(Guid id, CancellationToken ct)
    {
        var manifest = await processor.GetAsync(id, ct);
        Response.Headers.CacheControl = "no-store";
        var text = new StringBuilder("#EXTM3U\n#EXT-X-VERSION:3\n#EXT-X-TARGETDURATION:15\n");
        text.Append("#EXT-X-MEDIA-SEQUENCE:0\n#EXT-X-PLAYLIST-TYPE:EVENT\n");
        foreach (var segment in manifest.Segments)
        {
            text.Append(System.Globalization.CultureInfo.InvariantCulture, $"#EXTINF:{segment.DurationSeconds:0.######},\n");
            text.Append(System.Globalization.CultureInfo.InvariantCulture, $"segments/{segment.Sequence}.ts\n");
        }
        if (manifest.Podcast.State != PodcastState.Live) text.Append("#EXT-X-ENDLIST\n");
        return Content(text.ToString(), "application/vnd.apple.mpegurl", Encoding.UTF8);
    }

    /// <summary>Reads one immutable committed segment. Authentication is required on every request.</summary>
    [HttpGet("{id:guid}/segments/{sequence:int}.ts")]
    [Produces("video/mp2t")]
    public async Task<IActionResult> Segment(Guid id, int sequence, CancellationToken ct)
    {
        Response.Headers.CacheControl = "private, no-store";
        return File(await processor.OpenSegmentAsync(id, sequence, ct), "video/mp2t");
    }

    /// <summary>Deletes an ended broadcast and releases its owner's podcast quota.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var ownerId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(ownerId)) return Unauthorized();
        await processor.DeleteAsync(id, ownerId, ct);
        return NoContent();
    }
}
