using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Musical;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/museprefs")]
    public class MusicalPreferencesApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MusicalPreferencesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/MusicalPreferencesApi
        [HttpGet]
        public IEnumerable<MusicalPreference> GetMusicalPreferences()
        {
            return _context.MusicalPreference;
        }

        // GET: api/MusicalPreferencesApi/5
        [HttpGet("{id}", Name = "GetMusicalPreference")]
        public IActionResult GetMusicalPreference([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            MusicalPreference musicalPreference = _context.MusicalPreference.Single(m => m.OwnerProfileId == id);

            if (musicalPreference == null)
            {
                return NotFound();
            }

            return Ok(musicalPreference);
        }

        // PUT: api/MusicalPreferencesApi/5
        public IActionResult PutMusicalPreference(string id, [FromBody] MusicalPreference musicalPreference)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != musicalPreference.OwnerProfileId)
            {
                return BadRequest();
            }

            _context.Entry(musicalPreference).State = EntityState.Modified;

            try
            {
                _context.SaveChanges(User.GetUserId());
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MusicalPreferenceExists(id))
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

        // POST: api/MusicalPreferencesApi
        [HttpPost]
        public IActionResult PostMusicalPreference([FromBody] MusicalPreference musicalPreference)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.MusicalPreference.Add(musicalPreference);
            try
            {
                _context.SaveChanges(User.GetUserId());
            }
            catch (DbUpdateException)
            {
                if (MusicalPreferenceExists(musicalPreference.OwnerProfileId))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetMusicalPreference", new { id = musicalPreference.OwnerProfileId }, musicalPreference);
        }

        // DELETE: api/MusicalPreferencesApi/5
        [HttpDelete("{id}")]
        public IActionResult DeleteMusicalPreference(string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            MusicalPreference musicalPreference = _context.MusicalPreference.Single(m => m.OwnerProfileId == id);
            if (musicalPreference == null)
            {
                return NotFound();
            }

            _context.MusicalPreference.Remove(musicalPreference);
            _context.SaveChanges(User.GetUserId());

            return Ok(musicalPreference);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool MusicalPreferenceExists(string id)
        {
            return _context.MusicalPreference.Count(e => e.OwnerProfileId == id) > 0;
        }
    }
}
