
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Models;
using IdentityServer8.EntityFramework.Entities;
namespace Yavsc.Org.Controllers
{
    public class ApiScopeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ApiScopeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ApiScope
        public async Task<IActionResult> Index()
        {
            return View(await _context.ApiScopes.ToListAsync());
        }

        // GET: ApiScope/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apiScope = await _context.ApiScopes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (apiScope == null)
            {
                return NotFound();
            }

            return View(apiScope);
        }

        // GET: ApiScope/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ApiScope/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApiScope apiScope)
        {
            if (ModelState.IsValid)
            {
                _context.Add(apiScope);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(apiScope);
        }

        // GET: ApiScope/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apiScope = await _context.ApiScopes.FindAsync(id);
            if (apiScope == null)
            {
                return NotFound();
            }
            return View(apiScope);
        }

        // POST: ApiScope/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
        ApiScope apiScope)
        {
            if (id != apiScope.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(apiScope);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApiScopeExists(apiScope.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(apiScope);
        }

        // GET: ApiScope/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apiScope = await _context.ApiScopes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (apiScope == null)
            {
                return NotFound();
            }

            return View(apiScope);
        }

        // POST: ApiScope/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var apiScope = await _context.ApiScopes.FindAsync(id);
            if (apiScope != null)
            {
                _context.ApiScopes.Remove(apiScope);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ApiScopeExists(int id)
        {
            return _context.ApiScopes.Any(e => e.Id == id);
        }
    }
}
