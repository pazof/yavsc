using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Musical.Profiles;

namespace Yavsc.Controllers
{
    public class DjSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DjSettingsController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: DjSettings
        public async Task<IActionResult> Index()
        {
            return View(await _context.DjSettings.ToListAsync());
        }

        // GET: DjSettings/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            DjSettings djSettings = await _context.DjSettings.SingleAsync(m => m.UserId == id);
            if (djSettings == null)
            {
                return HttpNotFound();
            }

            return View(djSettings);
        }

        // GET: DjSettings/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DjSettings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DjSettings djSettings)
        {
            if (ModelState.IsValid)
            {
                _context.DjSettings.Add(djSettings);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(djSettings);
        }

        // GET: DjSettings/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            DjSettings djSettings = await _context.DjSettings.SingleAsync(m => m.UserId == id);
            if (djSettings == null)
            {
                return HttpNotFound();
            }
            return View(djSettings);
        }

        // POST: DjSettings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DjSettings djSettings)
        {
            if (ModelState.IsValid)
            {
                _context.Update(djSettings);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(djSettings);
        }

        // GET: DjSettings/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            DjSettings djSettings = await _context.DjSettings.SingleAsync(m => m.UserId == id);
            if (djSettings == null)
            {
                return HttpNotFound();
            }

            return View(djSettings);
        }

        // POST: DjSettings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            DjSettings djSettings = await _context.DjSettings.SingleAsync(m => m.UserId == id);
            _context.DjSettings.Remove(djSettings);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
