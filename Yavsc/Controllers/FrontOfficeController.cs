using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Identity;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;

namespace Yavsc.Controllers
{
    using Helpers;
    using Models;
    using Models.Workflow;
    using ViewModels.FrontOffice;
    public class FrontOfficeController : Controller
    {
        ApplicationDbContext _context;
        UserManager<ApplicationUser> _userManager;

        ILogger _logger;
        public FrontOfficeController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILoggerFactory loggerFactory)
        {
            _context = context;
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<FrontOfficeController>();
        }
        public ActionResult Index()
        {
            var uid = User.GetUserId();
            var now = DateTime.Now;

            var model = new FrontOfficeIndexViewModel
            {
                EstimateToProduceCount = _context.Commands.Where(c => c.PerformerId == uid && c.EventDate > now
               && c.ValidationDate == null && !_context.Estimates.Any(e => (e.CommandId == c.Id && e.ProviderValidationDate != null))).Count(),
                EstimateToSignAsProCount = _context.Commands.Where(c => (c.PerformerId == uid && c.EventDate > now
                && c.ValidationDate == null && _context.Estimates.Any(e => (e.CommandId == c.Id && e.ProviderValidationDate != null)))).Count(),
                EstimateToSignAsCliCount = _context.Estimates.Where(e => e.ClientId == uid && e.ClientValidationDate == null).Count(),
                BillToSignAsProCount = 0,
                BillToSignAsCliCount = 0,
                NewPayementsCount = 0
            };
            return View(model);
        }

        [Route("Profiles/{id?}"), HttpGet, AllowAnonymous]
        public ActionResult Profiles(string id)
        {
            if (id == null)
            {
                throw new NotImplementedException("No Activity code");
            }
            ViewBag.Activity = _context.Activities.FirstOrDefault(a => a.Code == id);
            var result = _context.ListPerformers(id);
            return View(result);
        }

        [Route("Profiles/{id}"), HttpPost, AllowAnonymous]
        public ActionResult Profiles(BookQuery bookQuery)
        {
            if (ModelState.IsValid)
            {
                var pro = _context.Performers.Include(
                    pr => pr.Performer
                ).FirstOrDefault(
                    x => x.PerformerId == bookQuery.PerformerId
                );
                if (pro == null)
                    return HttpNotFound();
                // Let's create a command
                if (bookQuery.Id == 0)
                {
                    _context.BookQueries.Add(bookQuery);
                }
                else
                {
                    _context.BookQueries.Update(bookQuery);
                }
                _context.SaveChanges();
                // TODO Send sys notifications &
                // notify the user (make him a basket badge)
                return View("Index");
            }
            ViewBag.Activities = _context.ActivityItems(null);
            return View("Profiles", _context.Performers.Include(p => p.Performer).Where
                (p => p.Active).OrderBy(
                    x => x.MinDailyCost
                ));
        }

        [Produces("text/x-tex"), Authorize, Route("estimate-{id}.tex")]
        public ViewResult EstimateTex(long id)
        {
            var estimate = _context.Estimates.Include(x => x.Query)
            .Include(x => x.Query.Client)
            .Include(x => x.Query.PerformerProfile)
            .Include(x => x.Query.PerformerProfile.OrganizationAddress)
            .Include(x => x.Query.PerformerProfile.Performer)
            .Include(e => e.Bill).FirstOrDefault(x => x.Id == id);
            Response.ContentType = "text/x-tex";
            return View("Estimate.tex", estimate);
        }

        [Authorize, Route("Estimate-{id}.pdf")]
        public IActionResult EstimatePdf(long id)
        {
            ViewBag.TempDir = Startup.SiteSetup.TempDir;
            ViewBag.BillsDir = Startup.UserBillsDirName;
            var estimate = _context.Estimates.Include(x => x.Query)
            .Include(x => x.Query.Client)
            .Include(x => x.Query.PerformerProfile)
            .Include(x => x.Query.PerformerProfile.OrganizationAddress)
            .Include(x => x.Query.PerformerProfile.Performer)
            .Include(e => e.Bill).FirstOrDefault(x => x.Id == id);
            if (estimate == null)
                throw new Exception("No data");
            return View("Estimate.pdf", estimate);
        }

        [Authorize]
        public IActionResult EstimateProValidation()
        {
            throw new NotImplementedException();

        }

        [Authorize]
        public IActionResult EstimateClientValidation()
        {
            throw new NotImplementedException();

        }
        [Authorize]
        public IActionResult BillValidation()
        {
            throw new NotImplementedException();

        }
        [Authorize]
        public IActionResult BillAcquitment()
        {
            throw new NotImplementedException();
        }
    }
}
