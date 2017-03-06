using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using YavscLib;
using System.Linq;

namespace Yavsc.Controllers.Generic
{
    public abstract class SettingsController<TSettings> : Controller where TSettings : class, ISpecializationSettings, new()
    {
        protected ApplicationDbContext _context;
        protected object dbSet;

        protected IQueryable<TSettings> QueryableDbSet { get { 
            return (IQueryable<TSettings>) dbSet;
        } }
        protected ISet<TSettings> RwDbSet { get { 
            return (ISet<TSettings>) dbSet;
        } }

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
            dbSet=_context.GetDbSet<TSettings>();
        }

        public async Task<IActionResult> Index()
        {
            var existing = await this.QueryableDbSet.SingleOrDefaultAsync(p=>p.UserId == User.GetUserId());
            return View(existing);
        }
        // GET: BrusherProfile/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                id = User.GetUserId();
            }

            var profile = await QueryableDbSet.SingleAsync(m => m.UserId == id);
            if (profile == null)
            {
                return HttpNotFound();
            }

            return View(profile);
        }
        

        // GET: BrusherProfile/Create
        public IActionResult Create()
        {
            return View();
        }

        // GET: BrusherProfile/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                id = User.GetUserId();
            }

            TSettings setting = await QueryableDbSet.SingleOrDefaultAsync(m => m.UserId == id);
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

            var brusherProfile = await QueryableDbSet.SingleAsync(m => m.UserId == id);
            if (brusherProfile == null)
            {
                return HttpNotFound();
            }

            return View(brusherProfile);
        }

        // POST: BrusherProfile/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public abstract Task<IActionResult> DeleteConfirmed(string id);
        // POST: BrusherProfile/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public abstract Task<IActionResult> Edit(TSettings profile);
    }
}