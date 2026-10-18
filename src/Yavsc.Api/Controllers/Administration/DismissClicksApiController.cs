using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Abstract.Models.Messaging;
using Yavsc.Models;
using Yavsc.Models.Messaging;
using Yavsc.Server.Helpers;

namespace Yavsc.ApiControllers.Administration;

/// <summary>
/// Notifications dismissed by the authenticated user.
/// </summary>
[Authorize]
[Produces("application/json")]
[Route(Constants.APIPrefix + "/" + Constants.NotificationsDismissPath)]
public sealed class DismissClicksApiController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public DismissClicksApiController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<DismissedNotification>>> GetDismissed()
    {
        var userId = User.GetUserId();
        var dismissed = await _dbContext.DismissClicked
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .OrderBy(d => d.NotificationId)
            .Select(d => new DismissedNotification { NotificationId = d.NotificationId })
            .ToListAsync();
        return Ok(dismissed);
    }

    /// <summary>
    /// Dismisses a notification for the current user. Idempotent.
    /// </summary>
    [HttpPost(Constants.NotificationsDismissClickRoute + "/{notificationId:long}")]
    public async Task<ActionResult<DismissedNotification>> Click(long notificationId)
    {
        var userId = User.GetUserId();
        if (!await _dbContext.Notification.AnyAsync(n => n.Id == notificationId))
            return NotFound();

        var exists = await _dbContext.DismissClicked
            .AnyAsync(d => d.UserId == userId && d.NotificationId == notificationId);
        if (!exists)
        {
            _dbContext.DismissClicked.Add(new DismissClicked { UserId = userId, NotificationId = notificationId });
            await _dbContext.SaveChangesAsync(userId);
        }

        return Ok(new DismissedNotification { NotificationId = notificationId });
    }

    /// <summary>
    /// Restores a previously dismissed notification for the current user.
    /// </summary>
    [HttpDelete(Constants.NotificationsDismissClickRoute + "/{notificationId:long}")]
    public async Task<IActionResult> Restore(long notificationId)
    {
        var userId = User.GetUserId();
        var dismissed = await _dbContext.DismissClicked
            .SingleOrDefaultAsync(d => d.UserId == userId && d.NotificationId == notificationId);
        if (dismissed is null)
            return NotFound();

        _dbContext.DismissClicked.Remove(dismissed);
        await _dbContext.SaveChangesAsync(userId);
        return NoContent();
    }
}
