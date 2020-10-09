using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Haircut;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/bursherprofiles")]
    public class BursherProfilesApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BursherProfilesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/BursherProfilesApi
        [HttpGet]
        public IEnumerable<BrusherProfile> GetBrusherProfile()
        {
            return _context.BrusherProfile.Include(p=>p.BaseProfile).Where(p => p.BaseProfile.Active);
        }

        // GET: api/BursherProfilesApi/5
        [HttpGet("{id}", Name = "GetBrusherProfile")]
        public async Task<IActionResult> GetBrusherProfile([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            BrusherProfile brusherProfile = await _context.BrusherProfile.SingleAsync(m => m.UserId == id);

            if (brusherProfile == null)
            {
                return HttpNotFound();
            }

            return Ok(brusherProfile);
        }

        // PUT: api/BursherProfilesApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBrusherProfile([FromRoute] string id, [FromBody] BrusherProfile brusherProfile)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (id != brusherProfile.UserId)
            {
                return HttpBadRequest();
            }
            
            if (id != User.GetUserId())
            {
                return HttpBadRequest();
            }
            _context.Entry(brusherProfile).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BrusherProfileExists(id))
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

        // POST: api/BursherProfilesApi
        [HttpPost]
        public async Task<IActionResult> PostBrusherProfile([FromBody] BrusherProfile brusherProfile)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            _context.BrusherProfile.Add(brusherProfile);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (BrusherProfileExists(brusherProfile.UserId))
                {
                    return new HttpStatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetBrusherProfile", new { id = brusherProfile.UserId }, brusherProfile);
        }

        // DELETE: api/BursherProfilesApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBrusherProfile([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            BrusherProfile brusherProfile = await _context.BrusherProfile.SingleAsync(m => m.UserId == id);
            if (brusherProfile == null)
            {
                return HttpNotFound();
            }

            _context.BrusherProfile.Remove(brusherProfile);
            await _context.SaveChangesAsync();

            return Ok(brusherProfile);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool BrusherProfileExists(string id)
        {
            return _context.BrusherProfile.Count(e => e.UserId == id) > 0;
        }
    }
}
