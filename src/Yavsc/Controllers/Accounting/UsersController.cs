using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;
using Yavsc.Models;

namespace Yavsc.Controllers
{
    [Authorize("AdministratorOnly")]
    public class UsersController : Controller
    {
        private ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ApplicationUser.Include(a => a.PostalAddress);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            ApplicationUser applicationUser = await _context.ApplicationUser.SingleAsync(m => m.Id == id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }

            return View(applicationUser);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            ViewData["PostalAddressId"] = new SelectList(_context.Locations, "Id", "PostalAddress");
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUser applicationUser)
        {
            if (ModelState.IsValid)
            {
                _context.ApplicationUser.Add(applicationUser);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["PostalAddressId"] = new SelectList(_context.Locations, "Id", "PostalAddress", applicationUser.PostalAddressId);
            return View(applicationUser);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            ApplicationUser applicationUser = await _context.ApplicationUser.SingleAsync(m => m.Id == id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }
            ViewData["PostalAddressId"] = new SelectList(_context.Locations, "Id", "PostalAddress", applicationUser.PostalAddressId);
            return View(applicationUser);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ApplicationUser applicationUser)
        {
            if (ModelState.IsValid)
            {
                _context.Update(applicationUser);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["PostalAddressId"] = new SelectList(_context.Locations, "Id", "PostalAddress", applicationUser.PostalAddressId);
            return View(applicationUser);
        }

        // GET: Users/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            ApplicationUser applicationUser = await _context.ApplicationUser.SingleAsync(m => m.Id == id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }

            return View(applicationUser);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            ApplicationUser applicationUser = await _context.ApplicationUser.SingleAsync(m => m.Id == id);
            _context.ApplicationUser.Remove(applicationUser);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
