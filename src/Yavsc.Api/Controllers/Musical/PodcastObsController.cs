using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yavsc.Abstract.Streaming;
using Yavsc.Api.Middleware;
using Yavsc.Api.Services;
using Yavsc.Server.Helpers;

namespace Yavsc.ApiControllers;

[ApiController, ApiExplorerSettings(GroupName = "podcasts")]
[Authorize(Policy = "ApiScope")]
[Route(Constants.APIPrefix + "/podcasts")]
[TypeFilter(typeof(PodcastExceptionFilter))]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class PodcastObsController(IPodcastObsService obs) : ControllerBase
{
    [HttpPost("{id:guid}/obs"), EndpointSummary("Activer OBS pour une emission programmee et obtenir une cle a usage limite")]
    [ProducesResponseType<PodcastObsCredentials>(200)]
    [ProducesResponseType<ProblemDetails>(403)]
    [ProducesResponseType<ProblemDetails>(404)]
    [ProducesResponseType<ProblemDetails>(409)]
    [ProducesResponseType<ProblemDetails>(429)]
    [ProducesResponseType<ProblemDetails>(413)]
    [ProducesResponseType<ProblemDetails>(503)]
    public async Task<ActionResult<PodcastObsCredentials>> Activate(Guid id, CancellationToken ct)
        => Ok(await obs.ActivateAsync(id, User.GetUserId(), ct));

    [HttpGet("{id:guid}/obs"), EndpointSummary("Consulter l'activation OBS et les erreurs de reception, sans exposer la cle")]
    [ProducesResponseType<PodcastObsStatus>(200)]
    [ProducesResponseType<ProblemDetails>(403)]
    [ProducesResponseType<ProblemDetails>(404)]
    public async Task<ActionResult<PodcastObsStatus>> Status(Guid id, CancellationToken ct)
        => Ok(await obs.StatusAsync(id, User.GetUserId(), ct));

    [HttpDelete("{id:guid}/obs"), EndpointSummary("Revoquer la cle OBS et deconnecter le diffuseur")]
    [ProducesResponseType(204)]
    [ProducesResponseType<ProblemDetails>(403)]
    [ProducesResponseType<ProblemDetails>(404)]
    [ProducesResponseType<ProblemDetails>(503)]
    [ResourceUsageRelief]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await obs.DeactivateAsync(id, User.GetUserId(), ct);
        return NoContent();
    }

}
