
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Relationship;

namespace Yavsc.Controllers
{
    public class CircleController : Controller
    {
        private ApplicationDbContext _context;

        public CircleController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Circle
        public async Task<IActionResult> Index()
        {
            return View(await _context.Circle.ToListAsync());
        }

        // GET: Circle/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Circle circle = await _context.Circle.SingleAsync(m => m.Id == id);
            if (circle == null)
            {
                return HttpNotFound();
            }

            return View(circle);
        }

        // GET: Circle/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Circle/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Circle circle)
        {
            if (ModelState.IsValid)
            {
                _context.Circle.Add(circle);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(circle);
        }

        // GET: Circle/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Circle circle = await _context.Circle.SingleAsync(m => m.Id == id);
            if (circle == null)
            {
                return HttpNotFound();
            }
            return View(circle);
        }

        // POST: Circle/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Circle circle)
        {
            if (ModelState.IsValid)
            {
                _context.Update(circle);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(circle);
        }

        // GET: Circle/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Circle circle = await _context.Circle.SingleAsync(m => m.Id == id);
            if (circle == null)
            {
                return HttpNotFound();
            }

            return View(circle);
        }

        // POST: Circle/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            Circle circle = await _context.Circle.SingleAsync(m => m.Id == id);
            _context.Circle.Remove(circle);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
