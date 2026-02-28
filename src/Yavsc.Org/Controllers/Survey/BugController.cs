using Microsoft.AspNetCore.Mvc;
using Yavsc.Models;
using Yavsc.Models.IT.Fixing;
using Yavsc.Models.IT.Evolution;
using Yavsc.Server.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Yavsc.Controllers
{
    public class BugController : Controller
    {
        readonly ApplicationDbContext _context;
        readonly IStringLocalizer<BugController> _localizer;
        readonly IStringLocalizer<Yavsc.Models.IT.Fixing.Resources> _statusLocalizer;

        public BugController(ApplicationDbContext context, 
                IStringLocalizer<BugController> localizer,
                IStringLocalizer<Resources> statusLocalizer
                )
        {
            _context = context;
            _localizer = localizer; 
            _statusLocalizer = statusLocalizer;
        }

        // GET: Bug
        public async Task<IActionResult> Index(int skip = 0, int take = 25)
        {
            if (take > 50) return BadRequest();
            ViewData["skip"]=skip;
            ViewData["take"]=take;
            return View(await _context.Bug.Skip(skip).Take(take).ToListAsync());
        }

        // GET: Bug/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Bug bug = await _context.Bug.SingleAsync(m => m.Id == id);
            if (bug == null)
            {
                return NotFound();
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
            _statusLocalizer.CreateSelectListItems(typeof(BugStatus), status); 

        IEnumerable<SelectListItem> Features(ApplicationDbContext context) => 
         context.Feature.CreateSelectListItems<Feature>(f => f.Id.ToString(), f => $"{f.ShortName} ({f.Description})", null)
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
        [Authorize("AdministratorOnly")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Bug bug = await _context.Bug.SingleAsync(m => m.Id == id);
            if (bug == null)
            {
                return NotFound();
            }

            ViewBag.Features = Features(_context);
            ViewBag.Statuses = Statuses(bug.Status);

            return View(bug);
        }

        // POST: Bug/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("AdministratorOnly")]
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
        [Authorize("AdministratorOnly")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Bug bug = await _context.Bug.SingleAsync(m => m.Id == id);
            if (bug == null)
            {
                return NotFound();
            }

            return View(bug);
        }



        // POST: Bug/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize("AdministratorOnly")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            Bug bug = await _context.Bug.SingleAsync(m => m.Id == id);
            _context.Bug.Remove(bug);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        
        [Authorize("AdministratorOnly")]
        public async Task<IActionResult> DeleteAllLike(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }  
            Bug bugref = await _context.Bug.SingleAsync(m => m.Id == id);
            if (bugref == null)
            {
                return null;
            }
            var bugs =  _context.Bug.Where(b => b.Description == bugref.Description);
            return View(bugs);
        }

        // POST: Bug/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("AdministratorOnly")]
        public async Task<IActionResult> DeleteAllLikeConfirmed(long id, int prefixLen = 25)
        {
            Bug bug = await _context.Bug.SingleAsync(m => m.Id == id);
            var bugs = await _context.Bug.Where(b => b.Description.Substring(0, prefixLen) == bug.Description.Substring(0, prefixLen)).ToArrayAsync();
            foreach (var btd in bugs)
                _context.Bug.Remove(btd);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
