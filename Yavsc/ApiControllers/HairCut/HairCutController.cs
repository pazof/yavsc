using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.Localization;

namespace Yavsc.ApiControllers
{
    using YavscLib;
    using System;
    using System.Linq;
    using System.Security.Claims;
    using Microsoft.Extensions.Logging;
    using Models;
    using Services;
    using Models.Haircut;
    using Resources;
    using System.Threading.Tasks;
    using Helpers;
    using Microsoft.Data.Entity;
    using Models.Payment;
    using Models.Relationship;

    [Route("api/haircut")]
    public class HairCutController : Controller
    {
        private ApplicationDbContext _context;
        private IEmailSender _emailSender;
        private IGoogleCloudMessageSender _GCMSender;
        private GoogleAuthSettings _googleSettings;
        private IStringLocalizer<YavscLocalisation> _localizer;
        private ILogger _logger;
        private SiteSettings _siteSettings;
        private SmtpSettings _smtpSettings;
        private UserManager<ApplicationUser> _userManager;

        PayPalSettings _paymentSettings;
        public HairCutController(ApplicationDbContext context,
        IOptions<GoogleAuthSettings> googleSettings,
        IGoogleCloudMessageSender GCMSender,
          UserManager<ApplicationUser> userManager,
          IStringLocalizer<Yavsc.Resources.YavscLocalisation> localizer,
          IEmailSender emailSender,
          IOptions<SmtpSettings> smtpSettings,
          IOptions<SiteSettings> siteSettings,
          IOptions<PayPalSettings> payPalSettings,
          ILoggerFactory loggerFactory)
        {
            _context = context;
            _GCMSender = GCMSender;
            _emailSender = emailSender;
            _googleSettings = googleSettings.Value;
            _userManager = userManager;
            _smtpSettings = smtpSettings.Value;
            _siteSettings = siteSettings.Value;
            _paymentSettings = payPalSettings.Value;
            _localizer = localizer;
            _logger = loggerFactory.CreateLogger<HairCutController>();
        }

        // GET: api/HairCutQueriesApi
        // Get the active queries for current
        // user, as a client
        public IActionResult Index()
        {
            var uid = User.GetUserId();
            var now = DateTime.Now;
            var result = _context.HairCutQueries.Where(
                q => q.ClientId == uid
                && q.EventDate > now
                && q.Status == QueryStatus.Inserted
                );
            return Ok(result);
        }

        [HttpPost]
        public IActionResult PostQuery(HairCutQuery query)
        {
            var uid = User.GetUserId();
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }
            _context.HairCutQueries.Add(query);
            _context.Update(uid);
            return Ok();
        }

        [HttpPost("createpayment/{id}")]
        public async Task<IActionResult> CreatePayment(long id)
        {
            var apiContext = PayPalHelpers.CreateAPIContext();

            var query = await _context.HairCutQueries.Include(q => q.Client).
            Include(q => q.Client.PostalAddress).Include(q => q.Prestation).Include(q=>q.Regularisation)
            .SingleAsync(q => q.Id == id);
            if (query.PaymentId!=null)
                return new BadRequestObjectResult(new { error = "An existing payment process already exists" });
            query.SelectedProfile = _context.BrusherProfile.Single(p => p.UserId == query.PerformerId);

            var payment = Request.CreatePayment("HairCutCommand",apiContext, query,  "sale", _logger);
            switch (payment.state)
            {
                case "created":
                case "approved":
                    {
                        var dbinfo = new PaypalPayment
                        {
                            ExecutorId = User.GetUserId(),
                            PaypalPayerId = payment.payer.payer_info?.payer_id,
                            PaypalPaymentId = payment.id,
                            State = payment.state
                        };
                        var links = payment.links.Select
                        (
                            l=> new Link {
                            HRef = l.href,
                            Rel = l.rel,
                            Method = l.method
                            });
                        _context.PaypalPayments.Add(dbinfo);
                        query.PaymentId = payment.id;
                        await _context.SaveChangesAsync(User.GetUserId());
                    }
                    break;
                case "failed":
                    return new BadRequestObjectResult(payment);
                default: throw new NotImplementedException();
            }

            return Json(payment);
        }
    }
}
