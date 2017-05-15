using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Haircut;

namespace Yavsc.Controllers
{
    [Authorize("AdministratorOnly")]
    public class HairTaintsController : Controller
    {
        private ApplicationDbContext _context;

        public HairTaintsController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: HairTaints
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.HairTaint.Include(h => h.Color);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: HairTaints/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            HairTaint hairTaint = await _context.HairTaint.SingleAsync(m => m.Id == id);
            if (hairTaint == null)
            {
                return HttpNotFound();
            }

            return View(hairTaint);
        }

        // GET: HairTaints/Create
        public IActionResult Create()
        {
            ViewBag.ColorId = new SelectList(_context.Color, "Id", "Name");
            return View();
        }

        // POST: HairTaints/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HairTaint hairTaint)
        {
            if (ModelState.IsValid)
            {
                _context.HairTaint.Add(hairTaint);
                await _context.SaveChangesAsync(User.GetUserId());
                return RedirectToAction("Index");
            }
            ViewBag.ColorId = new SelectList(_context.Color, "Id", "Name", hairTaint.ColorId);
            return View(hairTaint);
        }

        // GET: HairTaints/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            HairTaint hairTaint = await _context.HairTaint.SingleAsync(m => m.Id == id);
            if (hairTaint == null)
            {
                return HttpNotFound();
            }
            ViewBag.ColorId = new SelectList(_context.Color, "Id", "Name",hairTaint.ColorId);
            return View(hairTaint);
        }

        // POST: HairTaints/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(HairTaint hairTaint)
        {
            if (ModelState.IsValid)
            {
                _context.Update(hairTaint);
                await _context.SaveChangesAsync(User.GetUserId());
                return RedirectToAction("Index");
            }
            ViewBag.ColorId = new SelectList(_context.Color, "Id", "Name", hairTaint.ColorId);
            return View(hairTaint);
        }

        // GET: HairTaints/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            HairTaint hairTaint = await _context.HairTaint.SingleAsync(m => m.Id == id);
            if (hairTaint == null)
            {
                return HttpNotFound();
            }

            return View(hairTaint);
        }

        // POST: HairTaints/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            HairTaint hairTaint = await _context.HairTaint.SingleAsync(m => m.Id == id);
            _context.HairTaint.Remove(hairTaint);
            await _context.SaveChangesAsync(User.GetUserId());
            return RedirectToAction("Index");
        }
    }
}
