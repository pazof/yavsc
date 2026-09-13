#nullable enable annotations

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Npgsql;
using Yavsc.Models;
using Yavsc.Models.Billing;
using Yavsc.Models.Relationship;
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

        if (query.Location is null)
        {
            return BadRequest(new { Error = "location is required" });
        }

        var resolvedLocation = await ResolveLocationAsync(query.Location, cancellationToken);
        if (resolvedLocation is null)
        {
            return BadRequest(new { Error = "location payload is invalid" });
        }

        await PersistLocationIfNeededAsync(resolvedLocation, uid, cancellationToken);

        query.Location = resolvedLocation;

        var addedEntry = _context.RdvQueries.Add(query);
        EnsureLocationForeignKey(addedEntry, resolvedLocation.Id);

        try
        {
            await _context.SaveChangesAsync(User.GetUserId(), cancellationToken);
        }
        catch (DbUpdateException ex) when (IsLocationForeignKeyViolation(ex))
        {
            return BadRequest(new { Error = "location reference is invalid" });
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
            var resolvedLocation = await ResolveLocationAsync(query.Location, cancellationToken);
            if (resolvedLocation is null)
            {
                return BadRequest(new { Error = "location payload is invalid" });
            }

            await PersistLocationIfNeededAsync(resolvedLocation, uid, cancellationToken);

            existing.Location = resolvedLocation;
            EnsureLocationForeignKey(_context.Entry(existing), resolvedLocation.Id);
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
        catch (DbUpdateException ex) when (IsLocationForeignKeyViolation(ex))
        {
            return BadRequest(new { Error = "location reference is invalid" });
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

    private async Task<Location?> ResolveLocationAsync(Location postedLocation, CancellationToken cancellationToken)
    {
        if (postedLocation.Id > 0)
        {
            var byId = await _context.Locations
                .FirstOrDefaultAsync(x => x.Id == postedLocation.Id, cancellationToken);
            if (byId is not null)
            {
                return byId;
            }
        }

        if (string.IsNullOrWhiteSpace(postedLocation.Address))
        {
            return null;
        }

        var existingByCoordinates = await _context.Locations.FirstOrDefaultAsync(
            x => x.Address == postedLocation.Address
              && x.Longitude == postedLocation.Longitude
              && x.Latitude == postedLocation.Latitude,
            cancellationToken);

        if (existingByCoordinates is not null)
        {
            return existingByCoordinates;
        }

        // Treat unknown location ids as client-side placeholders and insert a new row.
        postedLocation.Id = 0;
        _context.Locations.Add(postedLocation);
        return postedLocation;
    }

    private async Task PersistLocationIfNeededAsync(Location location, string userId, CancellationToken cancellationToken)
    {
        if (_context.Entry(location).State != EntityState.Added)
        {
            return;
        }

        await _context.SaveChangesAsync(userId, cancellationToken);
    }

    private static bool IsLocationForeignKeyViolation(DbUpdateException ex)
    {
        if (ex.InnerException is not PostgresException pg)
        {
            return false;
        }

        return pg.SqlState == PostgresErrorCodes.ForeignKeyViolation
            && string.Equals(pg.ConstraintName, "FK_NominativeServiceCommand_Locations_LocationId", StringComparison.Ordinal);
    }

    private static void EnsureLocationForeignKey(EntityEntry<RdvQuery> entry, long locationId)
    {
        SetFkIfPresent(entry, "LocationId", locationId);
        SetFkIfPresent(entry, "RdvQuery_LocationId", locationId);
    }

    private static void SetFkIfPresent(EntityEntry<RdvQuery> entry, string propertyName, long value)
    {
        var property = entry.Metadata.FindProperty(propertyName);
        if (property is null)
        {
            return;
        }

        entry.Property(propertyName).CurrentValue = value;
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
