using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Logging;
using Yavsc.Models;
using Yavsc.Server.Models.IT;
using Microsoft.AspNet.Authorization;
using Yavsc.Server.Helpers;
using Yavsc.Models.Workflow;
using Yavsc.Models.Payment;
using Yavsc.Server.Models.IT.SourceCode;
using Microsoft.Extensions.Localization;

namespace Yavsc.Controllers
{
    [Authorize("AdministratorOnly")]
    public class ProjectController : Controller
    {
        private ApplicationDbContext _context;
        ILogger _logger;
        IStringLocalizer<Yavsc.YavscLocalisation> _localizer;
        IStringLocalizer<BugController> _bugLocalizer;
          
        public ProjectController(ApplicationDbContext context,
        ILoggerFactory loggerFactory,
        IStringLocalizer<Yavsc.YavscLocalisation> localizer,
        IStringLocalizer<BugController> bugLocalizer
        )
        {
            _context = context;
            _localizer = localizer;
            _bugLocalizer = bugLocalizer;
            _logger = loggerFactory.CreateLogger<ProjectController>();

        }

        // GET: Project
        public async Task<IActionResult> Index()
        {

            var applicationDbContext = _context.Project.Include(p => p.Client).Include(p => p.Context).Include(p => p.PerformerProfile).Include(p => p.Regularisation).Include(p => p.Repository);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Project/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Project project = await _context.Project.SingleAsync(m => m.Id == id);
            if (project == null)
            {
                return HttpNotFound();
            }

            return View(project);
        }

        // GET: Project/Create
        public IActionResult Create()
        {
            ViewBag.ClientIdItems = _context.ApplicationUser.CreateSelectListItems<ApplicationUser>(
                u => u.Id, u => u.UserName);
            ViewBag.OwnerIdItems = _context.ApplicationUser.CreateSelectListItems<ApplicationUser>(
                u => u.Id, u => u.UserName);
            ViewBag.ActivityCodeItems = _context.Activities.CreateSelectListItems<Activity>(
                a => a.Code, a => a.Name);
            ViewBag.PerformerIdItems = _context.Performers.Include(p=>p.Performer).CreateSelectListItems<PerformerProfile>(p => p.PerformerId, p => p.Performer.UserName);
            ViewBag.PaymentIdItems = _context.PayPalPayment.CreateSelectListItems<PayPalPayment>
            (p => p.OrderReference, p => $"{p.Executor.UserName} {p.PaypalPayerId} {p.OrderReference}");

            ViewBag.Status = _bugLocalizer.CreateSelectListItems(typeof(Yavsc.QueryStatus), Yavsc.QueryStatus.Inserted);
            ViewBag.RepositoryItems = _context.GitRepositoryReference.CreateSelectListItems<GitRepositoryReference>(
                u => u.Id.ToString(), u => u.ToString());

            return View();
        }

        // POST: Project/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project)
        {
            if (ModelState.IsValid)
            {
                _context.Project.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
           ViewBag.ClientIdItems = _context.ApplicationUser.CreateSelectListItems<ApplicationUser>(
                u => u.Id, u => u.UserName, project.ClientId);
            ViewBag.OwnerIdItems = _context.ApplicationUser.CreateSelectListItems<ApplicationUser>(
                u => u.Id, u => u.UserName, project.OwnerId);
            ViewBag.ActivityCodeItems = _context.Activities.CreateSelectListItems<Activity>(
                a => a.Code, a => a.Name, project.ActivityCode);
            ViewBag.PerformerIdItems = _context.Performers.Include(p=>p.Performer).CreateSelectListItems<PerformerProfile>(p => p.PerformerId, p => p.Performer.UserName, project.PerformerId);
            ViewBag.PaymentIdItems = _context.PayPalPayment.CreateSelectListItems<PayPalPayment>
            (p => p.OrderReference, p => $"{p.Executor.UserName} {p.PaypalPayerId} {p.OrderReference}", project.PaymentId);
            return View(project);
        }

        // GET: Project/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Project project = await _context.Project.SingleAsync(m => m.Id == id);
            if (project == null)
            {
                return HttpNotFound();
            }
         /*   ViewBag.ClientId = new SelectList(_context.ApplicationUser, "Id", "Client", project.ClientId);
            ViewBag.ActivityCodeItems = new SelectList(_context.Activities, "Code", "Context", project.ActivityCode);
            ViewBag.PerformerId = new SelectList(_context.Performers, "PerformerId", "PerformerProfile", project.PerformerId);
            ViewBag.PaymentId = new SelectList(_context.PayPalPayments, "CreationToken", "Regularisation", project.PaymentId);
            ViewBag.Name = new SelectList(_context.GitRepositoryReference, "Path", "Repository", project.Name);
          */
           ViewBag.Status = Yavsc.Extensions.EnumExtensions.GetSelectList(typeof(QueryStatus), _localizer, project.Status);
           ViewBag.Repository = new SelectList(_context.GitRepositoryReference, "Path", "Repository", project.Repository);
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

            Project project = await _context.Project.SingleAsync(m => m.Id == id);
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
            Project project = await _context.Project.SingleAsync(m => m.Id == id);
            _context.Project.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
