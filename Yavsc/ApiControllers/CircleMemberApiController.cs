using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Relationship;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/circlemember")]
    public class CircleMemberApiController : Controller
    {
        private ApplicationDbContext _context;

        public CircleMemberApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/CircleMemberApi
        [HttpGet]
        public IEnumerable<CircleMember> GetCircleMembers()
        {
            return _context.CircleMembers;
        }

        // GET: api/CircleMemberApi/5
        [HttpGet("{id}", Name = "GetCircleMember")]
        public async Task<IActionResult> GetCircleMember([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            CircleMember circleMember = await _context.CircleMembers.SingleAsync(m => m.Id == id);

            if (circleMember == null)
            {
                return HttpNotFound();
            }

            return Ok(circleMember);
        }

        // PUT: api/CircleMemberApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCircleMember([FromRoute] long id, [FromBody] CircleMember circleMember)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (id != circleMember.Id)
            {
                return HttpBadRequest();
            }

            _context.Entry(circleMember).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CircleMemberExists(id))
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

        // POST: api/CircleMemberApi
        [HttpPost]
        public async Task<IActionResult> PostCircleMember([FromBody] CircleMember circleMember)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            _context.CircleMembers.Add(circleMember);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CircleMemberExists(circleMember.Id))
                {
                    return new HttpStatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetCircleMember", new { id = circleMember.Id }, circleMember);
        }

        // DELETE: api/CircleMemberApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCircleMember([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            CircleMember circleMember = await _context.CircleMembers.SingleAsync(m => m.Id == id);
            if (circleMember == null)
            {
                return HttpNotFound();
            }

            _context.CircleMembers.Remove(circleMember);
            await _context.SaveChangesAsync();

            return Ok(circleMember);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CircleMemberExists(long id)
        {
            return _context.CircleMembers.Count(e => e.Id == id) > 0;
        }
    }
}