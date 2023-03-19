using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Yavsc.Controllers
{
    using System.Security.Claims;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using Yavsc.Helpers;
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
                return BadRequest(ModelState);
            }

            BlogTag postTag = _context.TagsDomain.Single(m => m.PostId == id);

            if (postTag == null)
            {
                return NotFound();
            }

            return Ok(postTag);
        }

        // PUT: api/PostTagsApi/5
        [HttpPut("{id}")]
        public IActionResult PutPostTag(long id, [FromBody] BlogTag postTag)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != postTag.PostId)
            {
                return BadRequest();
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
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return new StatusCodeResult(StatusCodes.Status204NoContent);
        }

        // POST: api/PostTagsApi
        [HttpPost]
        public IActionResult PostPostTag([FromBody] BlogTag postTag)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
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
                return BadRequest(ModelState);
            }

            BlogTag postTag = _context.TagsDomain.Single(m => m.PostId == id);
            if (postTag == null)
            {
                return NotFound();
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
