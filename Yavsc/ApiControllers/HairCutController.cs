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
    using Yavsc.Models.Haircut;
    using Yavsc.Resources;

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

        public HairCutController(ApplicationDbContext context,
        IOptions<GoogleAuthSettings> googleSettings,
        IGoogleCloudMessageSender GCMSender,
          UserManager<ApplicationUser> userManager,
          IStringLocalizer<Yavsc.Resources.YavscLocalisation> localizer,
          IEmailSender emailSender,
          IOptions<SmtpSettings> smtpSettings,
          IOptions<SiteSettings> siteSettings,
          ILoggerFactory loggerFactory)
        {
            _context = context;
            _GCMSender = GCMSender;
            _emailSender = emailSender;
            _googleSettings = googleSettings.Value;
            _userManager = userManager;
            _smtpSettings = smtpSettings.Value;
            _siteSettings = siteSettings.Value;
            _localizer = localizer;
            _logger = loggerFactory.CreateLogger<HairCutController>();
        }
        public IActionResult Index()
        {
            var uid = User.GetUserId();
            var now = DateTime.Now;
            var result = _context.HairCutQueries.Where(
                q=>q.ClientId == uid
                && q.EventDate > now
                && q.Status == QueryStatus.Inserted
                );
            return Ok(result);
        }

        [HttpPost]
        public IActionResult PostQuery (HairCutQuery query )
        {
            var uid = User.GetUserId();
            if (!ModelState.IsValid) {
                return new BadRequestObjectResult(ModelState);
            }
            _context.HairCutQueries.Add(query);
            _context.Update(uid);
            return Ok();
        }

    }
}
