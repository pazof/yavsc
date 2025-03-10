using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Blog;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/blog")]

    public class BlogApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BlogApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/BlogApi
        [HttpGet]
        public IEnumerable<BlogPost> GetBlogspot(int start=0, int take=25)
        {
            return _context.BlogSpot.OrderByDescending(b => b.UserModified)
            .Skip(start).Take(take);
        }

        // GET: api/BlogApi/5
        [HttpGet("{id}", Name = "GetBlog")]
        public IActionResult GetBlog([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            BlogPost blog = _context.BlogSpot.Single(m => m.Id == id);

            if (blog == null)
            {
                return NotFound();
            }

            return Ok(blog);
        }

        // PUT: api/BlogApi/5
        [HttpPut("{id}")]
        public IActionResult PutBlog(long id, [FromBody] BlogPost blog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != blog.Id)
            {
                return BadRequest();
            }

            _context.Entry(blog).State = EntityState.Modified;

            try
            {
                _context.SaveChanges(User.GetUserId());
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlogExists(id))
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

        // POST: api/BlogApi
        [HttpPost]
        public IActionResult PostBlog([FromBody] Models.Blog.BlogPost blog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.BlogSpot.Add(blog);
            try
            {
                _context.SaveChanges(User.GetUserId());
            }
            catch (DbUpdateException)
            {
                if (BlogExists(blog.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetBlog", new { id = blog.Id }, blog);
        }

        // DELETE: api/BlogApi/5
        [HttpDelete("{id}")]
        public IActionResult DeleteBlog(long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            BlogPost blog = _context.BlogSpot.Single(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            _context.BlogSpot.Remove(blog);
            _context.SaveChanges(User.GetUserId());

            return Ok(blog);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool BlogExists(long id)
        {
            return _context.BlogSpot.Count(e => e.Id == id) > 0;
        }
    }
}
