using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Yavsc.Helpers;
using Yavsc.Models;

namespace Yavsc.ApiControllers
{
    [Route("api/payment")]
    public class PaymentApiController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly SiteSettings siteSettings;
        private readonly ILogger _logger;
        public PaymentApiController(
            ApplicationDbContext dbContext,
            IOptions<SiteSettings> siteSettingsReceiver,
            ILoggerFactory loggerFactory)
        {
            this.dbContext = dbContext;
            siteSettings = siteSettingsReceiver.Value;
            _logger = loggerFactory.CreateLogger<PaymentApiController>();
        }

        public async Task<IActionResult> Info(string paymentId, string token)
        {
            var details = await dbContext.GetCheckoutInfo(token);
            _logger.LogInformation(JsonConvert.SerializeObject(details));
            return Ok(details);
        }

    }
}
