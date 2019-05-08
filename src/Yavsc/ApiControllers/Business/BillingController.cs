using System.IO;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using System.Web.Routing;
    using System.Linq;
    using Microsoft.Data.Entity;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.OptionsModel;
    using System;
    using System.Security.Claims;

namespace Yavsc.ApiControllers
{
    using Models;
    using Helpers;
    using Services;

    using Models.Messaging;
    using ViewModels.Auth;
    using Newtonsoft.Json;
    using Yavsc.ViewModels;
    using Yavsc.Abstract.FileSystem;

    [Route("api/bill"), Authorize]
    public class BillingController : Controller
    {
        ApplicationDbContext dbContext;
        private IStringLocalizer _localizer;
        private GoogleAuthSettings _googleSettings;
        private IYavscMessageSender _GCMSender;
        private IAuthorizationService authorizationService;


        private ILogger logger;
        private IBillingService billingService;

        public BillingController(
            IAuthorizationService authorizationService,
            ILoggerFactory loggerFactory,
            IStringLocalizer<Yavsc.Resources.YavscLocalisation> SR,
            ApplicationDbContext context,
            IOptions<GoogleAuthSettings> googleSettings,
            IYavscMessageSender GCMSender,
            IBillingService billingService
            )
        {
            _googleSettings=googleSettings.Value;
            this.authorizationService = authorizationService;
            dbContext = context;
            logger = loggerFactory.CreateLogger<BillingController>();
            this._localizer = SR;
            _GCMSender=GCMSender;
            this.billingService=billingService;
        }

        [HttpGet("facture-{billingCode}-{id}.pdf"), Authorize]
        public async Task<IActionResult> GetPdf(string billingCode, long id)
        {     
            var bill = await billingService.GetBillAsync(billingCode, id);

            if (!await authorizationService.AuthorizeAsync(User, bill, new ViewRequirement()))
            {
                return new ChallengeResult();
            }
 
            var fi = bill.GetBillInfo(billingService);

            if (!fi.Exists) return Ok(new { Error = "Not generated" });
            return File(fi.OpenRead(), "application/x-pdf", fi.Name); 
        }

        [HttpGet("facture-{billingCode}-{id}.tex"), Authorize]
        public async Task<IActionResult> GetTex(string billingCode, long id)
        {
            var bill = await billingService.GetBillAsync(billingCode, id);

            if (bill==null) {
               logger.LogCritical ( $"# not found !! {id} in {billingCode}");
               return this.HttpNotFound();
            }
            logger.LogVerbose(JsonConvert.SerializeObject(bill));

            if (!await authorizationService.AuthorizeAsync(User, bill, new ViewRequirement()))
            {
                return new ChallengeResult();
            }
            Response.ContentType = "text/x-tex";
            return ViewComponent("Bill",new object[] {  billingCode, bill , OutputFormat.LaTeX, true });
        }

        [HttpPost("genpdf/{billingCode}/{id}")]
        public async Task<IActionResult> GeneratePdf(string billingCode, long id)
        {
            var bill = await billingService.GetBillAsync(billingCode, id);
           
            if (bill==null) {
               logger.LogCritical ( $"# not found !! {id} in {billingCode}");
               return this.HttpNotFound();
            }
             logger.LogWarning("Got bill ack:"+bill.GetIsAcquitted().ToString());
            return ViewComponent("Bill",new object[] { billingCode, bill, OutputFormat.Pdf, true } );
        }


        [HttpPost("prosign/{billingCode}/{id}")]
        public async Task<IActionResult> ProSign(string billingCode, long id)
        {
            var estimate = dbContext.Estimates.
            Include(e=>e.Client).Include(e=>e.Client.DeviceDeclarations)
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
            User.ReceiveProSignature(billingCode,id,Request.Form.Files[0],"pro");
            estimate.ProviderValidationDate = DateTime.Now;
            dbContext.SaveChanges(User.GetUserId());
            // Notify the client
            var locstr = _localizer["EstimationMessageToClient"];

            var yaev = new EstimationEvent(estimate,_localizer);
            
            var regids = estimate.Client.DeviceDeclarations.Select(d => d.DeviceId).ToArray();
            bool gcmSent = false;
            if (regids.Length>0) {
                var grep = await _GCMSender.NotifyEstimateAsync(regids,yaev);
                gcmSent = grep.success>0;
            }
            return Ok (new { ProviderValidationDate = estimate.ProviderValidationDate, GCMSent = gcmSent });
        }

        [HttpGet("prosign/{billingCode}/{id}")]
        public async Task<IActionResult> GetProSign(string billingCode, long id)
        {
            // For authorization purpose
            var estimate = dbContext.Estimates.FirstOrDefault(e=>e.Id == id);
            if (!await authorizationService.AuthorizeAsync(User, estimate, new ViewRequirement()))
            {
                return new ChallengeResult();
            }

            var filename = FileSystemHelpers.SignFileNameFormat("pro",billingCode,id);
            FileInfo fi = new FileInfo(Path.Combine(AbstractFileSystemHelpers.UserBillsDirName, filename));
            if (!fi.Exists) return HttpNotFound(new { Error = "Professional signature not found" });
            return File(fi.OpenRead(), "application/x-pdf", filename); ;
        }

        [HttpPost("clisign/{billingCode}/{id}")]
        public async Task<IActionResult> CliSign(string billingCode, long id)
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
            User.ReceiveProSignature(billingCode,id,Request.Form.Files[0],"cli");
            estimate.ClientValidationDate = DateTime.Now;
            dbContext.SaveChanges(User.GetUserId());
            return Ok (new { ClientValidationDate = estimate.ClientValidationDate });
        }

        [HttpGet("clisign/{billingCode}/{id}")]
        public async Task<IActionResult> GetCliSign(string billingCode, long id)
        {
            // For authorization purpose
            var estimate = dbContext.Estimates.FirstOrDefault(e=>e.Id == id);
            if (!await authorizationService.AuthorizeAsync(User, estimate, new ViewRequirement()))
            {
                return new ChallengeResult();
            }
            
            var filename = FileSystemHelpers.SignFileNameFormat("pro",billingCode,id);
            FileInfo fi = new FileInfo(Path.Combine(AbstractFileSystemHelpers.UserBillsDirName, filename));
            if (!fi.Exists) return HttpNotFound(new { Error = "Professional signature not found" });
            return File(fi.OpenRead(), "application/x-pdf", filename); ;
        }
    }
}
