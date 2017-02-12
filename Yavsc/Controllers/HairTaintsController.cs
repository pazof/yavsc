using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;

namespace Yavsc.Controllers
{
    using Models;
    using Models.Haircut;
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
            return View(await _context.HairTaint.ToListAsync());
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
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
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
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
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
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
