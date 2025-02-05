using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Musical.Profiles;

namespace Yavsc.Controllers
{
    [Authorize]
    public class InstrumentationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InstrumentationController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Instrumentation
        public async Task<IActionResult> Index()
        {
            return View(await _context.Instrumentation.ToListAsync());
        }

        // GET: Instrumentation/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Instrumentation musicianSettings = await _context.Instrumentation.SingleAsync(m => m.UserId == id);
            if (musicianSettings == null)
            {
                return NotFound();
            }

            return View(musicianSettings);
        }

        // GET: Instrumentation/Create
        public IActionResult Create()
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var owned = _context.Instrumentation.Include(i=>i.Tool).Where(i=>i.UserId==uid).Select(i=>i.InstrumentId);
            var ownedArray = owned.ToArray();

            ViewBag.YetAvailableInstruments = _context.Instrument.Select(k=>new SelectListItem 
            { Text = k.Name, Value = k.Id.ToString(), Disabled = ownedArray.Contains(k.Id) });

            return View(new Instrumentation { UserId = uid });
        }

        // POST: Instrumentation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Instrumentation model)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ModelState.IsValid)
            {
                if (model.UserId != uid) if (!User.IsInRole(Constants.AdminGroupName))
                    return new ChallengeResult();

                _context.Instrumentation.Add(model);
                await _context.SaveChangesAsync(User.GetUserId());
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // GET: Instrumentation/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == null)
            {
                return NotFound();
            }
            if (id != uid) if (!User.IsInRole(Constants.AdminGroupName))
                    return new ChallengeResult();
            Instrumentation musicianSettings = await _context.Instrumentation.SingleAsync(m => m.UserId == id);
            if (musicianSettings == null)
            {
                return NotFound();
            }
            return View(musicianSettings);
        }

        // POST: Instrumentation/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Instrumentation musicianSettings)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (musicianSettings.UserId != uid) if (!User.IsInRole(Constants.AdminGroupName))
                    return new ChallengeResult();
            if (ModelState.IsValid)
            {
                _context.Update(musicianSettings);
                await _context.SaveChangesAsync(User.GetUserId());
                return RedirectToAction("Index");
            }
            return View(musicianSettings);
        }

        // GET: Instrumentation/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Instrumentation musicianSettings = await _context.Instrumentation.SingleAsync(m => m.UserId == id);
            if (musicianSettings == null)
            {
                return NotFound();
            }
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (musicianSettings.UserId != uid) if (!User.IsInRole(Constants.AdminGroupName))
                    return new ChallengeResult();
            return View(musicianSettings);
        }

        // POST: Instrumentation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            Instrumentation musicianSettings = await _context.Instrumentation.SingleAsync(m => m.UserId == id);
            
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (musicianSettings.UserId != uid) if (!User.IsInRole(Constants.AdminGroupName))
                    return new ChallengeResult();

            
            _context.Instrumentation.Remove(musicianSettings);
            await _context.SaveChangesAsync(User.GetUserId());
            return RedirectToAction("Index");
        }
    }
}
