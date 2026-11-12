using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yavsc.Abstract.Resources;
using Yavsc.Server.Helpers;

namespace Yavsc.ApiControllers.Administration;

[ApiController]
[ApiExplorerSettings(GroupName = Constants.Administration)]
[Authorize("AdministratorOnly")]
[ResourceUsageExempt]
[Route(Constants.APIPrefix + "/" + Constants.ResourceUsagePath + "/admin")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class ResourceUsageReliefApiController(
    IResourceUsageService usage, ILogger<ResourceUsageReliefApiController> logger) : ControllerBase
{
    [HttpPost("{userId}/erase")]
    [EndpointSummary("Effacer des points de consommation d’un utilisateur ayant atteint sa limite")]
    [EndpointDescription("Conserve les compteurs techniques, refuse les montants supérieurs à la consommation et les confirmations périmées, sans crédit futur.")]
    [ProducesResponseType<ResourceUsageSummary>(200, "application/json")]
    [ProducesResponseType<ProblemDetails>(400, "application/problem+json")]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType<ProblemDetails>(404, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(409, "application/problem+json")]
    public async Task<ActionResult<ResourceUsageSummary>> Erase(
        string userId, EraseResourceUsageRequest request, CancellationToken ct)
    {
        var actor = User.GetUserId();
        if (string.IsNullOrWhiteSpace(actor)) return Unauthorized();
        try { return Ok(await usage.EraseAsync(userId, actor, request, ct)); }
        catch (ResourceUsageAdjustmentException error)
        {
            logger.LogWarning("Point relief rejected for administrator {ActorUserId}, user {UserId}: {Reason}",
                actor, userId, error.Message);
            return Problem(statusCode: error.StatusCode, title: error.Message);
        }
    }
}
