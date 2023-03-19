
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Relationship;

namespace Yavsc.Controllers
{
    public class CircleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CircleController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Circle
        public async Task<IActionResult> Index()
        {
            return View(await _context.Circle.Where(c=>c.OwnerId==User.GetUserId()).ToListAsync());
        }

        // GET: Circle/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Circle circle = await _context.Circle.SingleAsync(m => m.Id == id);
            if (circle == null)
            {
                return NotFound();
            }
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (uid != circle.OwnerId) return this.Unauthorized();
            return View(circle);
        }

        // GET: Circle/Create
        public IActionResult Create()
        {
            return View(new Circle { OwnerId = User.GetUserId() } );
        }

        // POST: Circle/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Circle circle)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ModelState.IsValid)
            {
                if (uid != circle.OwnerId)
                    return this.Unauthorized();

                _context.Circle.Add(circle);
                await _context.SaveChangesAsync(uid);
                return RedirectToAction("Index");
            }
            return View(circle);
        }

        // GET: Circle/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Circle circle = await _context.Circle.SingleAsync(m => m.Id == id);
            
            if (circle == null)
            {
                return NotFound();
            }
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (uid != circle.OwnerId)
                return Unauthorized();
            return View(circle);
        }

        // POST: Circle/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Circle circle)
        {

            if (ModelState.IsValid)
            {
                var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (uid != circle.OwnerId) return Unauthorized();
                _context.Update(circle);
                await _context.SaveChangesAsync(uid);
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
                return NotFound();
            }

            Circle circle = await _context.Circle.SingleAsync(m => m.Id == id);
             if (circle == null)
            {
                return NotFound();
            }
             var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
             if (uid != circle.OwnerId) return Unauthorized();
            
            return View(circle);
        }

        // POST: Circle/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            Circle circle = await _context.Circle.SingleAsync(m => m.Id == id);
             var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
             if (uid != circle.OwnerId) return Unauthorized();
            _context.Circle.Remove(circle);
            await _context.SaveChangesAsync(uid);
            return RedirectToAction("Index");
        }
    }
}
