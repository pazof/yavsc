
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Blog;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/blogcomments")]
    public class CommentsApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/CommentsApi
        [HttpGet]
        public IEnumerable<Comment> GetComment()
        {
            return _context.Comment;
        }

        // GET: api/CommentsApi/5
        [HttpGet("{id}", Name = "GetComment")]
        public async Task<IActionResult> GetComment([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Comment comment = await _context.Comment.SingleAsync(m => m.Id == id);

            if (comment == null)
            {
                return NotFound();
            }

            return Ok(comment);
        }

        // PUT: api/CommentsApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutComment([FromRoute] long id, [FromBody] Comment comment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != comment.Id)
            {
                return BadRequest();
            }

            _context.Entry(comment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync(User.GetUserId());
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(id))
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

        // POST: api/CommentsApi
        [HttpPost]
        public async Task<IActionResult> PostComment([FromBody] Comment comment)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }
            if (!User.IsInRole(Constants.AdminGroupName))
            {
                if (User.GetUserId()!=comment.AuthorId) {
                    ModelState.AddModelError("Content","Vous ne pouvez pas poster au nom d'un autre.");
                    return new BadRequestObjectResult(ModelState);
                }
            }
            _context.Comment.Add(comment);
            try
            {
                await _context.SaveChangesAsync(User.GetUserId());
            }
            catch (DbUpdateException)
            {
                if (CommentExists(comment.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }
            return CreatedAtRoute("GetComment", new { id = comment.Id }, comment);
        }

        // DELETE: api/CommentsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Comment comment = await _context.Comment.SingleAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            RemoveRecursive(comment);
            await _context.SaveChangesAsync(User.GetUserId());

            return Ok(comment);
        }
        private void RemoveRecursive (Comment comment)
        {
            var children = _context.Comment.Where(c=>c.ParentId==comment.Id).ToList();
            foreach (var child in children) {
                RemoveRecursive(child);
            }
            _context.Comment.Remove(comment);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CommentExists(long id)
        {
            return _context.Comment.Count(e => e.Id == id) > 0;
        }
    }
}
