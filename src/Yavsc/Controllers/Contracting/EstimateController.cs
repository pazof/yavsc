using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Microsoft.Extensions.OptionsModel;
 using Yavsc.Helpers;

namespace Yavsc.Controllers
{
    using Models;
    using Models.Billing;
    using Models.Workflow;
    using ViewModels.Auth;

    [Authorize]
    public class EstimateController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SiteSettings _site;
        readonly IAuthorizationService authorizationService;

        public EstimateController(ApplicationDbContext context, IAuthorizationService authorizationService, IOptions<SiteSettings> siteSettings)
        {
            _context = context;
            _site = siteSettings.Value;
            this.authorizationService = authorizationService;
        }

        // GET: Estimate
        
        public IActionResult Index()
        {
            var uid = User.GetUserId();
            return View(_context.Estimates.Include(e=>e.Query)
            .Include(e=>e.Query.PerformerProfile)
            .Include(e=>e.Query.PerformerProfile.Performer)
            .Where(
                e=>e.OwnerId == uid || e.ClientId == uid
            ).OrderByDescending(e=>e.ProviderValidationDate)
            .ToList());
        }

        // GET: Estimate/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            var uid = User.GetUserId();
            if (id == null)
            {
                return HttpNotFound();
            }

            Estimate estimate = _context.Estimates
            .Include(e => e.Query)
            .Include(e => e.Query.PerformerProfile)
            .Include(e => e.Query.PerformerProfile.Performer)
            .Include(e=> e.Bill)
            .Where(
                e=>e.OwnerId == uid || e.ClientId == uid
            )
            .Single(m => m.Id == id);
            if (estimate == null)
            {
                return HttpNotFound();
            }
            if (!await authorizationService.AuthorizeAsync(User, estimate, new ViewRequirement()))
            {
                return new ChallengeResult();
            }
            return View(estimate);
        }


        // GET: Estimate/Create
        [Authorize]
        public IActionResult Create()
        {
            var uid = User.GetUserId();
            IQueryable<RdvQuery> queries = _context.RdvQueries.Include(q=>q.Location).Where(bq=>bq.PerformerId == uid);
            //.Select(bq=>new SelectListItem{ Text = bq.Client.UserName, Value = bq.Client.Id });
            ViewBag.Clients = queries.Select(q=>q.Client).Distinct();
            ViewBag.Queries = queries;

            return View();
        }

        // POST: Estimate/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Estimate estimate,
         ICollection<IFormFile> newGraphics,
         ICollection<IFormFile> newFiles
         )
        {
            estimate.OwnerId = User.GetUserId();
            if (ModelState.IsValid)
            {
                _context.Estimates
                .Add(estimate);
                _context.SaveChanges(User.GetUserId());
                var query = _context.RdvQueries.FirstOrDefault(
                    q=>q.Id == estimate.CommandId
                );
                var perfomerProfile = _context.Performers
                .Include(
                    perpr => perpr.Performer).FirstOrDefault(
                    x=>x.PerformerId == query.PerformerId
                );
                var command = _context.RdvQueries.FirstOrDefault(
                    cmd => cmd.Id == estimate.CommandId
                );

                var billsdir = Path.Combine(
                    _site.Bills,
                    perfomerProfile.Performer.UserName
               );

                foreach (var gr in newGraphics)
                {
                    ContentDisposition contentDisposition = new ContentDisposition(gr.ContentDisposition);
                    gr.SaveAs(
                        Path.Combine(
                        Path.Combine(billsdir, estimate.Id.ToString()),
                        contentDisposition.FileName));
                }
                foreach (var formFile in newFiles)
                {
                    ContentDisposition contentDisposition = new ContentDisposition(formFile.ContentDisposition);
                    formFile.SaveAs(
                        Path.Combine(
                        Path.Combine(billsdir, estimate.Id.ToString()),
                        contentDisposition.FileName));
                }
                return RedirectToAction("Index");
            }
            return View(estimate);
        }


        // GET: Estimate/Edit/5
        public IActionResult Edit(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            var uid = User.GetUserId();

            Estimate estimate = _context.Estimates
            .Where(e=>e.OwnerId==uid||e.ClientId==uid).Single(m => m.Id == id);
            if (estimate == null)
            {
                return HttpNotFound();
            }
            
            ViewBag.Files =  Yavsc.Helpers.FileSystemHelpers.GetFileName(null);
            
            // Yavsc.Helpers.GetUserFiles(User, null);

            return View(estimate);
        }

        // POST: Estimate/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Estimate estimate)
        {
            var uid = User.GetUserId();
            if (estimate.OwnerId!=uid&&estimate.ClientId!=uid
            ) return new HttpNotFoundResult();
            if (ModelState.IsValid)
            {
                _context.Update(estimate);
                _context.SaveChanges(User.GetUserId());
                return RedirectToAction("Index");
            }
            return View(estimate);
        }

        // GET: Estimate/Delete/5
        [ActionName("Delete")]
        public IActionResult Delete(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            var uid = User.GetUserId();

            Estimate estimate = _context.Estimates
            .Where(e=>e.OwnerId==uid||e.ClientId==uid) .Single(m => m.Id == id);
            if (estimate == null)
            {
                return HttpNotFound();
            }

            return View(estimate);
        }

        // POST: Estimate/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(long id)
        {
            Estimate estimate = _context.Estimates.Single(m => m.Id == id);
            _context.Estimates.Remove(estimate);
            _context.SaveChanges(User.GetUserId());
            return RedirectToAction("Index");
        }
    }
}
