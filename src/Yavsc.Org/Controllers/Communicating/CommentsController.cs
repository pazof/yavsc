
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Yavsc.Models;
using Yavsc.Models.Blog;
using Yavsc.Server.Helpers;

namespace Yavsc.Controllers
{

    /// <summary>
    /// Comment some post.
    /// </summary>
    [Route("~/api/v1/blogcomments")]
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentsController(ApplicationDbContext context)
        {
            _context = context;    
        }

        [HttpGet("{id:long}", Name = "GetComment")]
        public async Task<IActionResult> GetComment(long id)
        {
            var comment = await _context.Comment.SingleOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return Ok(comment);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        [Consumes("application/json")]
        public async Task<IActionResult> Post([FromBody] CommentPost post)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var uid = User.GetUserId();
            if (string.IsNullOrEmpty(uid))
            {
                return Challenge();
            }

            var article = await _context.BlogSpot.FirstOrDefaultAsync(p => p.Id == post.ReceiverId);
            if (article == null)
            {
                ModelState.AddModelError(nameof(post.ReceiverId), "not found");
                return BadRequest(ModelState);
            }

            if (post.ParentId != null)
            {
                var parentExists = await _context.Comment.AnyAsync(c => c.Id == post.ParentId);
                if (!parentExists)
                {
                    ModelState.AddModelError(nameof(post.ParentId), "not found");
                    return BadRequest(ModelState);
                }
            }

            var comment = new Comment
            {
                ReceiverId = post.ReceiverId,
                Article = post.Article,
                ParentId = post.ParentId,
                AuthorId = uid,
                UserModified = uid
            };

            _context.Comment.Add(comment);
            await _context.SaveChangesAsync(uid);

            return CreatedAtRoute("GetComment", new { id = comment.Id }, new { id = comment.Id, dateCreated = comment.DateCreated });
        }

        // GET: Comments
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Comment.Include(c => c.Post);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Comments/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Comment comment = await _context.Comment.SingleAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // GET: Comments/Create (MVC form endpoint)
        [HttpGet("form")]
        public IActionResult Create()
        {
            ViewBag.ReceiverId = new SelectList(_context.BlogSpot, "Id", "Title");
            return View();
        }

        // POST: Comments/Create (MVC form endpoint)
        [HttpPost("form")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Comment comment)
        {
            comment.UserCreated = User.GetUserId();
            // AuthorId/UserCreated is set server-side after model binding;
            // remove the stale binding error so a valid authenticated POST
            // does not fall into the invalid branch.
            ModelState.Remove(nameof(Comment.AuthorId));
            
            if (ModelState.IsValid)
            {
                _context.Comment.Add(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.ReceiverId = new SelectList(_context.BlogSpot, "Id", "Title", comment.ReceiverId);
            return View(comment);
        }

        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Comment comment = await _context.Comment.SingleAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }
            ViewBag.ReceiverId = new SelectList(_context.BlogSpot, "Id", "Title", comment.ReceiverId);
            return View(comment);
        }

        // POST: Comments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Comment comment)
        {
            if (ModelState.IsValid)
            {
                _context.Update(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.ReceiverId = new SelectList(_context.BlogSpot, "Id", "Title", comment.ReceiverId);
            return View(comment);
        }

        // GET: Comments/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Comment comment = await _context.Comment.SingleAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            Comment comment = await _context.Comment.SingleAsync(m => m.Id == id);
            _context.Comment.Remove(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
