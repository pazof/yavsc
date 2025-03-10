
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Blog;

namespace Yavsc.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/blogcomments")]
    public class CommentsApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

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
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CommentPost post)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }
            var article = await _context.BlogSpot.FirstOrDefaultAsync
            (p=> p.Id == post.ReceiverId);

            if (article==null) {
                ModelState.AddModelError("ReceiverId", "not found");
                return BadRequest(ModelState);
                }
            if (post.ParentId!=null)
            {
                var parentExists = _context.Comment.Any(c => c.Id == post.ParentId);
                if (!parentExists)
                {
                    ModelState.AddModelError("ParentId", "not found");
                    return BadRequest(ModelState);
                }
            }
            string uid = User.GetUserId();
            Comment c = new Comment{
                ReceiverId = post.ReceiverId,
                Content = post.Content,
                ParentId = post.ParentId,
                AuthorId = uid,
                UserModified = uid
            };

            _context.Comment.Add(c);
            try
            {
                await _context.SaveChangesAsync(uid);
            }
            catch (DbUpdateException)
            {
                if (CommentExists(c.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }
            return CreatedAtRoute("GetComment", new { id = c.Id }, new { id = c.Id, dateCreated = c.DateCreated });
        }

        // DELETE: api/CommentsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] long id)
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
            var children = _context.Comment.Where
                (c=>c.ParentId==comment.Id).ToList();
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
