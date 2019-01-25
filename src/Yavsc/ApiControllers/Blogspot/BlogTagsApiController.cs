using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Blog;
namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/blogtags")]
    public class BlogTagsApiController : Controller
    {
        private ApplicationDbContext _context;

        public BlogTagsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/BlogTagsApi
        [HttpGet]
        public IEnumerable<BlogTag> GetTagsDomain()
        {
            return _context.TagsDomain;
        }

        // GET: api/BlogTagsApi/5
        [HttpGet("{id}", Name = "GetBlogTag")]
        public async Task<IActionResult> GetBlogTag([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            BlogTag blogTag = await _context.TagsDomain.SingleAsync(m => m.PostId == id);

            if (blogTag == null)
            {
                return HttpNotFound();
            }

            return Ok(blogTag);
        }

        // PUT: api/BlogTagsApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBlogTag([FromRoute] long id, [FromBody] BlogTag blogTag)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (id != blogTag.PostId)
            {
                return HttpBadRequest();
            }

            _context.Entry(blogTag).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlogTagExists(id))
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

        // POST: api/BlogTagsApi
        [HttpPost]
        public async Task<IActionResult> PostBlogTag([FromBody] BlogTag blogTag)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            _context.TagsDomain.Add(blogTag);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (BlogTagExists(blogTag.PostId))
                {
                    return new HttpStatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetBlogTag", new { id = blogTag.PostId }, blogTag);
        }

        // DELETE: api/BlogTagsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlogTag([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            BlogTag blogTag = await _context.TagsDomain.SingleAsync(m => m.PostId == id);
            if (blogTag == null)
            {
                return HttpNotFound();
            }

            _context.TagsDomain.Remove(blogTag);
            await _context.SaveChangesAsync();

            return Ok(blogTag);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool BlogTagExists(long id)
        {
            return _context.TagsDomain.Count(e => e.PostId == id) > 0;
        }
    }
}