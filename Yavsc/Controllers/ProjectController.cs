using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Server.Models.IT;
using Microsoft.AspNet.Authorization;

namespace Yavsc.Controllers
{
    [Authorize("AdministratorOnly")]
    public class ProjectController : Controller
    {
        private ApplicationDbContext _context;

        public ProjectController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Project
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Projects.Include(p => p.Client).Include(p => p.Context).Include(p => p.PerformerProfile).Include(p => p.Regularisation).Include(p => p.Repository);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Project/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Project project = await _context.Projects.SingleAsync(m => m.Id == id);
            if (project == null)
            {
                return HttpNotFound();
            }

            return View(project);
        }

        // GET: Project/Create
        public IActionResult Create()
        {
            ViewData["ClientId"] = new SelectList(_context.ApplicationUser, "Id", "Client");
            ViewData["ActivityCode"] = new SelectList(_context.Activities, "Code", "Context");
            ViewData["PerformerId"] = new SelectList(_context.Performers, "PerformerId", "PerformerProfile");
            ViewData["PaymentId"] = new SelectList(_context.PayPalPayments, "CreationToken", "Regularisation");
            ViewData["Name"] = new SelectList(_context.GitRepositoryReference, "Path", "Repository");
            return View();
        }

        // POST: Project/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project)
        {
            if (ModelState.IsValid)
            {
                _context.Projects.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["ClientId"] = new SelectList(_context.ApplicationUser, "Id", "Client", project.ClientId);
            ViewData["ActivityCode"] = new SelectList(_context.Activities, "Code", "Context", project.ActivityCode);
            ViewData["PerformerId"] = new SelectList(_context.Performers, "PerformerId", "PerformerProfile", project.PerformerId);
            ViewData["PaymentId"] = new SelectList(_context.PayPalPayments, "CreationToken", "Regularisation", project.PaymentId);
            ViewData["Name"] = new SelectList(_context.GitRepositoryReference, "Path", "Repository", project.Name);
            return View(project);
        }

        // GET: Project/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Project project = await _context.Projects.SingleAsync(m => m.Id == id);
            if (project == null)
            {
                return HttpNotFound();
            }
            ViewData["ClientId"] = new SelectList(_context.ApplicationUser, "Id", "Client", project.ClientId);
            ViewData["ActivityCode"] = new SelectList(_context.Activities, "Code", "Context", project.ActivityCode);
            ViewData["PerformerId"] = new SelectList(_context.Performers, "PerformerId", "PerformerProfile", project.PerformerId);
            ViewData["PaymentId"] = new SelectList(_context.PayPalPayments, "CreationToken", "Regularisation", project.PaymentId);
            ViewData["Name"] = new SelectList(_context.GitRepositoryReference, "Path", "Repository", project.Name);
            return View(project);
        }

        // POST: Project/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Project project)
        {
            if (ModelState.IsValid)
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["ClientId"] = new SelectList(_context.ApplicationUser, "Id", "Client", project.ClientId);
            ViewData["ActivityCode"] = new SelectList(_context.Activities, "Code", "Context", project.ActivityCode);
            ViewData["PerformerId"] = new SelectList(_context.Performers, "PerformerId", "PerformerProfile", project.PerformerId);
            ViewData["PaymentId"] = new SelectList(_context.PayPalPayments, "CreationToken", "Regularisation", project.PaymentId);
            ViewData["Name"] = new SelectList(_context.GitRepositoryReference, "Path", "Repository", project.Name);
            return View(project);
        }

        // GET: Project/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Project project = await _context.Projects.SingleAsync(m => m.Id == id);
            if (project == null)
            {
                return HttpNotFound();
            }

            return View(project);
        }

        // POST: Project/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            Project project = await _context.Projects.SingleAsync(m => m.Id == id);
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
