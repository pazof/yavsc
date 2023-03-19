using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Models;
using Yavsc.Models.Relationship;

namespace Yavsc.Controllers
{
    [Authorize("AdministratorOnly")]
    public class HyperLinkController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HyperLinkController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: HyperLink
        public async Task<IActionResult> Index()
        {
            return View(await _context.HyperLink.ToListAsync());
        }

        // GET: HyperLink/Details/5
        public async Task<IActionResult> Details(string href, string method)
        {
            if (href == null || method ==null)
            {
                return NotFound();
            }

            HyperLink hyperLink = await _context.HyperLink.SingleAsync(m => m.HRef == href && m.Method == method);
            if (hyperLink == null)
            {
                return NotFound();
            }

            return View(hyperLink);
        }

        // GET: HyperLink/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: HyperLink/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HyperLink hyperLink)
        {
            if (ModelState.IsValid)
            {
                _context.HyperLink.Add(hyperLink);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(hyperLink);
        }

        // GET: HyperLink/Edit/5
        public async Task<IActionResult> Edit(string href, string method)
        {
            if (href == null || method ==null)
            {
                return NotFound();
            }

            HyperLink hyperLink = await _context.HyperLink.SingleAsync(m => m.HRef == href && m.Method == method);
            if (hyperLink == null)
            {
                return NotFound();
            }
            return View(hyperLink);
        }

        // POST: HyperLink/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(HyperLink hyperLink)
        {
            if (ModelState.IsValid)
            {
                _context.Update(hyperLink);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(hyperLink);
        }

        // GET: HyperLink/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(string href, string method)
        {
            if (href == null || method ==null)
            {
                return NotFound();
            }

            HyperLink hyperLink = await _context.HyperLink.SingleAsync(m => m.HRef == href && m.Method == method);

            if (hyperLink == null)
            {
                return NotFound();
            }

            return View(hyperLink);
        }

        // POST: HyperLink/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string HRef, string Method)
        {
            if (HRef == null || Method ==null)
            {
                return NotFound();
            }

            HyperLink hyperLink = await _context.HyperLink.SingleAsync(m => m.HRef == HRef && m.Method == Method);

            _context.HyperLink.Remove(hyperLink);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
