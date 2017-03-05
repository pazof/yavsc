using System.IO;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using System.Web.Routing;

namespace Yavsc.ApiControllers
{
    using Models;
    using Helpers;
    using System.Linq;
    using Microsoft.Data.Entity;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Security.Claims;
    using Microsoft.Extensions.Localization;
    using Yavsc.Services;
    using Yavsc.Models.Messaging;
    using Yavsc.ViewModels;
    using Microsoft.Extensions.OptionsModel;

    [Route("api/pdfestimate"), Authorize]
    public class PdfEstimateController : Controller
    {
        ApplicationDbContext dbContext;
        private IStringLocalizer _localizer;
        private GoogleAuthSettings _googleSettings;
        private IGoogleCloudMessageSender _GCMSender;
        private IAuthorizationService authorizationService;

        private ILogger logger;

        public PdfEstimateController(
            IAuthorizationService authorizationService,
            ILoggerFactory loggerFactory,
            IStringLocalizer<Yavsc.Resources.YavscLocalisation> SR,
            ApplicationDbContext context,
            IOptions<GoogleAuthSettings> googleSettings,
            IGoogleCloudMessageSender GCMSender
            )
        {
            _googleSettings=googleSettings.Value;
            this.authorizationService = authorizationService;
            dbContext = context;
            logger = loggerFactory.CreateLogger<PdfEstimateController>();
            this._localizer = SR;
            _GCMSender=GCMSender;
        }

        [HttpGet("get/{id}", Name = "Get"), Authorize]
        public async Task<IActionResult> Get(long id)
        {
            var estimate = dbContext.Estimates.Include(
                e=>e.Query
            ).FirstOrDefault(e=>e.Id == id);
            if (!await authorizationService.AuthorizeAsync(User, estimate, new ViewRequirement()))
            {
                return new ChallengeResult();
            }
 
            var filename = $"estimate-{id}.pdf";

            FileInfo fi = new FileInfo(Path.Combine(Startup.UserBillsDirName, filename));
            if (!fi.Exists) return Ok(new { Error = "Not generated" });
            return File(fi.OpenRead(), "application/x-pdf", filename); ;
        }

        [HttpGet("estimate-{id}.tex", Name = "GetTex"), Authorize]
        public async Task<IActionResult> GetTex(long id)
        {
            var estimate = dbContext.Estimates.Include(
                e=>e.Query
            ).FirstOrDefault(e=>e.Id == id);
            if (!await authorizationService.AuthorizeAsync(User, estimate, new ViewRequirement()))
            {
                return new ChallengeResult();
            }
            Response.ContentType = "text/x-tex";
            return ViewComponent("Estimate",new object[] { id, "LaTeX" });
        }

        [HttpPost("gen/{id}")]
        public async Task<IActionResult> GeneratePdf(long id)
        {
            var estimate = dbContext.Estimates.Include(
                e=>e.Query
            ).FirstOrDefault(e=>e.Id == id);
            if (!await authorizationService.AuthorizeAsync(User, estimate, new ViewRequirement()))
            {
                return new ChallengeResult();
            }
            return ViewComponent("Estimate",new object[] { id, "Pdf" } );
        }


        [HttpPost("prosign/{id}")]
        public async Task<IActionResult> ProSign(long id)
        {
            var estimate = dbContext.Estimates.
            Include(e=>e.Client).Include(e=>e.Client.Devices)
            .Include(e=>e.Bill).Include(e=>e.Owner).Include(e=>e.Owner.Performer)
            .FirstOrDefault(e=>e.Id == id);
            if (estimate == null)
                return new BadRequestResult();
            if (!await authorizationService.AuthorizeAsync(User, estimate, new ViewRequirement()))
            {
                return new ChallengeResult();
            }
            if (Request.Form.Files.Count!=1)
                return new BadRequestResult();
            User.ReceiveProSignature(id,Request.Form.Files[0],"pro");
            estimate.ProviderValidationDate = DateTime.Now;
            dbContext.SaveChanges(User.GetUserId());
            // Notify the client
            var locstr = _localizer["EstimationMessageToClient"];

            var yaev = new EstimationEvent(dbContext,estimate,_localizer);
            
            var regids = estimate.Client.Devices.Select(d => d.GCMRegistrationId).ToArray();
            bool gcmSent = false;
            if (regids.Length>0) {
                var grep = await _GCMSender.NotifyEstimateAsync(_googleSettings,regids,yaev);
                gcmSent = grep.success>0;
            }
            return Ok (new { ProviderValidationDate = estimate.ProviderValidationDate, GCMSent = gcmSent });
        }

        [HttpGet("prosign/{id}")]
        public async Task<IActionResult> GetProSign(long id)
        {
            // For authorization purpose
            var estimate = dbContext.Estimates.FirstOrDefault(e=>e.Id == id);
            if (!await authorizationService.AuthorizeAsync(User, estimate, new ViewRequirement()))
            {
                return new ChallengeResult();
            }

            var filename = FileSystemHelpers.SignFileNameFormat("pro",id);
            FileInfo fi = new FileInfo(Path.Combine(Startup.UserBillsDirName, filename));
            if (!fi.Exists) return HttpNotFound(new { Error = "Professional signature not found" });
            return File(fi.OpenRead(), "application/x-pdf", filename); ;
        }

        [HttpPost("clisign/{id}")]
        public async Task<IActionResult> CliSign(long id)
        {
            var uid = User.GetUserId();
            var estimate = dbContext.Estimates.Include( e=>e.Query
            ).Include(e=>e.Owner).Include(e=>e.Owner.Performer).Include(e=>e.Client)
            .FirstOrDefault( e=> e.Id == id && e.Query.ClientId == uid );
            if (!await authorizationService.AuthorizeAsync(User, estimate, new ViewRequirement()))
            {
                return new ChallengeResult();
            }
            if (Request.Form.Files.Count!=1)
                return new BadRequestResult();
            User.ReceiveProSignature(id,Request.Form.Files[0],"cli");
            estimate.ClientValidationDate = DateTime.Now;
            dbContext.SaveChanges(User.GetUserId());
            return Ok (new { ClientValidationDate = estimate.ClientValidationDate });
        }

        [HttpGet("clisign/{id}")]
        public async Task<IActionResult> GetCliSign(long id)
        {
            // For authorization purpose
            var estimate = dbContext.Estimates.FirstOrDefault(e=>e.Id == id);
            if (!await authorizationService.AuthorizeAsync(User, estimate, new ViewRequirement()))
            {
                return new ChallengeResult();
            }
            
            var filename = FileSystemHelpers.SignFileNameFormat("pro",id);
            FileInfo fi = new FileInfo(Path.Combine(Startup.UserBillsDirName, filename));
            if (!fi.Exists) return HttpNotFound(new { Error = "Professional signature not found" });
            return File(fi.OpenRead(), "application/x-pdf", filename); ;
        }
    }
}