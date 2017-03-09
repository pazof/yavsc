using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Exceptions;
using Yavsc.Models;
using YavscLib;

namespace Yavsc.Controllers.Generic
{
    [Authorize]
    public abstract class SettingsController<TSettings> : Controller where TSettings : class, ISpecializationSettings, new()
    {
        protected ApplicationDbContext _context;
        private object dbSet;

        protected DbSet<TSettings> Settings { get { 
            return (DbSet<TSettings>) dbSet;
        } }

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
            dbSet=_context.GetDbSet<TSettings>();
            if (dbSet==null) throw new InvalidWorkflowModelException(this.GetType().Name);
        }

        public async Task<IActionResult> Index()
        {
            var existing = await this.Settings.SingleOrDefaultAsync(p=>p.UserId == User.GetUserId());
            return View(existing);
        }
        // GET: BrusherProfile/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                id = User.GetUserId();
            }

            var profile = await Settings.SingleAsync(m => m.UserId == id);
            if (profile == null)
            {
                return HttpNotFound();
            }

            return View(profile);
        }
        

        // GET: BrusherProfile/Create
        public IActionResult Create()
        {
            return View("Edit", new TSettings());
        }

        // GET: BrusherProfile/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                id = User.GetUserId();
            }

            TSettings setting = await Settings.SingleOrDefaultAsync(m => m.UserId == id);
            if (setting == null)
            {
                setting = new TSettings { };
            }
            return View(setting);
        }

        

        // GET: BrusherProfile/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            var brusherProfile = await Settings.SingleAsync(m => m.UserId == id);
            if (brusherProfile == null)
            {
                return HttpNotFound();
            }

            return View(brusherProfile);
        }

        // POST: FormationSettings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TSettings settings)
        {
            if (settings.UserId == null) settings.UserId = User.GetUserId();

            if (ModelState.IsValid)
            {
                Settings.Add(settings);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View("Edit",settings);
        }
        
        // POST: FormationSettings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TSettings settings)
        {
            if (settings.UserId == null) {
                settings.UserId = User.GetUserId();
                Settings.Add(settings);
            } else 
                _context.Update(settings);
            if (ModelState.IsValid)
            {
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(settings);
        }
        
        // POST: FormationSettings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public  async Task<IActionResult> DeleteConfirmed(string id)
        {
            TSettings formationSettings = await Settings.SingleAsync(m => m.UserId == id);
            Settings.Remove(formationSettings);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

    }
}