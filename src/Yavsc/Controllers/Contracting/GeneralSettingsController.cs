using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Musical.Profiles;

namespace Yavsc.Controllers
{
    public class GeneralSettingsController : Controller
    {
        private ApplicationDbContext _context;

        public GeneralSettingsController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: GeneralSettings
        public async Task<IActionResult> Index()
        {
            return View(await _context.GeneralSettings.ToListAsync());
        }

        // GET: GeneralSettings/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            GeneralSettings generalSettings = await _context.GeneralSettings.SingleAsync(m => m.UserId == id);
            if (generalSettings == null)
            {
                return HttpNotFound();
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
        public async Task<IActionResult> Create(GeneralSettings generalSettings)
        {
            if (ModelState.IsValid)
            {
                _context.GeneralSettings.Add(generalSettings);
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
                return HttpNotFound();
            }

            GeneralSettings generalSettings = await _context.GeneralSettings.SingleAsync(m => m.UserId == id);
            if (generalSettings == null)
            {
                return HttpNotFound();
            }
            return View(generalSettings);
        }

        // POST: GeneralSettings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GeneralSettings generalSettings)
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
                return HttpNotFound();
            }

            GeneralSettings generalSettings = await _context.GeneralSettings.SingleAsync(m => m.UserId == id);
            if (generalSettings == null)
            {
                return HttpNotFound();
            }

            return View(generalSettings);
        }

        // POST: GeneralSettings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            GeneralSettings generalSettings = await _context.GeneralSettings.SingleAsync(m => m.UserId == id);
            _context.GeneralSettings.Remove(generalSettings);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
