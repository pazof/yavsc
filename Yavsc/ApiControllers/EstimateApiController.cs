using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Billing;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/estimate"),Authorize()]
    public class EstimateApiController : Controller
    {
        private ApplicationDbContext _context;

        public EstimateApiController(ApplicationDbContext context)
        {
            _context = context;
        }
        bool UserIsAdminOrThis(string uid)
        {
            if (User.IsInRole(Constants.AdminGroupName)) return true;
            return uid == User.GetUserId();
        }
        bool UserIsAdminOrInThese (string oid, string uid)
        {
            if (User.IsInRole(Constants.AdminGroupName)) return true;
            var cuid = User.GetUserId();
            return cuid == uid || cuid == oid;
        }
        // GET: api/Estimate{?ownerId=User.GetUserId()}
        [HttpGet]
        public IActionResult GetEstimates(string ownerId=null)
        {
            if ( ownerId == null ) ownerId = User.GetUserId();
            else if (!UserIsAdminOrThis(ownerId)) // throw new Exception("Not authorized") ;
            // or just do nothing
            return new HttpStatusCodeResult(StatusCodes.Status403Forbidden);
            return Ok(_context.Estimates.Where(e=>e.OwnerId == ownerId));
        }
        // GET: api/Estimate/5
        [HttpGet("{id}", Name = "GetEstimate")]
        public IActionResult GetEstimate([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            Estimate estimate = _context.Estimates.Single(m => m.Id == id);

            if (estimate == null)
            {
                return HttpNotFound();
            }

            if (UserIsAdminOrInThese(estimate.ClientId,estimate.OwnerId))
            return Ok(estimate);
            return new HttpStatusCodeResult(StatusCodes.Status403Forbidden);
        }

        // PUT: api/Estimate/5
        [HttpPut("{id}")]
        public IActionResult PutEstimate(long id, [FromBody] Estimate estimate)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (id != estimate.Id)
            {
                return HttpBadRequest();
            }
            var uid = User.GetUserId();
            if (!User.IsInRole(Constants.AdminGroupName))
            {
                if (uid != estimate.OwnerId)
                {
                    ModelState.AddModelError("OwnerId","You can only modify your own estimates");
                    return HttpBadRequest(ModelState);
                }
            }
            estimate.LatestValidationDate = DateTime.Now;
            _context.Entry(estimate).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EstimateExists(id))
                {
                    return HttpNotFound();
                }
                else
                {
                    throw;
                }
            }

            return new HttpStatusCodeResult(StatusCodes.Status204NoContent);
        }

        // POST: api/Estimate
        [HttpPost]
        public IActionResult PostEstimate([FromBody] Estimate estimate)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }
            var uid = User.GetUserId();
            if (!User.IsInRole(Constants.AdminGroupName))
            {
                if (uid != estimate.OwnerId)
                {
                    ModelState.AddModelError("OwnerId","You can only create your own estimates");
                    return HttpBadRequest(ModelState);
                }
            }
            estimate.LatestValidationDate = DateTime.Now;
            _context.Estimates.Add(estimate);
            foreach (var l in estimate.Bill) _context.Attach<CommandLine>(l);
            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (EstimateExists(estimate.Id))
                {
                    return new HttpStatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetEstimate", new { Id = estimate.Id }, estimate);
        }

        // DELETE: api/Estimate/5
        [HttpDelete("{id}")]
        public IActionResult DeleteEstimate(long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            Estimate estimate = _context.Estimates.Single(m => m.Id == id);
           
            if (estimate == null)
            {
                return HttpNotFound();
            }
            var uid = User.GetUserId();
            if (!User.IsInRole(Constants.AdminGroupName))
            {
                if (uid != estimate.OwnerId)
                {
                    ModelState.AddModelError("OwnerId","You can only create your own estimates");
                    return HttpBadRequest(ModelState);
                }
            }
            _context.Estimates.Remove(estimate);
            _context.SaveChanges();

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