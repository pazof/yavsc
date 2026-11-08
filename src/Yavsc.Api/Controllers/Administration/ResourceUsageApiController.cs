using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yavsc.Abstract.Resources;
using Yavsc.Server.Helpers;

namespace Yavsc.ApiControllers.Administration;

[Produces("application/json")]
[Authorize]
[ResourceUsageExempt]
[Route(Constants.APIPrefix + "/" + Constants.ResourceUsagePath)]
public sealed class ResourceUsageApiController(IResourceUsageService usage) : Controller
{
    [HttpGet(Constants.ResourceUsageCurrentUserRoute)]
    public async Task<ActionResult<ResourceUsageSummary>> GetCurrentUserUsage(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        return Ok(await usage.GetCurrentAsync(userId,
            User.Identity?.Name ?? Constants.ResourceUsageCurrentUserDisplayName, cancellationToken));
    }

    [HttpGet(Constants.ResourceUsageAdminRoute)]
    public async Task<ActionResult<List<ResourceUsageSummary>>> GetAdminOverview(CancellationToken cancellationToken)
    {
        if (!User.IsInRole(Constants.AdminName)) return Forbid();
        return Ok(await usage.GetOverviewAsync(cancellationToken));
    }
}
