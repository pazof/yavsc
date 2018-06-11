using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Server.Models.IT.SourceCode;

namespace Yavsc.Controllers
{
    [Authorize("AdministratorOnly")]
    public class GitController : Controller
    {
        private ApplicationDbContext _context;

        public GitController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Git
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.GitRepositoryReference.Include(g => g.Owner);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Git/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            GitRepositoryReference gitRepositoryReference = await _context.GitRepositoryReference.SingleAsync(m => m.Path == id);
            if (gitRepositoryReference == null)
            {
                return HttpNotFound();
            }

            return View(gitRepositoryReference);
        }

        // GET: Git/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Git/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GitRepositoryReference gitRepositoryReference)
        {
            gitRepositoryReference.OwnerId = User.GetUserId();
            if (ModelState.IsValid)
            {
                _context.GitRepositoryReference.Add(gitRepositoryReference);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["OwnerId"] = new SelectList(_context.ApplicationUser, "Id", "Owner", gitRepositoryReference.OwnerId);
            return View(gitRepositoryReference);
        }

        // GET: Git/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            GitRepositoryReference gitRepositoryReference = await _context.GitRepositoryReference.SingleAsync(m => m.Path == id);
            if (gitRepositoryReference == null)
            {
                return HttpNotFound();
            }
            ViewData["OwnerId"] = new SelectList(_context.ApplicationUser, "Id", "Owner", gitRepositoryReference.OwnerId);
            return View(gitRepositoryReference);
        }

        // POST: Git/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GitRepositoryReference gitRepositoryReference)
        {
            if (ModelState.IsValid)
            {
                _context.Update(gitRepositoryReference);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["OwnerId"] = new SelectList(_context.ApplicationUser, "Id", "Owner", gitRepositoryReference.OwnerId);
            return View(gitRepositoryReference);
        }

        // GET: Git/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            GitRepositoryReference gitRepositoryReference = await _context.GitRepositoryReference.SingleAsync(m => m.Path == id);
            if (gitRepositoryReference == null)
            {
                return HttpNotFound();
            }

            return View(gitRepositoryReference);
        }

        // POST: Git/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            GitRepositoryReference gitRepositoryReference = await _context.GitRepositoryReference.SingleAsync(m => m.Path == id);
            _context.GitRepositoryReference.Remove(gitRepositoryReference);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
