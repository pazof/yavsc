using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Workflow.Profiles;

namespace Yavsc.Controllers
{
    public class FormationSettingsController : Controller
    {
        private ApplicationDbContext _context;

        public FormationSettingsController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: FormationSettings
        public async Task<IActionResult> Index()
        {
            return View(await _context.FormationSettings.ToListAsync());
        }

        // GET: FormationSettings/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            FormationSettings formationSettings = await _context.FormationSettings.SingleAsync(m => m.UserId == id);
            if (formationSettings == null)
            {
                return HttpNotFound();
            }

            return View(formationSettings);
        }

        // GET: FormationSettings/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: FormationSettings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FormationSettings formationSettings)
        {
            if (ModelState.IsValid)
            {
                _context.FormationSettings.Add(formationSettings);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(formationSettings);
        }

        // GET: FormationSettings/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            FormationSettings formationSettings = await _context.FormationSettings.SingleAsync(m => m.UserId == id);
            if (formationSettings == null)
            {
                return HttpNotFound();
            }
            return View(formationSettings);
        }

        // POST: FormationSettings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FormationSettings formationSettings)
        {
            if (ModelState.IsValid)
            {
                _context.Update(formationSettings);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(formationSettings);
        }

        // GET: FormationSettings/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            FormationSettings formationSettings = await _context.FormationSettings.SingleAsync(m => m.UserId == id);
            if (formationSettings == null)
            {
                return HttpNotFound();
            }

            return View(formationSettings);
        }

        // POST: FormationSettings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            FormationSettings formationSettings = await _context.FormationSettings.SingleAsync(m => m.UserId == id);
            _context.FormationSettings.Remove(formationSettings);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
