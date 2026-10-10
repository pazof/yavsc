using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Abstract.Models.Messaging;
using Yavsc.Models;
using Yavsc.Server.Helpers;

namespace Yavsc.ApiControllers.Administration;

[Produces("application/json")]
[Route(Constants.APIPrefix + "/" + Constants.NotificationsPath)]
public sealed class NotificationsApiController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public NotificationsApiController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [HttpGet(Constants.NotificationsCurrentUserRoute)]
    [AllowAnonymous]
    public async Task<ActionResult<List<Notification>>> GetCurrentUserNotifications()
    {
        var currentUserId = User.GetUserId();
        var isAdmin = User.IsInRole(Constants.AdminGroupName);

        var dismissedIds = string.IsNullOrWhiteSpace(currentUserId)
            ? new HashSet<long>()
            : await _dbContext.DismissClicked
                .AsNoTracking()
                .Where(d => d.UserId == currentUserId)
                .Select(d => d.NotificationId)
                .ToHashSetAsync();

        var notifications = await _dbContext.Notification
            .AsNoTracking()
            .Where(n => !dismissedIds.Contains(n.Id))
            .Where(n =>
                string.IsNullOrWhiteSpace(n.Target)
                || (n.Target != null && n.Target.StartsWith("pub/"))
                || (!string.IsNullOrWhiteSpace(currentUserId) && n.Target == $"user/{currentUserId}")
                || (isAdmin && n.Target == "administration"))
            .OrderByDescending(n => n.Id)
            .ToListAsync();

        return Ok(notifications);
    }
}
