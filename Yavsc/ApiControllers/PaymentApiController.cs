using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using PayPal.Api;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Billing;
using Yavsc.ViewModels.PayPal;

namespace Yavsc.ApiControllers
{
    [Route("api/payment")]
    public class PaymentApiController : Controller
    {
        private ApplicationDbContext dbContext;
        PayPalSettings paymentSettings;
        private SiteSettings siteSettings;
        private readonly ILogger _logger;
        public PaymentApiController(
            ApplicationDbContext dbContext,
            IOptions<PayPalSettings> paypalSettingsReceiver,
            IOptions<SiteSettings> siteSettingsReceiver,
            ILoggerFactory loggerFactory)
        {
            this.dbContext = dbContext;
            paymentSettings = paypalSettingsReceiver.Value;
            siteSettings = siteSettingsReceiver.Value;
            _logger = loggerFactory.CreateLogger<PaymentApiController>();
        }

        public async Task<IActionResult> Info(string paymentId)
        {
            var result = new PaymentInfo {
              DbContent = await dbContext.PaypalPayments.SingleAsync(
                  p=>p.PaypalPaymentId==paymentId)
            };
            await Task.Run( () => {
              var apiContext = paymentSettings.CreateAPIContext();
              result.FromPaypal = Payment.Get(apiContext,paymentId);
            });

            return Ok(result);
        }

        [HttpPost("execute")]
        public async Task<IActionResult> Execute(string paymentId, string payerId)
        {
            Payment result=null;
            await Task.Run( () => {
            var apiContext = paymentSettings.CreateAPIContext();
            var payment = Payment.Get(apiContext,paymentId);
            var execution = new PaymentExecution();
            execution.payer_id = payerId;
            execution.transactions = payment.transactions;
             result = payment.Execute(apiContext,execution);
            });

            return Ok(result);
        }

        [HttpPost("create"),AllowAnonymous]
        public async Task<IActionResult> Create()
        {
            var apiContext = paymentSettings.CreateAPIContext();
            Payment result=apiContext.CreatePayment(new Estimate());
            return Ok(Payment.Create(apiContext,result));
        }

    }
}
