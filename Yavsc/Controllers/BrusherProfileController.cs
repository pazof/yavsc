using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using System.Security.Claims;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Haircut;
using Microsoft.AspNet.Authorization;
using System;

namespace Yavsc.Controllers
{
    [Authorize(Roles="Performer")]
    public class BrusherProfileController : Controller
    {
        private ApplicationDbContext _context;

        public BrusherProfileController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: BrusherProfile
        public async Task<IActionResult> Index()
        {
            var existing = await _context.BrusherProfile.SingleOrDefaultAsync(p=>p.UserId == User.GetUserId());
            return View(existing);
        }

        // GET: BrusherProfile/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                id = User.GetUserId();
            }

            BrusherProfile brusherProfile = await _context.BrusherProfile.SingleAsync(m => m.UserId == id);
            if (brusherProfile == null)
            {
                return HttpNotFound();
            }

            return View(brusherProfile);
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

            BrusherProfile brusherProfile = await _context.BrusherProfile.SingleOrDefaultAsync(m => m.UserId == id);
            if (brusherProfile == null)
            {
                brusherProfile = new BrusherProfile { };
            }
            return View(brusherProfile);
        }

        // POST: BrusherProfile/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BrusherProfile brusherProfile)
        {
            if (string.IsNullOrEmpty(brusherProfile.UserId))
            {
                // a creation
                brusherProfile.UserId = User.GetUserId();
                if (ModelState.IsValid)
                {
                    _context.BrusherProfile.Add(brusherProfile);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            } 
            else if (ModelState.IsValid)
            {
                _context.Update(brusherProfile);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(brusherProfile);
        }

        // GET: BrusherProfile/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            BrusherProfile brusherProfile = await _context.BrusherProfile.SingleAsync(m => m.UserId == id);
            if (brusherProfile == null)
            {
                return HttpNotFound();
            }

            return View(brusherProfile);
        }

        // POST: BrusherProfile/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            BrusherProfile brusherProfile = await _context.BrusherProfile.SingleAsync(m => m.UserId == id);
            _context.BrusherProfile.Remove(brusherProfile);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
