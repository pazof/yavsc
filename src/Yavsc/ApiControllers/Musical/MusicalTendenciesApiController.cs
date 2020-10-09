using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Musical;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/MusicalTendenciesApi")]
    public class MusicalTendenciesApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MusicalTendenciesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/MusicalTendenciesApi
        [HttpGet]
        public IEnumerable<MusicalTendency> GetMusicalTendency()
        {
            return _context.MusicalTendency;
        }

        // GET: api/MusicalTendenciesApi/5
        [HttpGet("{id}", Name = "GetMusicalTendency")]
        public IActionResult GetMusicalTendency([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            MusicalTendency musicalTendency = _context.MusicalTendency.Single(m => m.Id == id);

            if (musicalTendency == null)
            {
                return HttpNotFound();
            }

            return Ok(musicalTendency);
        }

        // PUT: api/MusicalTendenciesApi/5
        [HttpPut("{id}")]
        public IActionResult PutMusicalTendency(long id, [FromBody] MusicalTendency musicalTendency)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (id != musicalTendency.Id)
            {
                return HttpBadRequest();
            }

            _context.Entry(musicalTendency).State = EntityState.Modified;

            try
            {
                _context.SaveChanges(User.GetUserId());
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MusicalTendencyExists(id))
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

        // POST: api/MusicalTendenciesApi
        [HttpPost]
        public IActionResult PostMusicalTendency([FromBody] MusicalTendency musicalTendency)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            _context.MusicalTendency.Add(musicalTendency);
            try
            {
                _context.SaveChanges(User.GetUserId());
            }
            catch (DbUpdateException)
            {
                if (MusicalTendencyExists(musicalTendency.Id))
                {
                    return new HttpStatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetMusicalTendency", new { id = musicalTendency.Id }, musicalTendency);
        }

        // DELETE: api/MusicalTendenciesApi/5
        [HttpDelete("{id}")]
        public IActionResult DeleteMusicalTendency(long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            MusicalTendency musicalTendency = _context.MusicalTendency.Single(m => m.Id == id);
            if (musicalTendency == null)
            {
                return HttpNotFound();
            }

            _context.MusicalTendency.Remove(musicalTendency);
            _context.SaveChanges(User.GetUserId());

            return Ok(musicalTendency);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool MusicalTendencyExists(long id)
        {
            return _context.MusicalTendency.Count(e => e.Id == id) > 0;
        }
    }
}
