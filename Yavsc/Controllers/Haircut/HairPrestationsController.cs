using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Haircut;

namespace Yavsc.Controllers
{
    public class HairPrestationsController : Controller
    {
        private ApplicationDbContext _context;

        public HairPrestationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: HairPrestations
        public async Task<IActionResult> Index()
        {
            return View(await _context.HairPrestation.ToListAsync());
        }

        // GET: HairPrestations/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            HairPrestation hairPrestation = await _context.HairPrestation.SingleAsync(m => m.Id == id);
            if (hairPrestation == null)
            {
                return HttpNotFound();
            }

            return View(hairPrestation);
        }

        // GET: HairPrestations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: HairPrestations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HairPrestation hairPrestation)
        {
            if (ModelState.IsValid)
            {
                _context.HairPrestation.Add(hairPrestation);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(hairPrestation);
        }

        // GET: HairPrestations/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            HairPrestation hairPrestation = await _context.HairPrestation.SingleAsync(m => m.Id == id);
            if (hairPrestation == null)
            {
                return HttpNotFound();
            }
            return View(hairPrestation);
        }

        // POST: HairPrestations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(HairPrestation hairPrestation)
        {
            if (ModelState.IsValid)
            {
                _context.Update(hairPrestation);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(hairPrestation);
        }

        // GET: HairPrestations/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            HairPrestation hairPrestation = await _context.HairPrestation.SingleAsync(m => m.Id == id);
            if (hairPrestation == null)
            {
                return HttpNotFound();
            }

            return View(hairPrestation);
        }

        // POST: HairPrestations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            HairPrestation hairPrestation = await _context.HairPrestation.SingleAsync(m => m.Id == id);
            _context.HairPrestation.Remove(hairPrestation);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
