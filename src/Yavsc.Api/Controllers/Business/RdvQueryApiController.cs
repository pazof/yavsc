using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Models;
using Yavsc.Models.Billing;
using Yavsc.Models.Workflow;
using Yavsc.Server.Helpers;

namespace Yavsc.Controllers;

[Authorize]
[Produces("application/json")]
[Route(Constants.APIPrefix + "/billing/" + BillingCodes.Rdv)]
public class RdvQueryApiController : Controller
{
    private readonly ApplicationDbContext _context;

    public RdvQueryApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetQueries(CancellationToken cancellationToken)
    {
        var uid = User.GetUserId();

        var queries = await _context.RdvQueries
            .AsNoTracking()
            .Include(q => q.Location)
            .Include(q => q.Client)
            .Include(q => q.PerformerProfile)
            .Where(q => q.ClientId == uid || q.PerformerId == uid)
            .OrderByDescending(q => q.Id)
            .ToListAsync(cancellationToken);

        return Ok(queries);
    }

    [HttpGet("{id}", Name = "GetRdvQuery")]
    public async Task<IActionResult> GetQuery([FromRoute] long id, CancellationToken cancellationToken)
    {
        var uid = User.GetUserId();

        var query = await _context.RdvQueries
            .Include(q => q.Location)
            .Include(q => q.Client)
            .Include(q => q.PerformerProfile)
            .SingleOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (query is null)
        {
            return NotFound();
        }

        if (query.ClientId != uid && query.PerformerId != uid && !User.IsInRole(Constants.AdminGroupName))
        {
            return Forbid();
        }

        return Ok(query);
    }

    [HttpPost]
    public async Task<IActionResult> PostQuery([FromBody] RdvQuery query, CancellationToken cancellationToken)
    {
        var uid = User.GetUserId();
        // Security: the caller always posts for themselves.
        query.ClientId = uid;
        query.EventDate = EnsureUtc(query.EventDate);

        ModelState.Remove("Client");
        ModelState.Remove("ClientId");
        ModelState.Remove("UserCreated");
        ModelState.Remove("UserModified");
        ModelState.Remove("SelectedProfile");
        ModelState.Remove("PerformerProfile");
        ModelState.Remove("Context");
        ModelState.Remove("Regularization");

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (query.Location is not null)
        {
            var existingLocation = await _context.Locations.FirstOrDefaultAsync(
                x => x.Address == query.Location.Address
                  && x.Longitude == query.Location.Longitude
                  && x.Latitude == query.Location.Latitude,
                cancellationToken);

            if (existingLocation is not null)
            {
                query.Location = existingLocation;
            }
            else
            {
                _context.Attach(query.Location);
            }
        }

        _context.RdvQueries.Add(query);

        try
        {
            await _context.SaveChangesAsync(User.GetUserId(), cancellationToken);
        }
        catch (DbUpdateException)
        {
            if (QueryExists(query.Id))
            {
                return Conflict();
            }

            throw;
        }

        return CreatedAtRoute("GetRdvQuery", new { id = query.Id }, query);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutQuery([FromRoute] long id, [FromBody] RdvQuery query, CancellationToken cancellationToken)
    {
        var uid = User.GetUserId();
        var existing = await _context.RdvQueries
            .Include(q => q.Location)
            .SingleOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (existing is null)
        {
            return NotFound();
        }

        if (existing.ClientId != uid && !User.IsInRole(Constants.AdminGroupName))
        {
            return Forbid();
        }

        existing.ActivityCode = query.ActivityCode;
        existing.PerformerId = query.PerformerId;
        existing.Consent = query.Consent;
        existing.EventDate = EnsureUtc(query.EventDate);
        existing.LocationType = query.LocationType;
        existing.Reason = query.Reason;
        existing.Status = query.Status;
        existing.Provisional = query.Provisional;

        if (query.Location is not null)
        {
            var resolvedLocation = await _context.Locations.FirstOrDefaultAsync(
                x => x.Address == query.Location.Address
                  && x.Longitude == query.Location.Longitude
                  && x.Latitude == query.Location.Latitude,
                cancellationToken);

            existing.Location = resolvedLocation ?? query.Location;
            if (resolvedLocation is null)
            {
                _context.Attach(query.Location);
            }
        }

        try
        {
            await _context.SaveChangesAsync(User.GetUserId(), cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!QueryExists(id))
            {
                return NotFound();
            }

            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteQuery([FromRoute] long id, CancellationToken cancellationToken)
    {
        var uid = User.GetUserId();

        var query = await _context.RdvQueries
            .SingleOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (query is null)
        {
            return NotFound();
        }

        if (query.ClientId != uid && !User.IsInRole(Constants.AdminGroupName))
        {
            return Forbid();
        }

        _context.RdvQueries.Remove(query);
        await _context.SaveChangesAsync(User.GetUserId(), cancellationToken);

        return Ok(query);
    }

    private bool QueryExists(long id)
    {
        return _context.RdvQueries.Any(e => e.Id == id);
    }

    private static DateTime EnsureUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
