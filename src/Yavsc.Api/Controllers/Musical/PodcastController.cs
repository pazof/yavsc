#nullable enable

using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yavsc.Abstract.Streaming;
using Yavsc.Api.Middleware;
using Yavsc.Api.Services;
using Yavsc.Server.Helpers;
using Yavsc.Services;

namespace Yavsc.ApiControllers;

[ApiController]
[ApiExplorerSettings(GroupName = "podcasts")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Authorize(Policy = "ApiScope")]
[Route(Constants.APIPrefix + "/podcasts")]
[TypeFilter(typeof(PodcastExceptionFilter))]
public sealed class PodcastController(ILiveProcessor processor, IPodcastScheduleService schedule) : ControllerBase
{
    /// <summary>Lists live broadcasts and retained replays for signed-in listeners.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PodcastResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PodcastResponse>>> List(CancellationToken ct)
        => Ok(await schedule.ListAsync(User.GetUserId(), ct));

    /// <summary>Public announcements contain presentation details only, never the stream.</summary>
    [AllowAnonymous, HttpGet("announcements")]
    [ProducesResponseType<IReadOnlyList<PodcastAnnouncementResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Announcements(CancellationToken ct)
        => Ok(await schedule.AnnouncementsAsync(ct));

    /// <summary>Prepares a private emission; a nonempty announcement publishes its presentation.</summary>
    [HttpPost("planned")]
    [ProducesResponseType<PodcastResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Prepare(PreparePodcastRequest request, CancellationToken ct)
    {
        var response = await schedule.PrepareAsync(User.GetUserId(), request, null, ct);
        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }

    /// <summary>Edits an owned emission before its first start.</summary>
    [HttpPut("{id:guid}/planned")]
    [ProducesResponseType<PodcastResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, PreparePodcastRequest request, CancellationToken ct)
        => Ok(await schedule.PrepareAsync(User.GetUserId(), request, id, ct));

    /// <summary>Manually starts a prepared emission; retries retain the same broadcast identifier.</summary>
    [HttpPost("{id:guid}/start")]
    [ProducesResponseType<PodcastResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Start(Guid id, CancellationToken ct)
        => Ok(await schedule.StartAsync(id, User.GetUserId(), ct));

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
        => Ok(await schedule.GetAsync(id, User.GetUserId(), ct));

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
        await processor.AppendAsync(id, ownerId, sequence, duration, Request.Body, ct);
        return Ok((await schedule.GetAsync(id, ownerId, ct)).Podcast);
    }

    /// <summary>Ends the broadcast and schedules preparation of its MP4 replay.</summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType<PodcastResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PodcastResponse>> Complete(Guid id, CancellationToken ct)
    {
        var ownerId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(ownerId)) return Unauthorized();
        await processor.CompleteAsync(id, ownerId, ct);
        return Ok((await schedule.GetAsync(id, ownerId, ct)).Podcast);
    }

    /// <summary>Serves a completed MP4 replay with authenticated byte-range support.</summary>
    [HttpGet("{id:guid}/replay.mp4")]
    [HttpHead("{id:guid}/replay.mp4")]
    [Produces("video/mp4")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status206PartialContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Replay(Guid id, CancellationToken ct)
    {
        Response.Headers.CacheControl = "private, no-store";
        return File(await processor.OpenReplayAsync(id, ct), "video/mp4", enableRangeProcessing: true);
    }

    /// <summary>Retries failed MP4 preparation. Only the broadcast owner can retry.</summary>
    [HttpPost("{id:guid}/replay/retry")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> RetryReplay(Guid id, CancellationToken ct)
    {
        var ownerId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(ownerId)) return Unauthorized();
        await processor.RetryReplayAsync(id, ownerId, ct);
        return AcceptedAtAction(nameof(Get), new { id }, value: null);
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
        await schedule.DeleteAsync(id, ownerId, ct);
        return NoContent();
    }
}
