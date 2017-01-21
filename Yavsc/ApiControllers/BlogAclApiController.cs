using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Access;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/blogacl")]
    public class BlogAclApiController : Controller
    {
        private ApplicationDbContext _context;

        public BlogAclApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/BlogAclApi
        [HttpGet]
        public IEnumerable<CircleAuthorizationToBlogPost> GetBlogACL()
        {
            return _context.BlogACL;
        }

        // GET: api/BlogAclApi/5
        [HttpGet("{id}", Name = "GetCircleAuthorizationToBlogPost")]
        public async Task<IActionResult> GetCircleAuthorizationToBlogPost([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }
            var uid = User.GetUserId();
            CircleAuthorizationToBlogPost circleAuthorizationToBlogPost = await _context.BlogACL.SingleAsync(
                m => m.CircleId == id && m.Allowed.OwnerId == uid );

            if (circleAuthorizationToBlogPost == null)
            {
                return HttpNotFound();
            }

            return Ok(circleAuthorizationToBlogPost);
        }

        // PUT: api/BlogAclApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCircleAuthorizationToBlogPost([FromRoute] long id, [FromBody] CircleAuthorizationToBlogPost circleAuthorizationToBlogPost)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (id != circleAuthorizationToBlogPost.CircleId)
            {
                return HttpBadRequest();
            }

            if (!CheckOwner(circleAuthorizationToBlogPost.CircleId))
            {
                return new ChallengeResult();
            }
            _context.Entry(circleAuthorizationToBlogPost).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CircleAuthorizationToBlogPostExists(id))
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
        private bool CheckOwner (long circleId)
        {
            
            var uid = User.GetUserId();
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
                return HttpBadRequest(ModelState);
            }
            if (!CheckOwner(circleAuthorizationToBlogPost.CircleId))
            {
                return new ChallengeResult();
            }
            _context.BlogACL.Add(circleAuthorizationToBlogPost);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CircleAuthorizationToBlogPostExists(circleAuthorizationToBlogPost.CircleId))
                {
                    return new HttpStatusCodeResult(StatusCodes.Status409Conflict);
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
                return HttpBadRequest(ModelState);
            }
            var uid = User.GetUserId();

            CircleAuthorizationToBlogPost circleAuthorizationToBlogPost = await _context.BlogACL.Include(
                a=>a.Allowed
            ).SingleAsync(m => m.CircleId == id
            && m.Allowed.OwnerId == uid);
            if (circleAuthorizationToBlogPost == null)
            {
                return HttpNotFound();
            }
            _context.BlogACL.Remove(circleAuthorizationToBlogPost);
            await _context.SaveChangesAsync();

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
            return _context.BlogACL.Count(e => e.CircleId == id) > 0;
        }
    }
}