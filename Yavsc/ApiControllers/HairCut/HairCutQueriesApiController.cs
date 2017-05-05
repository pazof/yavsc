using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Haircut;
using Yavsc.Models.Haircut.Views;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/haircutquery")]
    public class HairCutQueriesApiController : Controller
    {
        private ApplicationDbContext _context;

        public HairCutQueriesApiController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: api/HairCutQueriesApi
        // Get the active queries for current
        // user, as a client
        [HttpGet]
        public async Task<IActionResult> GetHairCutQueries()
        {
            IEnumerable<HaircutQueryClientInfo> info = null;
            await Task.Run(
                 () =>
                 {
                     var bi = DateTime.Now.AddDays(-15);

                     var uid = User.GetUserId();

                     var dat = _context.HairCutQueries
                         .Include(q => q.Prestation)
                         .Include(q => q.Client)
                         .Include(q => q.PerformerProfile)
                         .Include(q => q.Location)
                         .Where(q => q.ClientId == uid
                         && (q.EventDate == null || q.EventDate > bi))
                         ;
                     info = dat.ToArray().Select(q => new HaircutQueryClientInfo(q));
                 });

             return Ok(info);
        }

        // GET: api/HairCutQueriesApi/5
        [HttpGet("{id}", Name = "GetHairCutQuery")]
        public async Task<IActionResult> GetHairCutQuery([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            HairCutQuery hairCutQuery = await _context.HairCutQueries.SingleAsync(m => m.Id == id);

            if (hairCutQuery == null)
            {
                return HttpNotFound();
            }

            return Ok(hairCutQuery);
        }

        // PUT: api/HairCutQueriesApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHairCutQuery([FromRoute] long id, [FromBody] HairCutQuery hairCutQuery)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (id != hairCutQuery.Id)
            {
                return HttpBadRequest();
            }

            _context.Entry(hairCutQuery).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HairCutQueryExists(id))
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

        // POST: api/HairCutQueriesApi
        [HttpPost]
        public async Task<IActionResult> PostHairCutQuery([FromBody] HairCutQuery hairCutQuery)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            _context.HairCutQueries.Add(hairCutQuery);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (HairCutQueryExists(hairCutQuery.Id))
                {
                    return new HttpStatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetHairCutQuery", new { id = hairCutQuery.Id }, hairCutQuery);
        }

        // DELETE: api/HairCutQueriesApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHairCutQuery([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            HairCutQuery hairCutQuery = await _context.HairCutQueries.SingleAsync(m => m.Id == id);
            if (hairCutQuery == null)
            {
                return HttpNotFound();
            }

            _context.HairCutQueries.Remove(hairCutQuery);
            await _context.SaveChangesAsync();

            return Ok(hairCutQuery);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool HairCutQueryExists(long id)
        {
            return _context.HairCutQueries.Count(e => e.Id == id) > 0;
        }
    }
}
