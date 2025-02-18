
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Blog;

namespace Yavsc.Controllers
{
    /// <summary>
    /// Comment some post.
    /// </summary>
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentsController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Comments
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

        // GET: Comments/Create
        public IActionResult Create()
        {
            ViewData["ReceiverId"] = new SelectList(_context.BlogSpot, "Id", "Post");
            return View();
        }

        // POST: Comments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Comment comment)
        {
            comment.UserCreated = User.GetUserId();
            
            if (ModelState.IsValid)
            {
                _context.Comment.Add(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["ReceiverId"] = new SelectList(_context.BlogSpot, "Id", "Post", comment.ReceiverId);
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
            ViewData["ReceiverId"] = new SelectList(_context.BlogSpot, "Id", "Post", comment.ReceiverId);
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
            ViewData["ReceiverId"] = new SelectList(_context.BlogSpot, "Id", "Post", comment.ReceiverId);
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
