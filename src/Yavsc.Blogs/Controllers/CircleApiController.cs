using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Models;
using Yavsc.Models.Relationship;
using Yavsc.Server.Helpers;

namespace Yavsc.Blogs.Controllers
{
    [Produces("application/json")]
    [Route("api/circle")]
    public class CircleApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CircleApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns the caller's own circles. Circles are personal —
        /// the API never exposes another user's circles, even by id.
        /// </summary>
        // GET: api/circle
        [HttpGet]
        public IEnumerable<Circle> GetCircle()
        {
            var uid = User.GetUserId();
            return _context.Circle.Where(c => c.OwnerId == uid);
        }

        /// <summary>
        /// Returns a single circle only when it belongs to the caller.
        /// </summary>
        // GET: api/circle/5
        [HttpGet("{id}", Name = "GetCircle")]
        public async Task<IActionResult> GetCircle([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var uid = User.GetUserId();
            Circle circle = await _context.Circle.SingleOrDefaultAsync(
                m => m.Id == id && m.OwnerId == uid);

            if (circle == null)
            {
                return NotFound();
            }

            return Ok(circle);
        }

        /// <summary>
        /// Replaces a circle. The caller must own it; the server
        /// reasserts ownership regardless of any OwnerId the client
        /// tries to put in the body.
        /// </summary>
        // PUT: api/circle/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCircle([FromRoute] long id, [FromBody] Circle circle)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != circle.Id)
            {
                return BadRequest();
            }

            var uid = User.GetUserId();
            var existing = await _context.Circle.SingleOrDefaultAsync(
                c => c.Id == id && c.OwnerId == uid);
            if (existing is null)
            {
                return new ChallengeResult();
            }

            // Force OwnerId to the caller; the body value is ignored.
            circle.OwnerId = uid;
            _context.Entry(circle).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(User.GetUserId());
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CircleExists(id))
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

        /// <summary>
        /// Creates a circle owned by the caller. The server overwrites
        /// any OwnerId the client sends in the body.
        /// </summary>
        // POST: api/circle
        [HttpPost]
        public async Task<IActionResult> PostCircle([FromBody] Circle circle)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var uid = User.GetUserId();
            circle.OwnerId = uid;

            _context.Circle.Add(circle);
            try
            {
                await _context.SaveChangesAsync(User.GetUserId());
            }
            catch (DbUpdateException)
            {
                if (CircleExists(circle.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetCircle", new { id = circle.Id }, circle);
        }

        /// <summary>
        /// Deletes a circle only if the caller owns it. Returns 404
        /// (not 403) when the circle does not exist or is not owned
        /// by the caller, to avoid leaking the existence of someone
        /// else's circle.
        /// </summary>
        // DELETE: api/circle/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCircle([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var uid = User.GetUserId();
            Circle circle = await _context.Circle.SingleOrDefaultAsync(
                m => m.Id == id && m.OwnerId == uid);
            if (circle == null)
            {
                return NotFound();
            }

            _context.Circle.Remove(circle);
            await _context.SaveChangesAsync(User.GetUserId());

            return Ok(circle);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CircleExists(long id)
        {
            return _context.Circle.Count(e => e.Id == id) > 0;
        }
    }
}
