
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using PayPal.Api;
using Yavsc.Helpers;
using Yavsc.Models;

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

        [HttpPost("create")]
        public async Task<IActionResult> Create()
        {
            var apiContext = paymentSettings.CreateAPIContext();
            var payment = Payment.Create(apiContext,
            new Payment
            {
                intent = "authorize", // "sale", "order", "authorize"
                payer = new Payer
                {
                    payment_method = "paypal"
                },
                transactions = new List<Transaction>
    {
        new Transaction
        {
            description = "Transaction description.",
            invoice_number = "001",
            amount = new Amount
            {
                currency = "EUR",
                total = "0.11",
                details = new Details
                {
                    tax = "0.01",
                    shipping = "0.02",
                    subtotal = "0.08"
                }
            },
            item_list = new ItemList
            {
                items = new List<Item>
                {
                    new Item
                    {
                        name = "nah",
                        currency = "EUR",
                        price = "0.02",
                        quantity = "4",
                        sku = "sku"
                    }
                }
            }
        }
    },
                redirect_urls = new RedirectUrls
                {
                    return_url = siteSettings.Audience+ "/Manage/Credit/Return",
                    cancel_url = siteSettings.Audience+ "/Manage/Credit/Cancel"
                }
            });
            return Json(payment);
        }

    }
}
