using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Yavsc.Controllers
{
    using Helpers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Localization;
    using Models;
    using ViewModels.FrontOffice;
    using Yavsc.Server.Helpers;
    using Yavsc.Services;

    public class FrontOfficeController : Controller
    {
        readonly ApplicationDbContext _context;
        readonly UserManager<ApplicationUser> _userManager;
        readonly ILogger _logger;
        readonly IStringLocalizer _SR;
        private readonly IBillingService _billing;

        public FrontOfficeController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IBillingService billing,
            ILoggerFactory loggerFactory,
        IStringLocalizer<Yavsc.YavscLocalization> SR)
        {
            _context = context;
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<FrontOfficeController>();
            _SR = SR;
            _billing = billing;
        }
        public ActionResult Index()
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var now = DateTime.Now;

            var model = new FrontOfficeIndexViewModel
            {
                EstimateToProduceCount = _context.RdvQueries.Where(c => c.PerformerId == uid && c.EventDate > now
               && c.ValidationDate == null && !_context.Estimates.Any(e => (e.CommandId == c.Id && e.ProviderValidationDate != null))).Count(),
                EstimateToSignAsProCount = _context.RdvQueries.Where(c => (c.PerformerId == uid && c.EventDate > now
                && c.ValidationDate == null && _context.Estimates.Any(e => (e.CommandId == c.Id && e.ProviderValidationDate != null)))).Count(),
                EstimateToSignAsCliCount = _context.Estimates.Where(e => e.ClientId == uid && e.ClientValidationDate == null).Count(),
                BillToSignAsProCount = 0,
                BillToSignAsCliCount = 0,
                NewPayementsCount = 0
            };
            return View(model);
        }

        [AllowAnonymous]
        public async Task<ActionResult> Profiles(string id)
        {
            if (id == null)
            {
                throw new NotImplementedException("No Activity code");
            }
            ViewBag.Activity = await _context.Activities.FirstOrDefaultAsync(a => a.Code == id);
            var result = await _context.ListPerformersAsync(_billing, id);
            return View(result);
        }

        [AllowAnonymous]
        public async Task <ActionResult> HairCut(string id)
        {
            if (id == null)
            {
                throw new NotImplementedException("No Activity code");
            }
            ViewBag.Activity = await _context.Activities.FirstOrDefaultAsync(a => a.Code == id);
            var result = await _context.ListPerformersAsync(_billing, id);
            return View(result);
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
            ViewBag.TempDir = Config.SiteSetup.TempDir;
            ViewBag.BillsDir = AbstractFileSystemHelpers.UserBillsDirName;
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
