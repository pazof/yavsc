using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Models;
using Yavsc.Models.Musical.Profiles;

namespace Yavsc.Controllers
{
    public class GeneralSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GeneralSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: GeneralSettings
        public async Task<IActionResult> Index()
        {
            return View(await _context.MusicLoverSettings.ToListAsync());
        }

        // GET: GeneralSettings/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MusicLoverSettings generalSettings = await _context.MusicLoverSettings.SingleAsync(m => m.UserId == id);
            if (generalSettings == null)
            {
                return NotFound();
            }

            return View(generalSettings);
        }

        // GET: GeneralSettings/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GeneralSettings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MusicLoverSettings generalSettings)
        {
            if (ModelState.IsValid)
            {
                _context.MusicLoverSettings.Add(generalSettings);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(generalSettings);
        }

        // GET: GeneralSettings/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MusicLoverSettings generalSettings = await _context.MusicLoverSettings.SingleAsync(m => m.UserId == id);
            if (generalSettings == null)
            {
                return NotFound();
            }
            return View(generalSettings);
        }

        // POST: GeneralSettings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MusicLoverSettings generalSettings)
        {
            if (ModelState.IsValid)
            {
                _context.Update(generalSettings);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(generalSettings);
        }

        // GET: GeneralSettings/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MusicLoverSettings generalSettings = await _context.MusicLoverSettings.SingleAsync(m => m.UserId == id);
            if (generalSettings == null)
            {
                return NotFound();
            }

            return View(generalSettings);
        }

        // POST: GeneralSettings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            MusicLoverSettings generalSettings = await _context.MusicLoverSettings.SingleAsync(m => m.UserId == id);
            _context.MusicLoverSettings.Remove(generalSettings);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
