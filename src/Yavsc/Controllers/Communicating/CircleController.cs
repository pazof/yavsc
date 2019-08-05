
using System.Linq;
using System.Security.Claims;
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
            return View(await _context.Circle.Where(c=>c.OwnerId==User.GetUserId()).ToListAsync());
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
            var uid = User.GetUserId();
            if (uid != circle.OwnerId) return this.HttpUnauthorized();
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
            var uid = User.GetUserId();
            if (ModelState.IsValid)
            {
                if (uid != circle.OwnerId)
                    return this.HttpUnauthorized();

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
                return HttpNotFound();
            }

            Circle circle = await _context.Circle.SingleAsync(m => m.Id == id);
            
            if (circle == null)
            {
                return HttpNotFound();
            }
            var uid = User.GetUserId();
            if (uid != circle.OwnerId)
                return this.HttpUnauthorized();
            return View(circle);
        }

        // POST: Circle/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Circle circle)
        {

            if (ModelState.IsValid)
            {
                var uid = User.GetUserId();
                if (uid != circle.OwnerId) return this.HttpUnauthorized();
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
                return HttpNotFound();
            }

            Circle circle = await _context.Circle.SingleAsync(m => m.Id == id);
             if (circle == null)
            {
                return HttpNotFound();
            }
             var uid = User.GetUserId();
             if (uid != circle.OwnerId) return this.HttpUnauthorized();
            
            return View(circle);
        }

        // POST: Circle/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            Circle circle = await _context.Circle.SingleAsync(m => m.Id == id);
             var uid = User.GetUserId();
             if (uid != circle.OwnerId) return this.HttpUnauthorized();
            _context.Circle.Remove(circle);
            await _context.SaveChangesAsync(uid);
            return RedirectToAction("Index");
        }
    }
}
