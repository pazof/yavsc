using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Billing;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/Estimate")]
    public class EstimateApiController : Controller
    {
        private ApplicationDbContext _context;

        public EstimateApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Estimate
        [HttpGet]
        public IEnumerable<Estimate> GetEstimates()
        {
            return _context.Estimates;
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

            return Ok(estimate);
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

            _context.Estimates.Add(estimate);
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

            return CreatedAtRoute("GetEstimate", new { id = estimate.Id }, estimate);
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