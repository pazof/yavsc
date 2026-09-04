using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Models;
using Yavsc.Models.Billing;
using Yavsc.Models.Haircut;
using Yavsc.Models.Relationship;
using Yavsc.Server.Helpers;

namespace Yavsc.Controllers;

[Authorize]
[Produces("application/json")]
[Route(Constants.APIPrefix + "/billing/" + BillingCodes.Brush)]
public class HairCutQueryApiController : Controller
{
    private readonly ApplicationDbContext _context;

    public HairCutQueryApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetQueries(CancellationToken cancellationToken)
    {
        var uid = User.GetUserId();

        var queries = await _context.HairCutQueries
            .AsNoTracking()
            .Include(q => q.Prestation)
            .Include(q => q.Location)
            .Include(q => q.Client)
            .Include(q => q.PerformerProfile)
            .Where(q => q.ClientId == uid || q.PerformerId == uid)
            .OrderByDescending(q => q.Id)
            .ToListAsync(cancellationToken);

        return Ok(queries);
    }

    [HttpGet("prestations")]
    public async Task<IActionResult> GetPrestations(CancellationToken cancellationToken)
    {
        var prestations = await _context.HairPrestation
            .AsNoTracking()
            .OrderBy(p => p.Gender)
            .ThenBy(p => p.Length)
            .ThenBy(p => p.Tech)
            .Select(p => ToDto(p))
            .ToListAsync(cancellationToken);

        return Ok(prestations);
    }

    [HttpGet("{id}", Name = "GetBillingHairCutQuery")]
    public async Task<IActionResult> GetQuery([FromRoute] long id, CancellationToken cancellationToken)
    {
        var uid = User.GetUserId();

        var query = await _context.HairCutQueries
            .Include(q => q.Prestation)
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
    public async Task<IActionResult> PostQuery([FromBody] HairCutQuery query, CancellationToken cancellationToken)
    {
        var uid = User.GetUserId();
        if (string.IsNullOrWhiteSpace(query.ClientId))
        {
            query.ClientId = uid;
        }

        ModelState.Remove("ClientId");
        ModelState.Remove("Prestation");

        if (query.ClientId != uid && !User.IsInRole(Constants.AdminGroupName))
        {
            ModelState.AddModelError("ClientId", "You can only create your own HairCutQuery");
            return BadRequest(ModelState);
        }

        query.Prestation = await _context.HairPrestation
            .SingleOrDefaultAsync(p => p.Id == query.PrestationId, cancellationToken);
        if (query.Prestation is null)
        {
            ModelState.AddModelError("PrestationId", "Unknown hair prestation.");
            return BadRequest(ModelState);
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        query.Location = await ResolveLocationAsync(query.Location, cancellationToken);

        _context.HairCutQueries.Add(query);

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

        return CreatedAtRoute("GetBillingHairCutQuery", new { id = query.Id }, query);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutQuery([FromRoute] long id, [FromBody] HairCutQuery query, CancellationToken cancellationToken)
    {
        var existing = await _context.HairCutQueries
            .Include(q => q.Location)
            .SingleOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (existing is null)
        {
            return NotFound();
        }

        var uid = User.GetUserId();
        if (existing.ClientId != uid && !User.IsInRole(Constants.AdminGroupName))
        {
            return Forbid();
        }

        var prestation = await _context.HairPrestation
            .SingleOrDefaultAsync(p => p.Id == query.PrestationId, cancellationToken);
        if (prestation is null)
        {
            ModelState.AddModelError("PrestationId", "Unknown hair prestation.");
            return BadRequest(ModelState);
        }

        existing.ActivityCode = query.ActivityCode;
        existing.PerformerId = query.PerformerId;
        existing.Consent = query.Consent;
        existing.EventDate = query.EventDate;
        existing.AdditionalInfo = query.AdditionalInfo;
        existing.Status = query.Status;
        existing.Provisional = query.Provisional;
        existing.PrestationId = prestation.Id;
        existing.Prestation = prestation;
        existing.Location = await ResolveLocationAsync(query.Location, cancellationToken);

        await _context.SaveChangesAsync(User.GetUserId(), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteQuery([FromRoute] long id, CancellationToken cancellationToken)
    {
        var uid = User.GetUserId();

        var query = await _context.HairCutQueries
            .SingleOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (query is null)
        {
            return NotFound();
        }

        if (query.ClientId != uid && !User.IsInRole(Constants.AdminGroupName))
        {
            return Forbid();
        }

        _context.HairCutQueries.Remove(query);
        await _context.SaveChangesAsync(User.GetUserId(), cancellationToken);

        return Ok(query);
    }

    private async Task<Location> ResolveLocationAsync(Location candidate, CancellationToken cancellationToken)
    {
        if (candidate is null)
        {
            return null;
        }

        var existingLocation = await _context.Locations.FirstOrDefaultAsync(
            x => x.Address == candidate.Address
              && x.Longitude == candidate.Longitude
              && x.Latitude == candidate.Latitude,
            cancellationToken);

        if (existingLocation is not null)
        {
            return existingLocation;
        }

        _context.Attach(candidate);
        return candidate;
    }

    private bool QueryExists(long id)
    {
        return _context.HairCutQueries.Any(e => e.Id == id);
    }

    private static HairPrestationDto ToDto(HairPrestation prestation)
    {
        return new HairPrestationDto
        {
            Id = prestation.Id,
            Title = prestation.GetDisplayTitle(),
            Details = prestation.GetDisplayDetails(),
        };
    }
}