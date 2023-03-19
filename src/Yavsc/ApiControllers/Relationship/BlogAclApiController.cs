using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Access;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/blogacl")]
    public class BlogAclApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BlogAclApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/BlogAclApi
        [HttpGet]
        public IEnumerable<CircleAuthorizationToBlogPost> GetBlogACL()
        {
            return _context.CircleAuthorizationToBlogPost;
        }

        // GET: api/BlogAclApi/5
        [HttpGet("{id}", Name = "GetCircleAuthorizationToBlogPost")]
        public async Task<IActionResult> GetCircleAuthorizationToBlogPost([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            CircleAuthorizationToBlogPost circleAuthorizationToBlogPost = await _context.CircleAuthorizationToBlogPost.SingleAsync(
                m => m.CircleId == id && m.Allowed.OwnerId == uid );

            if (circleAuthorizationToBlogPost == null)
            {
                return NotFound();
            }

            return Ok(circleAuthorizationToBlogPost);
        }

        // PUT: api/BlogAclApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCircleAuthorizationToBlogPost([FromRoute] long id, [FromBody] CircleAuthorizationToBlogPost circleAuthorizationToBlogPost)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != circleAuthorizationToBlogPost.CircleId)
            {
                return BadRequest();
            }

            if (!CheckOwner(circleAuthorizationToBlogPost.CircleId))
            {
                return new ChallengeResult();
            }
            _context.Entry(circleAuthorizationToBlogPost).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(User.GetUserId());
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CircleAuthorizationToBlogPostExists(id))
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
        private bool CheckOwner (long circleId)
        {
            
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var circle = _context.Circle.First(c=>c.Id==circleId);
            _context.Entry(circle).State = EntityState.Detached;
            return (circle.OwnerId == uid);
        }
        // POST: api/BlogAclApi
        [HttpPost]
        public async Task<IActionResult> PostCircleAuthorizationToBlogPost([FromBody] CircleAuthorizationToBlogPost circleAuthorizationToBlogPost)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!CheckOwner(circleAuthorizationToBlogPost.CircleId))
            {
                return new ChallengeResult();
            }
            _context.CircleAuthorizationToBlogPost.Add(circleAuthorizationToBlogPost);
            try
            {
                await _context.SaveChangesAsync(User.GetUserId());
            }
            catch (DbUpdateException)
            {
                if (CircleAuthorizationToBlogPostExists(circleAuthorizationToBlogPost.CircleId))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetCircleAuthorizationToBlogPost", new { id = circleAuthorizationToBlogPost.CircleId }, circleAuthorizationToBlogPost);
        }

        // DELETE: api/BlogAclApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCircleAuthorizationToBlogPost([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);

            CircleAuthorizationToBlogPost circleAuthorizationToBlogPost = await _context.CircleAuthorizationToBlogPost.Include(
                a=>a.Allowed
            ).SingleAsync(m => m.CircleId == id
            && m.Allowed.OwnerId == uid);
            if (circleAuthorizationToBlogPost == null)
            {
                return NotFound();
            }
            _context.CircleAuthorizationToBlogPost.Remove(circleAuthorizationToBlogPost);
            await _context.SaveChangesAsync(User.GetUserId());

            return Ok(circleAuthorizationToBlogPost);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CircleAuthorizationToBlogPostExists(long id)
        {
            return _context.CircleAuthorizationToBlogPost.Count(e => e.CircleId == id) > 0;
        }
    }
}
