
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Abstract.BlogSpot;
using Yavsc.Models;
using Yavsc.Models.Access;
using Yavsc.Server.Helpers;
using static Yavsc.Constants;

namespace Yavsc.Blogs.Controllers
{
    [Produces("application/json")]
    [Route(APIPrefix+"/blogacl")]
    public class BlogAclApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BlogAclApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns the ACL entries for the caller's own blog posts.
        /// Blog posts (and therefore their ACLs) are private to their
        /// author — the API never exposes another user's ACL.
        /// </summary>
        // GET: api/v1/blogacl
        [HttpGet]
        public IEnumerable<CircleAuthorizationToBlogPost> GetBlogACL()
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return _context.CircleAuthorizationToBlogPost
                .Include(a => a.Allowed)
                .Where(a => a.Allowed.OwnerId == uid);
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

            if (!await CheckOwnerAsync(circleAuthorizationToBlogPost.CircleId))
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
        private async Task<bool> CheckOwnerAsync (long circleId)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (uid==null) return false;
            var circle = await _context.Circle.FirstOrDefaultAsync(c=>c.Id==circleId);
            if (circle == null) return false;
            return circle.OwnerId == uid;
        }
        // POST: api/BlogAclApi
        [HttpPost]
        public async Task<IActionResult> PostCircleAuthorizationToBlogPost(
            [FromBody] PostAccessControlRulePayload circleAuthorizationToBlogPost)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!await CheckOwnerAsync(circleAuthorizationToBlogPost.CircleId))
            {
                return new ChallengeResult();
            }
            CircleAuthorizationToBlogPost entity = new CircleAuthorizationToBlogPost
            {
                BlogPostId = circleAuthorizationToBlogPost.BlogPostId,
                CircleId = circleAuthorizationToBlogPost.CircleId
            };
            _context.CircleAuthorizationToBlogPost.Add(entity);
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
