using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Drawing;

namespace Yavsc.Controllers
{
    public class ColorsController : Controller
    {
        private ApplicationDbContext _context;

        public ColorsController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Colors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Color.ToListAsync());
        }

        // GET: Colors/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Color color = await _context.Color.SingleAsync(m => m.Id == id);
            if (color == null)
            {
                return HttpNotFound();
            }

            return View(color);
        }

        // GET: Colors/Create
        public IActionResult Create()
        {
            return View(new Color());
        }

        // POST: Colors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Color color)
        {
            if (ModelState.IsValid)
            {
                _context.Color.Add(color);
                await _context.SaveChangesAsync(User.GetUserId());
                return RedirectToAction("Index");
            }
            return View(color);
        }

        // GET: Colors/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Color color = await _context.Color.SingleAsync(m => m.Id == id);
            if (color == null)
            {
                return HttpNotFound();
            }
            return View(color);
        }

        // POST: Colors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Color color)
        {
            if (ModelState.IsValid)
            {
                _context.Update(color);
                await _context.SaveChangesAsync(User.GetUserId());
                return RedirectToAction("Index");
            }
            return View(color);
        }

        // GET: Colors/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Color color = await _context.Color.SingleAsync(m => m.Id == id);
            if (color == null)
            {
                return HttpNotFound();
            }

            return View(color);
        }

        // POST: Colors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            Color color = await _context.Color.SingleAsync(m => m.Id == id);
            _context.Color.Remove(color);
            await _context.SaveChangesAsync(User.GetUserId());
            return RedirectToAction("Index");
        }
    }
}
