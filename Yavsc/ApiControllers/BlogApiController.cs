using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/blog")]
    public class BlogApiController : Controller
    {
        private ApplicationDbContext _context;

        public BlogApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/BlogApi
        [HttpGet]
        public IEnumerable<Blog> GetBlogspot()
        {
            return _context.Blogspot.Where(b=>b.Visible).OrderByDescending(b=>b.Modified);
        }

        // GET: api/BlogApi/5
        [HttpGet("{id}", Name = "GetBlog")]
        public IActionResult GetBlog([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            Blog blog = _context.Blogspot.Single(m => m.Id == id);

            if (blog == null)
            {
                return HttpNotFound();
            }

            return Ok(blog);
        }

        // PUT: api/BlogApi/5
        [HttpPut("{id}")]
        public IActionResult PutBlog(long id, [FromBody] Blog blog)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (id != blog.Id)
            {
                return HttpBadRequest();
            }

            _context.Entry(blog).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlogExists(id))
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

        // POST: api/BlogApi
        [HttpPost]
        public IActionResult PostBlog([FromBody] Blog blog)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            _context.Blogspot.Add(blog);
            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (BlogExists(blog.Id))
                {
                    return new HttpStatusCodeResult(StatusCodes.Status409Conflict);
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
                return HttpBadRequest(ModelState);
            }

            Blog blog = _context.Blogspot.Single(m => m.Id == id);
            if (blog == null)
            {
                return HttpNotFound();
            }

            _context.Blogspot.Remove(blog);
            _context.SaveChanges();

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
            return _context.Blogspot.Count(e => e.Id == id) > 0;
        }
    }
}