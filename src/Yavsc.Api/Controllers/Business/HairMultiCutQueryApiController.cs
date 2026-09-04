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
[Route(Constants.APIPrefix + "/billing/" + BillingCodes.MBrush)]
public class HairMultiCutQueryApiController : Controller
{
    private readonly ApplicationDbContext _context;

    public HairMultiCutQueryApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetQueries(CancellationToken cancellationToken)
    {
        var uid = User.GetUserId();

        var queries = await _context.HairMultiCutQueries
            .AsNoTracking()
            .Include(q => q.Prestations)
            .ThenInclude(p => p.Prestation)
            .Include(q => q.Location)
            .Include(q => q.Client)
            .Include(q => q.PerformerProfile)
            .Where(q => q.ClientId == uid || q.PerformerId == uid)
            .OrderByDescending(q => q.Id)
            .ToListAsync(cancellationToken);

        return Ok(queries.Select(SanitizeForResponse).ToList());
    }

    [HttpGet("prestations")]
    public async Task<IActionResult> GetPrestations(CancellationToken cancellationToken)
    {
        var prestations = await _context.HairPrestation
            .AsNoTracking()
            .OrderBy(p => p.Gender)
            .ThenBy(p => p.Length)
            .ThenBy(p => p.Tech)
            .Select(p => new HairPrestationDto
            {
                Id = p.Id,
                Title = p.GetDisplayTitle(),
                Details = p.GetDisplayDetails()
            })
            .ToListAsync(cancellationToken);

        return Ok(prestations);
    }

    [HttpGet("{id}", Name = "GetBillingHairMultiCutQuery")]
    public async Task<IActionResult> GetQuery([FromRoute] long id, CancellationToken cancellationToken)
    {
        var uid = User.GetUserId();

        var query = await _context.HairMultiCutQueries
            .Include(q => q.Prestations)
            .ThenInclude(p => p.Prestation)
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

        return Ok(SanitizeForResponse(query));
    }

    [HttpPost]
    public async Task<IActionResult> PostQuery([FromBody] HairMultiCutQuery query, CancellationToken cancellationToken)
    {
        var uid = User.GetUserId();
        if (string.IsNullOrWhiteSpace(query.ClientId))
        {
            query.ClientId = uid;
        }

        ModelState.Remove("ClientId");

        if (query.ClientId != uid && !User.IsInRole(Constants.AdminGroupName))
        {
            ModelState.AddModelError("ClientId", "You can only create your own HairMultiCutQuery");
            return BadRequest(ModelState);
        }

        if (query.Prestations is null || query.Prestations.Count == 0)
        {
            ModelState.AddModelError("Prestations", "At least one hair prestation is required.");
            return BadRequest(ModelState);
        }

        var prestationItems = await ResolvePrestationsAsync(query.Prestations, cancellationToken);
        if (prestationItems is null)
        {
            ModelState.AddModelError("Prestations", "One or more hair prestations are unknown.");
            return BadRequest(ModelState);
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        query.Prestations = prestationItems;
        query.Location = await ResolveLocationAsync(query.Location, cancellationToken);
        _context.HairMultiCutQueries.Add(query);

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

        return CreatedAtRoute("GetBillingHairMultiCutQuery", new { id = query.Id }, SanitizeForResponse(query));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutQuery([FromRoute] long id, [FromBody] HairMultiCutQuery query, CancellationToken cancellationToken)
    {
        var existing = await _context.HairMultiCutQueries
            .Include(q => q.Prestations)
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

        if (query.Prestations is null || query.Prestations.Count == 0)
        {
            ModelState.AddModelError("Prestations", "At least one hair prestation is required.");
            return BadRequest(ModelState);
        }

        var prestationItems = await ResolvePrestationsAsync(query.Prestations, cancellationToken);
        if (prestationItems is null)
        {
            ModelState.AddModelError("Prestations", "One or more hair prestations are unknown.");
            return BadRequest(ModelState);
        }

        _context.RemoveRange(existing.Prestations);
        existing.ActivityCode = query.ActivityCode;
        existing.PerformerId = query.PerformerId;
        existing.Consent = query.Consent;
        existing.EventDate = query.EventDate;
        existing.Status = query.Status;
        existing.Provisional = query.Provisional;
        existing.Prestations = prestationItems;
        existing.Location = await ResolveLocationAsync(query.Location, cancellationToken);

        await _context.SaveChangesAsync(User.GetUserId(), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteQuery([FromRoute] long id, CancellationToken cancellationToken)
    {
        var uid = User.GetUserId();

        var query = await _context.HairMultiCutQueries
            .Include(q => q.Prestations)
            .SingleOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (query is null)
        {
            return NotFound();
        }

        if (query.ClientId != uid && !User.IsInRole(Constants.AdminGroupName))
        {
            return Forbid();
        }

        _context.RemoveRange(query.Prestations);
        _context.HairMultiCutQueries.Remove(query);
        await _context.SaveChangesAsync(User.GetUserId(), cancellationToken);

        return Ok(SanitizeForResponse(query));
    }

    private async Task<List<HairPrestationCollectionItem>> ResolvePrestationsAsync(
        IEnumerable<HairPrestationCollectionItem> requestedItems,
        CancellationToken cancellationToken)
    {
        var ids = requestedItems
            .Select(x => x.PrestationId)
            .Where(x => x > 0)
            .ToArray();

        if (ids.Length == 0)
        {
            return null;
        }

        var prestations = await _context.HairPrestation
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        if (prestations.Count != ids.Distinct().Count())
        {
            return null;
        }

        return requestedItems
            .Select(item => new HairPrestationCollectionItem
            {
                PrestationId = item.PrestationId,
                Prestation = prestations[item.PrestationId],
            })
            .ToList();
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
        return _context.HairMultiCutQueries.Any(e => e.Id == id);
    }

    private static HairMultiCutQuery SanitizeForResponse(HairMultiCutQuery query)
    {
        if (query.Prestations is not null)
        {
            foreach (var item in query.Prestations)
            {
                item.Query = null;
            }
        }

        return query;
    }
}