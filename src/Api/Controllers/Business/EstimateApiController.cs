using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Billing;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/estimate"), Authorize]
    public class EstimateApiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        public EstimateApiController(ApplicationDbContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<EstimateApiController>();
        }
        bool UserIsAdminOrThis(string uid)
        {
            if (User.IsInRole(Constants.AdminGroupName)) return true;
            return uid == User.GetUserId();
        }
        bool UserIsAdminOrInThese(string oid, string uid)
        {
            if (User.IsInRole(Constants.AdminGroupName)) return true;
            var cuid = User.GetUserId();
            return cuid == uid || cuid == oid;
        }
        // GET: api/Estimate{?ownerId=User.GetUserId()}
        [HttpGet]
        public IActionResult GetEstimates(string ownerId = null)
        {
            if (ownerId == null) ownerId = User.GetUserId();
            else if (!UserIsAdminOrThis(ownerId)) // throw new Exception("Not authorized") ;
                                                  // or just do nothing
                return new StatusCodeResult(StatusCodes.Status403Forbidden);
            return Ok(_context.Estimates.Include(e => e.Bill).Where(e => e.OwnerId == ownerId));
        }
        // GET: api/Estimate/5
        [HttpGet("{id}", Name = "GetEstimate")]
        public IActionResult GetEstimate([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Estimate estimate = _context.Estimates.Include(e => e.Bill).Single(m => m.Id == id);

            if (estimate == null)
            {
                return NotFound();
            }

            if (UserIsAdminOrInThese(estimate.ClientId, estimate.OwnerId))
                return Ok(estimate);
            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        // PUT: api/Estimate/5
        [HttpPut("{id}"), Produces("application/json")]
        public IActionResult PutEstimate(long id, [FromBody] Estimate estimate)
        {

            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }

            if (id != estimate.Id)
            {
                return BadRequest();
            }
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole(Constants.AdminGroupName))
            {
                if (uid != estimate.OwnerId)
                {
                    ModelState.AddModelError("OwnerId", "You can only modify your own estimates");
                    return BadRequest(ModelState);
                }
            }

            var entry = _context.Attach(estimate);
            try
            {
                _context.SaveChanges(User.GetUserId());
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EstimateExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { estimate.Id });
        }

        // POST: api/Estimate
        [HttpPost, Produces("application/json")]
        public IActionResult PostEstimate([FromBody] Estimate estimate)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (estimate.OwnerId == null) estimate.OwnerId = uid;

            if (!User.IsInRole(Constants.AdminGroupName))
            {
                if (uid != estimate.OwnerId)
                {
                    ModelState.AddModelError("OwnerId", "You can only create your own estimates");
                    return BadRequest(ModelState);
                }
            }

            if (estimate.CommandId != null)
            {
                var query = _context.RdvQueries.FirstOrDefault(q => q.Id == estimate.CommandId);
                if (query == null)
                {
                    return BadRequest(ModelState);
                }
                query.ValidationDate = DateTime.Now;
                _context.SaveChanges(User.GetUserId());
                _context.Entry(query).State = EntityState.Detached;
            }
            if (!ModelState.IsValid)
            {
                _logger.LogError(JsonConvert.SerializeObject(ModelState));
                return Json(ModelState);
            }
            _context.Estimates.Add(estimate);


            /* _context.AttachRange(estimate.Bill);
             _context.Attach(estimate);
             _context.Entry(estimate).State = EntityState.Added;
             foreach (var line in estimate.Bill)
                 _context.Entry(line).State = EntityState.Added;
             // foreach (var l in estimate.Bill) _context.Attach<CommandLine>(l);
            */
            try
            {
                _context.SaveChanges(User.GetUserId());
            }
            catch (DbUpdateException)
            {
                if (EstimateExists(estimate.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }
            return Ok(new { estimate.Id, estimate.Bill });
        }

        // DELETE: api/Estimate/5
        [HttpDelete("{id}")]
        public IActionResult DeleteEstimate(long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Estimate estimate = _context.Estimates.Include(e => e.Bill).Single(m => m.Id == id);

            if (estimate == null)
            {
                return NotFound();
            }
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole(Constants.AdminGroupName))
            {
                if (uid != estimate.OwnerId)
                {
                    ModelState.AddModelError("OwnerId", "You can only create your own estimates");
                    return BadRequest(ModelState);
                }
            }
            _context.Estimates.Remove(estimate);
            _context.SaveChanges(User.GetUserId());

            return Ok(estimate);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool EstimateExists(long id)
        {
            return _context.Estimates.Count(e => e.Id == id) > 0;
        }
    }
}
