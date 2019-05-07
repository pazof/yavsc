using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Workflow;

namespace Yavsc.Controllers
{
    public class CoWorkingController : Controller
    {
        private ApplicationDbContext _context;

        public CoWorkingController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: CoWorking
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.CoWorking.Include(c => c.Performer).Include(c => c.WorkingFor);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: CoWorking/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            CoWorking coWorking = await _context.CoWorking.SingleAsync(m => m.Id == id);
            if (coWorking == null)
            {
                return HttpNotFound();
            }

            return View(coWorking);
        }

        // GET: CoWorking/Create
        public IActionResult Create()
        {
            ViewBag.PerformerId = _context.Performers.Select( p=> new SelectListItem { Value = p.PerformerId, Text = p.Performer.UserName});
            ViewBag.WorkingForId = new SelectList(_context.Users, "Id", "UserName");
            return View();
        }

        // POST: CoWorking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CoWorking coWorking)
        {
            if (ModelState.IsValid)
            {
                _context.CoWorking.Add(coWorking);
                await _context.SaveChangesAsync(User.GetUserId());
                return RedirectToAction("Index");
            }
            ViewData["PerformerId"] = new SelectList(_context.Performers, "PerformerId", "Performer", coWorking.PerformerId);
            ViewData["WorkingForId"] = new SelectList(_context.Users, "Id", "WorkingFor", coWorking.WorkingForId);
            return View(coWorking);
        }

        // GET: CoWorking/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            CoWorking coWorking = await _context.CoWorking.SingleAsync(m => m.Id == id);
            if (coWorking == null)
            {
                return HttpNotFound();
            }
            ViewData["PerformerId"] = new SelectList(_context.Performers, "PerformerId", "Performer", coWorking.PerformerId);
            ViewData["WorkingForId"] = new SelectList(_context.Users, "Id", "WorkingFor", coWorking.WorkingForId);
            return View(coWorking);
        }

        // POST: CoWorking/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CoWorking coWorking)
        {
            if (ModelState.IsValid)
            {
                _context.Update(coWorking);
                await _context.SaveChangesAsync(User.GetUserId());
                return RedirectToAction("Index");
            }
            ViewData["PerformerId"] = new SelectList(_context.Performers, "PerformerId", "Performer", coWorking.PerformerId);
            ViewData["WorkingForId"] = new SelectList(_context.Users, "Id", "WorkingFor", coWorking.WorkingForId);
            return View(coWorking);
        }

        // GET: CoWorking/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            CoWorking coWorking = await _context.CoWorking.SingleAsync(m => m.Id == id);
            if (coWorking == null)
            {
                return HttpNotFound();
            }

            return View(coWorking);
        }

        // POST: CoWorking/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            CoWorking coWorking = await _context.CoWorking.SingleAsync(m => m.Id == id);
            _context.CoWorking.Remove(coWorking);
            await _context.SaveChangesAsync(User.GetUserId());
            return RedirectToAction("Index");
        }
    }
}
