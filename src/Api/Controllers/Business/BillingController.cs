using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Security.Claims;
using Yavsc.Helpers;
using Yavsc.ViewModels;

namespace Yavsc.ApiControllers
{
    using Models;
    using Services;

    using Models.Messaging;
    using Microsoft.Extensions.Options;
    using Microsoft.EntityFrameworkCore;
    using Yavsc.ViewModels.Auth;
    using Yavsc.Server.Helpers;

    [Route("api/bill"), Authorize]
    public class BillingController : Controller
    {
        readonly ApplicationDbContext dbContext;
        private readonly IStringLocalizer _localizer;
        private readonly GoogleAuthSettings _googleSettings;
        private readonly IYavscMessageSender _GCMSender;
        private readonly IAuthorizationService authorizationService;


        private readonly ILogger logger;
        private readonly IBillingService billingService;

        public BillingController(
            IAuthorizationService authorizationService,
            ILoggerFactory loggerFactory,
            IStringLocalizer<Yavsc.YavscLocalization> SR,
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

            if ( authorizationService.AuthorizeAsync(User, bill, new ReadPermission()).IsFaulted)
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
               return this.NotFound();
            }
            logger.LogTrace(JsonConvert.SerializeObject(bill));

            if (!(await authorizationService.AuthorizeAsync(User, bill, new ReadPermission())).Succeeded)
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
               return this.NotFound();
            }
             logger.LogWarning("Got bill ack:"+bill.GetIsAcquitted().ToString());
            return ViewComponent("Bill",new object[] { billingCode, bill, OutputFormat.Pdf, true } );
        }


        [HttpPost("prosign/{billingCode}/{id}")]
        public async Task<IActionResult> ProSign(string billingCode, long id)
        {
            var estimate = dbContext.Estimates.
            Include(e=>e.Client).Include(e=>e.Client.DeviceDeclaration)
            .Include(e=>e.Bill).Include(e=>e.Owner).Include(e=>e.Owner.Performer)
            .FirstOrDefault(e=>e.Id == id);
            if (estimate == null)
                return new BadRequestResult();
            if (!(await authorizationService.AuthorizeAsync(User, estimate, new ReadPermission())).Succeeded)

          
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
            
            var regids = new [] { estimate.Client.Id };
            bool gcmSent = false;
                var grep = await _GCMSender.NotifyEstimateAsync(regids,yaev);
                gcmSent = grep.success>0;
            return Ok (new { ProviderValidationDate = estimate.ProviderValidationDate, GCMSent = gcmSent });
        }

        [HttpGet("prosign/{billingCode}/{id}")]
        public async Task<IActionResult> GetProSign(string billingCode, long id)
        {
            // For authorization purpose
            var estimate = dbContext.Estimates.FirstOrDefault(e=>e.Id == id);
            if (!(await authorizationService.AuthorizeAsync(User, estimate, new ReadPermission())).Succeeded)
            
            {
                return new ChallengeResult();
            }

            var filename = AbstractFileSystemHelpers.SignFileNameFormat("pro", billingCode, id);
            FileInfo fi = new FileInfo(Path.Combine(AbstractFileSystemHelpers.UserBillsDirName, filename));
            if (!fi.Exists) return NotFound(new { Error = "Professional signature not found" });
            return File(fi.OpenRead(), "application/x-pdf", filename); ;
        }

        [HttpPost("clisign/{billingCode}/{id}")]
        public async Task<IActionResult> CliSign(string billingCode, long id)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var estimate = dbContext.Estimates.Include( e=>e.Query
            ).Include(e=>e.Owner).Include(e=>e.Owner.Performer).Include(e=>e.Client)
            .FirstOrDefault( e=> e.Id == id && e.Query.ClientId == uid );
            if (!(await authorizationService.AuthorizeAsync(User, estimate, new ReadPermission())).Succeeded)
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
            if (!(await authorizationService.AuthorizeAsync(User, estimate, new ReadPermission())).Succeeded)
            {
                return new ChallengeResult();
            }
            
            var filename = AbstractFileSystemHelpers.SignFileNameFormat("pro", billingCode, id);
            FileInfo fi = new FileInfo(Path.Combine(AbstractFileSystemHelpers.UserBillsDirName, filename));
            if (!fi.Exists) return NotFound(new { Error = "Professional signature not found" });
            return File(fi.OpenRead(), "application/x-pdf", filename); ;
        }
    }
}
