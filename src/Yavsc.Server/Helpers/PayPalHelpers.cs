using System.Collections.Generic;

using Microsoft.Extensions.Logging;
using Yavsc.Models.Billing;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PayPal.PayPalAPIInterfaceService.Model;
using PayPal.PayPalAPIInterfaceService;
using Yavsc.ViewModels.PayPal;
using Yavsc.Models;
using System.Linq;
using Yavsc.Models.Payment;
using Microsoft.EntityFrameworkCore;

namespace Yavsc.Helpers
{
    public static class PayPalHelpers
    {
        private static Dictionary<string,string> payPalProperties = null;
        public static Dictionary<string,string> GetPayPalProperties() {
            if (payPalProperties==null) {

                payPalProperties = new Dictionary<string,string>();
                var paypalSettings = Config.PayPalSettings;
                // Don't do:
                //  payPalProperties.Add("mode", Startup.PayPalSettings.Mode);
                // Instead, set the endpoint parameter.
                if (paypalSettings.Mode == "production") {
                    // use nvp end point: https://api-3t.paypal.com/nvp
                    payPalProperties.Add("endpoint", "https://api-3t.paypal.com/nvp");

                } else {
                    payPalProperties.Add("endpoint", "https://api-3t.sandbox.paypal.com/nvp");
                }
                payPalProperties.Add("clientId", paypalSettings.ClientId);
                payPalProperties.Add("clientSecret", paypalSettings.ClientSecret);

                int numClient = 0;
                if (paypalSettings.Accounts!=null)
                foreach (var account in paypalSettings.Accounts) {
                    numClient++;
                    payPalProperties.Add ($"account{numClient}.apiUsername",account.ApiUsername);
                    payPalProperties.Add ($"account{numClient}.apiPassword",account.ApiPassword);
                    payPalProperties.Add ($"account{numClient}.apiSignature",account.Signature);
                }
            }
            return payPalProperties;
        }
        private static PayPalAPIInterfaceServiceService payPalService = null;
        public static PayPalAPIInterfaceServiceService PayPalService {
            get {
            if (payPalService==null)
             payPalService = new PayPal.PayPalAPIInterfaceService.PayPalAPIInterfaceServiceService(GetPayPalProperties());
            return payPalService;
        }}

        public class PaymentUrls
        {
            public string ReturnUrl { get; set; }
            public string CancelUrl { get; set; }
            public string CGVUrl { get; set; }
        }
        public static PaymentUrls GetPaymentUrls(this HttpRequest request, string controllerName, string id)
        {
            var result = new PaymentUrls
            {
                ReturnUrl = request.ToAbsolute($"{controllerName}/PaymentConfirmation/{id}"),
                CancelUrl = request.ToAbsolute($"{controllerName}/ClientCancel/{id}"),
                CGVUrl = request.ToAbsolute($"{controllerName}/CGV")
            };
            return result;
        }
        public static SetExpressCheckoutResponseType CreatePayment(this HttpRequest request, string controllerName, NominativeServiceCommand query, string intent = "sale", ILogger logger = null)
        {
            var items = query.GetBillItems();
            var total = items.Addition().ToString("F2");
            var coreq = new SetExpressCheckoutReq {};
            var urls = request.GetPaymentUrls(controllerName, query.Id.ToString());

            var pitem = new PaymentDetailsItemType {};
            coreq.SetExpressCheckoutRequest = new SetExpressCheckoutRequestType{
                DetailLevel = new List<DetailLevelCodeType?> { DetailLevelCodeType.RETURNALL },
                SetExpressCheckoutRequestDetails = new SetExpressCheckoutRequestDetailsType
                {
                    PaymentDetails = new List<PaymentDetailsType>( new [] { new PaymentDetailsType{
                    OrderDescription = query.Description,
                    OrderTotal = new BasicAmountType {
                        currencyID = CurrencyCodeType.EUR,
                        value = total
                    },
                    PaymentDetailsItem = new List <PaymentDetailsItemType> (
                        items.Select(i =>  new PaymentDetailsItemType {
                            Amount = new BasicAmountType { currencyID = CurrencyCodeType.EUR, value = i.UnitaryCost.ToString("F2") },
                            Name = i.Name,
                            Quantity = i.Count,
                            Description = i.Description
                        })
                    )
                    }}),
                    InvoiceID = query.GetInvoiceId(),
                    // NOTE don't set OrderDescription : "You cannot pass both the new and deprecated order description.","ErrorCode":"11804
                    CancelURL = urls.CancelUrl,
                    ReturnURL = urls.ReturnUrl
                }
            };

            var d = new SetExpressCheckoutRequestDetailsType();

            logger.LogInformation($"Creating express checkout for {Config.PayPalSettings.MerchantAccountUserName} : "+JsonConvert.SerializeObject(coreq));
            var response = PayPalService.SetExpressCheckout( coreq, Config.PayPalSettings.MerchantAccountUserName );

            return response;
        }

        public static async Task<PaymentInfo> GetCheckoutInfo(
             this ApplicationDbContext context,
             string token)
        {
            return  await CreatePaymentViewModel(context,token,GetExpressCheckoutDetails(token));
        }
        private static GetExpressCheckoutDetailsResponseType GetExpressCheckoutDetails(string token)
        {
            GetExpressCheckoutDetailsReq req = new GetExpressCheckoutDetailsReq{
               GetExpressCheckoutDetailsRequest = new GetExpressCheckoutDetailsRequestType {
                Token = token
               }
            };
            return  PayPalService.GetExpressCheckoutDetails(req,Config.PayPalSettings.Accounts[0].ApiUsername);
        }
        public static async Task<PaymentInfo> ConfirmPayment(
             this ApplicationDbContext context,
             string userId,
             string payerId,
             string token)
        {
            var details = GetExpressCheckoutDetails(token);
            var payment = await context.PayPalPayment.SingleOrDefaultAsync(p=>p.CreationToken == token);
            if (payment == null)
                {
                    payment = new PayPalPayment{
                            ExecutorId = userId,
                            PaypalPayerId = payerId,
                            CreationToken = token,
                            // NOTE: 1 order <=> 1 bill <=> 1 payment
                            OrderReference = details.GetExpressCheckoutDetailsResponseDetails.InvoiceID,
                            State = details.Ack.ToString()
                        };

                    context.PayPalPayment.Add(payment);
                }
                else {
                    payment.ExecutorId = userId;
                    payment.PaypalPayerId = payerId;
                    payment.State = details.Ack.ToString();
                }
             await context.SaveChangesAsync(userId);
             // GetCheckoutInfo(,token);
             return new PaymentInfo { DbContent = payment, DetailsFromPayPal = details };
        }
        public static async Task<PaymentInfo> CreatePaymentViewModel (
            this ApplicationDbContext context,
            string token, GetExpressCheckoutDetailsResponseType fromPayPal)
        {
            return new PaymentInfo {
              DbContent = await context.PayPalPayment
              .Include(p=>p.Executor)
              .SingleOrDefaultAsync(
                  p=>p.CreationToken==token),
                  DetailsFromPayPal = fromPayPal
            };
        }

    }
}
