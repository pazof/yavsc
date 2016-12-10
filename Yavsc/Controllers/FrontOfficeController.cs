using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;
using Yavsc.Models;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Logging;
using Yavsc.Models.Booking;
using Yavsc.Helpers;
using System;

namespace Yavsc.Controllers
{
    [ServiceFilter(typeof(LanguageActionFilter)),
    Route("do")]
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
            var latestPosts = _context.Blogspot.Where(
                x => x.Visible == true
            ).OrderBy(x => x.Modified).Take(25).ToArray();
            return View(latestPosts);
        }

        [Route("Book/{id?}"), HttpGet]
        public ActionResult Book(string id)
        {
            if (id == null)
            {
                throw new NotImplementedException("No Activity code");
            }

            ViewBag.Activities = _context.ActivityItems(id);
            ViewBag.Activity = _context.Activities.FirstOrDefault(
                a => a.Code == id);

            return View(
                _context.Performers.Include(p => p.Performer)
                .Include(p=>p.Performer.Devices).Where
                (p => p.ActivityCode == id && p.Active).OrderBy(
                    x => x.MinDailyCost
                )
            );
        }

        [Route("Book/{id}"), HttpPost]
        public ActionResult Book(BookQuery bookQuery)
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
            return View(_context.Performers.Include(p => p.Performer).Where
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

        [Authorize,Route("Estimate-{id}.pdf")]
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
            if (estimate==null)
                throw new Exception("No data");
            return View("Estimate.pdf",estimate);
        }
    } 
}
