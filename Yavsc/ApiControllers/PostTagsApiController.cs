using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;

namespace Yavsc.Controllers
{
    using System.Security.Claims;
    using Models;    
    using Yavsc.Models.Blog;

    [Produces("application/json")]
    [Route("~/api/PostTagsApi")]
    public class PostTagsApiController : Controller
    {
        private ApplicationDbContext _context;

        public PostTagsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/PostTagsApi
        [HttpGet]
        public IEnumerable<BlogTag> GetTagsDomain()
        {
            return _context.TagsDomain;
        }

        // GET: api/PostTagsApi/5
        [HttpGet("{id}", Name = "GetPostTag")]
        public IActionResult GetPostTag([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            BlogTag postTag = _context.TagsDomain.Single(m => m.PostId == id);

            if (postTag == null)
            {
                return HttpNotFound();
            }

            return Ok(postTag);
        }

        // PUT: api/PostTagsApi/5
        [HttpPut("{id}")]
        public IActionResult PutPostTag(long id, [FromBody] BlogTag postTag)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (id != postTag.PostId)
            {
                return HttpBadRequest();
            }

            _context.Entry(postTag).State = EntityState.Modified;

            try
            {
                _context.SaveChanges(User.GetUserId());
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostTagExists(id))
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

        // POST: api/PostTagsApi
        [HttpPost]
        public IActionResult PostPostTag([FromBody] BlogTag postTag)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            _context.TagsDomain.Add(postTag);
            try
            {
                _context.SaveChanges(User.GetUserId());
            }
            catch (DbUpdateException)
            {
                if (PostTagExists(postTag.PostId))
                {
                    return new HttpStatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetPostTag", new { id = postTag.PostId }, postTag);
        }

        // DELETE: api/PostTagsApi/5
        [HttpDelete("{id}")]
        public IActionResult DeletePostTag(long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            BlogTag postTag = _context.TagsDomain.Single(m => m.PostId == id);
            if (postTag == null)
            {
                return HttpNotFound();
            }

            _context.TagsDomain.Remove(postTag);
            _context.SaveChanges(User.GetUserId());

            return Ok(postTag);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PostTagExists(long id)
        {
            return _context.TagsDomain.Count(e => e.PostId == id) > 0;
        }
    }
}