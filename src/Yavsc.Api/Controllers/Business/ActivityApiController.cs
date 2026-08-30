using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Abstract.Workflow;
using Yavsc.Server.Helpers;
using Yavsc.Models;
using Yavsc.Models.Workflow;


namespace Yavsc.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route(Constants.APIPrefix + "/activity")]
    public class ActivityApiController : Controller
    {
        private ApplicationDbContext _context;

        public ActivityApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ActivityApi
        [HttpGet]
        public IEnumerable<Activity> GetActivities()
        {
            return _context.Activities.Include(a=>a.Forms).Where( a => !a.Hidden );
        }

        [HttpGet("catalog")]
        public async Task<ActionResult<IEnumerable<ActivityBrowseItemDto>>> GetCatalog(
            CancellationToken cancellationToken,
            [FromQuery] string parentCode = null)
        {
            var activities = await _context.Activities
                .AsNoTracking()
                .Include(a => a.Forms)
                .Include(a => a.Children)
                .ThenInclude(c => c.Forms)
                .Where(a => !a.Hidden && a.ParentCode == parentCode)
                .OrderByDescending(a => a.Rate)
                .ToListAsync(cancellationToken);

            var codes = activities
                .Select(a => a.Code)
                .Concat(activities.SelectMany(a => (a.Children ?? new List<Activity>())
                    .Where(c => !c.Hidden)
                    .Select(c => c.Code)))
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .ToArray();

            var performerCounts = await (
                from ua in _context.UserActivities.AsNoTracking()
                join p in _context.Performers.AsNoTracking() on ua.UserId equals p.PerformerId
                where p.Active && !string.IsNullOrWhiteSpace(ua.DoesCode) && codes.Contains(ua.DoesCode)
                group ua by ua.DoesCode into g
                select new
                {
                    Code = g.Key,
                    Count = g.Select(x => x.UserId).Distinct().Count()
                })
                .ToDictionaryAsync(x => x.Code, x => x.Count, cancellationToken);

            return Ok(activities.Select(a => ToBrowseItem(a, performerCounts)).ToList());
        }

        [HttpGet("{id}/performers")]
        public async Task<ActionResult<IEnumerable<ActivityPerformerDto>>> GetPerformers(
            [FromRoute] string id,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Activity code is required.");
            }

            var activity = await _context.Activities
                .AsNoTracking()
                .SingleOrDefaultAsync(a => a.Code == id, cancellationToken);
            if (activity is null)
            {
                return NotFound();
            }

            var performers = await (
                from p in _context.Performers.AsNoTracking()
                join ua in _context.UserActivities.AsNoTracking() on p.PerformerId equals ua.UserId
                join u in _context.ApplicationUser.AsNoTracking() on p.PerformerId equals u.Id into users
                from user in users.DefaultIfEmpty()
                where p.Active && ua.DoesCode == id
                orderby p.Rate
                select new ActivityPerformerDto
                {
                    PerformerId = p.PerformerId,
                    UserName = user != null ? (user.UserName ?? string.Empty) : string.Empty,
                    Active = p.Active,
                    AcceptNotifications = p.AcceptNotifications,
                    AcceptPublicContact = p.AcceptPublicContact,
                    WebSite = p.WebSite ?? string.Empty,
                    ActivityCode = id,
                    ActivityName = activity.Name,
                    SettingsClassName = _context.Activities
                        .Where(a => a.Code == id)
                        .Select(a => a.SettingsClassName)
                        .FirstOrDefault() ?? string.Empty,
                    ExtraActivityCount = _context.UserActivities
                        .Where(x => x.UserId == p.PerformerId && x.DoesCode != id)
                        .Count()
                })
                .Distinct()
                .ToListAsync(cancellationToken);

            return Ok(performers);
        }

        // GET: api/ActivityApi/5
        [HttpGet("{id}", Name = "GetActivity")]
        public async Task<IActionResult> GetActivity([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Activity activity = await _context.Activities.SingleAsync(m => m.Code == id);

            if (activity == null)
            {
                return NotFound();
            }
            // Also return hidden ones
            // hidden doesn't mean disabled
            return Ok(activity);
        }

        // PUT: api/ActivityApi/5
        [HttpPut("{id}"),Authorize("AdministratorOnly")]
        public async Task<IActionResult> PutActivity([FromRoute] string id, [FromBody] Activity activity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != activity.Code)
            {
                return BadRequest();
            }

            _context.Entry(activity).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(User.GetUserId());
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ActivityExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return new StatusCodeResult(StatusCodes.Status204NoContent);
        }

        // POST: api/ActivityApi
        [HttpPost, Authorize("AdministratorOnly")]
        public async Task<IActionResult> PostActivity([FromBody] Activity activity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Activities.Add(activity);
            try
            {
                await _context.SaveChangesAsync(User.GetUserId());
            }
            catch (DbUpdateException)
            {
                if (ActivityExists(activity.Code))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetActivity", new { id = activity.Code }, activity);
        }

        // DELETE: api/ActivityApi/5
        [HttpDelete("{id}"),Authorize("AdministratorOnly")]
        public async Task<IActionResult> DeleteActivity([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Activity activity = await _context.Activities.SingleAsync(m => m.Code == id);
            if (activity == null)
            {
                return NotFound();
            }

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync(User.GetUserId());

            return Ok(activity);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ActivityExists(string id)
        {
            return _context.Activities.Count(e => e.Code == id) > 0;
        }

        private static ActivityBrowseItemDto ToBrowseItem(
            Activity activity,
            IReadOnlyDictionary<string, int> performerCounts)
        {
            return new ActivityBrowseItemDto
            {
                Code = activity.Code,
                Name = activity.Name,
                ParentCode = activity.ParentCode,
                Description = activity.Description,
                Photo = activity.Photo,
                Rate = activity.Rate,
                PerformerCount = performerCounts.TryGetValue(activity.Code, out var count) ? count : 0,
                Forms = (activity.Forms ?? Enumerable.Empty<CommandForm>())
                    .Select(f => new CommandFormSummaryDto
                    {
                        Id = f.Id,
                        ActionName = f.ActionName,
                        Title = f.Title,
                    })
                    .ToList(),
                Children = (activity.Children ?? Enumerable.Empty<Activity>())
                    .Where(c => !c.Hidden)
                    .OrderByDescending(c => c.Rate)
                    .Select(c => new ActivityBrowseItemDto
                    {
                        Code = c.Code,
                        Name = c.Name,
                        ParentCode = c.ParentCode,
                        Description = c.Description,
                        Photo = c.Photo,
                        Rate = c.Rate,
                        PerformerCount = performerCounts.TryGetValue(c.Code, out var childCount) ? childCount : 0,
                        Forms = (c.Forms ?? Enumerable.Empty<CommandForm>())
                            .Select(f => new CommandFormSummaryDto
                            {
                                Id = f.Id,
                                ActionName = f.ActionName,
                                Title = f.Title,
                            })
                            .ToList(),
                    })
                    .ToList(),
            };
        }
    }
}
