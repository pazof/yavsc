using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Relationship;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/cirle")]
    public class CircleApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CircleApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/CircleApi
        [HttpGet]
        public IEnumerable<Circle> GetCircle()
        {
            return _context.Circle;
        }

        // GET: api/CircleApi/5
        [HttpGet("{id}", Name = "GetCircle")]
        public async Task<IActionResult> GetCircle([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Circle circle = await _context.Circle.SingleAsync(m => m.Id == id);

            if (circle == null)
            {
                return NotFound();
            }

            return Ok(circle);
        }

        // PUT: api/CircleApi/5
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

        // POST: api/CircleApi
        [HttpPost]
        public async Task<IActionResult> PostCircle([FromBody] Circle circle)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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

        // DELETE: api/CircleApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCircle([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Circle circle = await _context.Circle.SingleAsync(m => m.Id == id);
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
