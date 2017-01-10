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
         ApplicationDbContext context)
        {
            this.authorizationService = authorizationService;
            dbContext = context;
            logger = loggerFactory.CreateLogger<PdfEstimateController>();
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
            var uid = User.GetUserId();
            var estimate = dbContext.Estimates.Include(
                e=>e.Query
            ).FirstOrDefault(e=>e.Id == id && e.OwnerId == uid );
            if (!await authorizationService.AuthorizeAsync(User, estimate, new ViewRequirement()))
            {
                return new ChallengeResult();
            }
            if (Request.Form.Files.Count!=1)
                return new BadRequestResult();
            User.ReceiveProSignature(id,Request.Form.Files[0],"pro");
            estimate.ProviderValidationDate = DateTime.Now;
            dbContext.SaveChanges();
            // Notify the client
            var yaev = new EstimationEvent(dbContext,estimate,_localizer);
            var regids = estimate.Client.Devices.Select(d => d.GCMRegistrationId);
            var grep = await _GCMSender.NotifyEstimateAsync(_googleSettings,regids,yaev);
            return Ok (new { ProviderValidationDate = estimate.ProviderValidationDate, GCMSent = grep.success });
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
            ).FirstOrDefault( e=> e.Id == id && e.Query.ClientId == uid );
            if (!await authorizationService.AuthorizeAsync(User, estimate, new ViewRequirement()))
            {
                return new ChallengeResult();
            }
            if (Request.Form.Files.Count!=1)
                return new BadRequestResult();
            User.ReceiveProSignature(id,Request.Form.Files[0],"cli");
            estimate.ClientValidationDate = DateTime.Now;
            dbContext.SaveChanges();
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