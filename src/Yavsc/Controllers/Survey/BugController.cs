using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.IT.Fixing;
using Yavsc.Models.IT.Evolution;
using Yavsc.Server.Helpers;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Extensions.Localization;

namespace Yavsc.Controllers
{
    public class BugController : Controller
    {
        ApplicationDbContext _context;
        IStringLocalizer<BugController> _localizer;

        public BugController(ApplicationDbContext context, IStringLocalizer<BugController> localizer )
        {
            _context = context;
            _localizer = localizer; 
        }

        // GET: Bug
        public async Task<IActionResult> Index()
        {
            return View(await _context.Bug.ToListAsync());
        }

        // GET: Bug/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Bug bug = await _context.Bug.SingleAsync(m => m.Id == id);
            if (bug == null)
            {
                return HttpNotFound();
            }

            return View(bug);
        }

        // GET: Bug/Create
        public IActionResult Create()
        {
            ViewBag.Features = Features(_context);
            ViewBag.Statuses = Statuses(default(BugStatus));
            return View();
        }

        IEnumerable<SelectListItem> Statuses(BugStatus ?status) =>
            typeof(Yavsc.Models.IT.Fixing.BugStatus).CreateSelectListItems(status);
        IEnumerable<SelectListItem> Features(ApplicationDbContext context) => 
         context.Feature.CreateSelectListItems<Feature>(f => f.Id.ToString(), f => f.ShortName, null)
         .AddNull(_localizer["noAttachedFID"]); 

        // POST: Bug/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Bug bug)
        {
            if (ModelState.IsValid)
            {
                _context.Bug.Add(bug);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.Features = Features(_context);
            ViewBag.Statuses = Statuses(default(BugStatus));
            return View(bug);
        }

        // GET: Bug/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Bug bug = await _context.Bug.SingleAsync(m => m.Id == id);
            if (bug == null)
            {
                return HttpNotFound();
            }

            ViewBag.Features = Features(_context);
            ViewBag.Statuses = Statuses(bug.Status);

            return View(bug);
        }

        // POST: Bug/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Bug bug)
        {
            if (ModelState.IsValid)
            {
                _context.Update(bug);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(bug);
        }

        // GET: Bug/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Bug bug = await _context.Bug.SingleAsync(m => m.Id == id);
            if (bug == null)
            {
                return HttpNotFound();
            }

            return View(bug);
        }

        // POST: Bug/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            Bug bug = await _context.Bug.SingleAsync(m => m.Id == id);
            _context.Bug.Remove(bug);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
