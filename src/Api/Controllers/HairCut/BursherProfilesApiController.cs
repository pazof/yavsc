using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Helpers;
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
                return BadRequest(ModelState);
            }

            BrusherProfile brusherProfile = await _context.BrusherProfile.SingleAsync(m => m.UserId == id);

            if (brusherProfile == null)
            {
                return NotFound();
            }

            return Ok(brusherProfile);
        }

        // PUT: api/BursherProfilesApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBrusherProfile([FromRoute] string id, [FromBody] BrusherProfile brusherProfile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != brusherProfile.UserId)
            {
                return BadRequest();
            }
            
            if (id != User.GetUserId())
            {
                return BadRequest();
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
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return new StatusCodeResult(StatusCodes.Status204NoContent);
        }

        // POST: api/BursherProfilesApi
        [HttpPost]
        public async Task<IActionResult> PostBrusherProfile([FromBody] BrusherProfile brusherProfile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
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
                return BadRequest(ModelState);
            }

            BrusherProfile brusherProfile = await _context.BrusherProfile.SingleAsync(m => m.UserId == id);
            if (brusherProfile == null)
            {
                return NotFound();
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
