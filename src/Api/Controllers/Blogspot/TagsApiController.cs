
using Microsoft.AspNetCore.Mvc;
using Yavsc.Models;

namespace Yavsc.Controllers
{
    using System.Security.Claims;
    using Microsoft.EntityFrameworkCore;
    using Models.Relationship;
    using Yavsc.Helpers;

    [Produces("application/json")]
    [Route("api/TagsApi")]
    public class TagsApiController : Controller
    {
        private ApplicationDbContext _context;
        ILogger _logger;

        public TagsApiController(ApplicationDbContext context,
            ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger<TagsApiController>();
        }

        // GET: api/TagsApi
        [HttpGet]
        public IEnumerable<Tag> GetTag()
        {
            return _context.Tags;
        }

        // GET: api/TagsApi/5
        [HttpGet("{id}", Name = "GetTag")]
        public IActionResult GetTag([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Tag tag = _context.Tags.Single(m => m.Id == id);

            if (tag == null)
            {
                return NotFound();
            }

            return Ok(tag);
        }

        // PUT: api/TagsApi/5
        [HttpPut("{id}")]
        public IActionResult PutTag(long id, [FromBody] Tag tag)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != tag.Id)
            {
                return BadRequest();
            }

            _context.Entry(tag).State = EntityState.Modified;

            try
            {
                _context.SaveChanges(User.GetUserId());
                _logger.LogInformation("Tag created");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TagExists(id))
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

        // POST: api/TagsApi
        [HttpPost]
        public IActionResult PostTag([FromBody] Tag tag)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Tags.Add(tag);
            try
            {
                _context.SaveChanges(User.GetUserId());
            }
            catch (DbUpdateException)
            {
                if (TagExists(tag.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetTag", new { id = tag.Id }, tag);
        }

        // DELETE: api/TagsApi/5
        [HttpDelete("{id}")]
        public IActionResult DeleteTag(long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Tag tag = _context.Tags.Single(m => m.Id == id);
            if (tag == null)
            {
                return NotFound();
            }

            _context.Tags.Remove(tag);
            _context.SaveChanges(User.GetUserId());

            return Ok(tag);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool TagExists(long id)
        {
            return _context.Tags.Count(e => e.Id == id) > 0;
        }
    }
}
