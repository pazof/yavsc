using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Yavsc.Models;
using Yavsc.Models.Musical;

namespace Yavsc.Controllers
{
    public class InstrumentRatingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InstrumentRatingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: InstrumentRating
        public async Task<IActionResult> Index()
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var applicationDbContext = 
            _context.InstrumentRating
            .Include(i => i.Profile)
            .Include(i => i.Instrument)
            .Where(i => i.OwnerId == uid);

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: InstrumentRating/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);

            InstrumentRating instrumentRating = await _context.InstrumentRating
            .Include(i => i.Instrument).SingleAsync(m => m.Id == id);
            if (instrumentRating == null)
            {
                return NotFound();
            }

            return View(instrumentRating);
        }

        // GET: InstrumentRating/Create
        public async Task<IActionResult> Create()
        {
             var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);

             var actual = await _context.InstrumentRating
                .Where(m => m.OwnerId == uid). Select( r => r.InstrumentId ).ToArrayAsync();
            

            ViewBag.YetAvailableInstruments = _context.Instrument.Select(k=>new SelectListItem 
            { Text = k.Name, Value = k.Id.ToString(), Disabled = actual.Contains(k.Id) });
            
            if (User.IsInRole("Administrator"))
                ViewBag.OwnerIds = new SelectList(_context.Performers, "PerformerId", "Profile");
            return View();
        }

        // POST: InstrumentRating/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InstrumentRating instrumentRating)
        {
            if (ModelState.IsValid)
            {
                _context.InstrumentRating.Add(instrumentRating);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["OwnerId"] = new SelectList(_context.Performers, "PerformerId", "Profile", instrumentRating.OwnerId);
            return View(instrumentRating);
        }

        // GET: InstrumentRating/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            InstrumentRating instrumentRating = await _context.InstrumentRating.SingleAsync(m => m.Id == id);
            if (instrumentRating == null)
            {
                return NotFound();
            }
            ViewData["OwnerId"] = new SelectList(_context.Performers, "PerformerId", "Profile", instrumentRating.OwnerId);
            return View(instrumentRating);
        }

        // POST: InstrumentRating/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(InstrumentRating instrumentRating)
        {
            if (ModelState.IsValid)
            {
                _context.Update(instrumentRating);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["OwnerId"] = new SelectList(_context.Performers, "PerformerId", "Profile", instrumentRating.OwnerId);
            return View(instrumentRating);
        }

        // GET: InstrumentRating/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            InstrumentRating instrumentRating = await _context.InstrumentRating
            .Include(i => i.Instrument).SingleAsync(m => m.Id == id);
            if (instrumentRating == null)
            {
                return NotFound();
            }

            return View(instrumentRating);
        }

        // POST: InstrumentRating/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            InstrumentRating instrumentRating = await _context.InstrumentRating.SingleAsync(m => m.Id == id);
            _context.InstrumentRating.Remove(instrumentRating);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
