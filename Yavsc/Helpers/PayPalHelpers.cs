using System.Collections.Generic;

using Microsoft.Extensions.Logging;
using PayPal.Exception;
using Yavsc.Models.Billing;
using Microsoft.AspNet.Http;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using System.Text;
using Newtonsoft.Json;
using PayPal.PayPalAPIInterfaceService.Model;
using PayPal.PayPalAPIInterfaceService;
using Yavsc.ViewModels.PayPal;
using Yavsc.Models;
using Microsoft.Data.Entity;
using System.Linq;
using Yavsc.Models.Payment;

namespace Yavsc.Helpers
{
    public static class PayPalHelpers
    {
        private static Dictionary<string,string> payPalProperties = null;
        public static Dictionary<string,string> GetPayPalProperties() {
            if (payPalProperties==null) {

                payPalProperties = new Dictionary<string,string>();
                // Don't do:
                //  payPalProperties.Add("mode", Startup.PayPalSettings.Mode);
                // Instead, set the endpoint parameter.
                if (Startup.PayPalSettings.Mode == "production") {
                    // use nvp end point: https://api-3t.paypal.com/nvp
                    payPalProperties.Add("endpoint", "https://api-3t.paypal.com/nvp");

                } else {
                    payPalProperties.Add("endpoint", "https://api-3t.sandbox.paypal.com/nvp");
                }
                payPalProperties.Add("clientId", Startup.PayPalSettings.ClientId);
                payPalProperties.Add("clientSecret", Startup.PayPalSettings.ClientSecret);

                int numClient = 0;
                if (Startup.PayPalSettings.Accounts!=null)
                foreach (var account in Startup.PayPalSettings.Accounts) {
                    numClient++;
                    payPalProperties.Add ($"account{numClient}.apiUsername",account.ApiUsername);
                    payPalProperties.Add ($"account{numClient}.apiPassword",account.ApiPassword);
                    payPalProperties.Add ($"account{numClient}.apiSignature",account.Signature);
                }
            }
            return payPalProperties;
        }

        public class PayPalOAuth2Token
        {
            public string scope;
            public string nonce;
            public string access_token;
            public string token_type;

            public string app_id;
            public string expires_in;

        }
        private static PayPalOAuth2Token token = null;
        private static PayPalAPIInterfaceServiceService payPalService = null;
        public static PayPalAPIInterfaceServiceService PayPalService {
            get {
            if (payPalService==null)
             payPalService = new PayPal.PayPalAPIInterfaceService.PayPalAPIInterfaceServiceService(GetPayPalProperties());
            return payPalService;
        }}

        [Obsolete("use the PayPalService property")]
        internal static async Task<string> GetAccessToken()
        {


            if (token == null)
            {
                token = new PayPalOAuth2Token();

                using (HttpClient client = new HttpClient())
                {

                    var uriString = Startup.PayPalSettings.Mode == "production" ?
                    "https://api.paypal.com/v1/oauth2/token" : "https://api.sandbox.paypal.com/v1/oauth2/token";

                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.DefaultRequestHeaders.Add("Accept-Language", "en_US");


                    string oAuthCredentials = Convert.ToBase64String(Encoding.Default.GetBytes(Startup.PayPalSettings.ClientId + ":" + Startup.PayPalSettings.ClientSecret));

                    var h_request = new HttpRequestMessage(HttpMethod.Post, uriString);

                    h_request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", oAuthCredentials);
                    var keyValues = new List<KeyValuePair<string, string>>();
                    keyValues.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
                    h_request.Content = new FormUrlEncodedContent(keyValues);

                    using (var response = await client.SendAsync(h_request))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            Console.WriteLine(content);
                            token = JsonConvert.DeserializeObject<PayPalOAuth2Token>(content);
                        }
                        else
                        {
                            throw new PayPalException($"{response.StatusCode}");
                        }
                    }

                }
            }
            return token?.access_token ?? null;
                //return PayPalCredential.GetAccessToken();

        }

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
                ReturnUrl = request.ToAbsolute($"{controllerName}/Confirmation/{id}"),
                CancelUrl = request.ToAbsolute($"{controllerName}/ClientCancel/{id}"),
                CGVUrl = request.ToAbsolute($"{controllerName}/CGV")
            };
            return result;
        }

        private static string MerchantUserId { get {
            return Startup.PayPalSettings.Accounts[0].ApiUsername;
        }}
        public static SetExpressCheckoutResponseType CreatePayment(this HttpRequest request, string controllerName, NominativeServiceCommand query, string intent = "sale", ILogger logger = null)
        {
            var items = query.GetBillItems();
            var total = items.Addition().ToString("F2");
            var queryType = query.GetType().Name;
            var coreq = new SetExpressCheckoutReq {};
            var urls = request.GetPaymentUrls(controllerName, query.Id.ToString());
            coreq.SetExpressCheckoutRequest = new SetExpressCheckoutRequestType{
                DetailLevel = new List<DetailLevelCodeType?> { DetailLevelCodeType.RETURNALL },
                SetExpressCheckoutRequestDetails = new SetExpressCheckoutRequestDetailsType
                {
                    PaymentDetails = items.Select(i => new PaymentDetailsType{
                         OrderTotal = new BasicAmountType {
                        currencyID = CurrencyCodeType.EUR,
                        value = (i.Count * i.UnitaryCost).ToString("F2")
                         },
                        OrderDescription = i.Description
                    }).ToList(),
                    InvoiceID = queryType +  "/" + query.Id.ToString(),
OrderDescription = query.Description, CancelURL = urls.CancelUrl, ReturnURL = urls.ReturnUrl
                }
            };
            var response = PayPalService.SetExpressCheckout( coreq, MerchantUserId );

            // transaction.item_list.shipping_address.city
            // country_code default_address id
            // line1 line2 preferred_address recipient_name state status type
            /*   transaction.item_list = new ItemList();
               if (query.Client.PostalAddress!=null) {
                   var address =  query.Client.PostalAddress?.Address;
                   if (address!=null) {
                       var parts = new Stack<string> ( address.Split(',') );
                       var country = parts.Pop().Trim();
                       var city = parts.Pop().Trim().Split(' ');
                       var line1 = parts.First().Trim();
                       var line2 = string.Join(" - ",parts.Skip(1));
                   transaction.item_list.shipping_address = new ShippingAddress {
                       line1 = line1,
                       line2 = line2,
                       city = city[1],
                       postal_code = city[0],
                       country_code = country == "France" ? "fr" : country
                   };
                   }
               }
               transaction.item_list.shipping_phone_number = query.Client.PhoneNumber;
               var items = query.GetBillItems();
               transaction.item_list.items = items.Select(i => new Item {
                   name = i.Name,
                   description = i.Description,
                   quantity = i.Count.ToString(),
                   price = i.UnitaryCost.ToString("F2"),
                   currency = "EUR",
                   sku = "sku"
          //  postback_data=
             //  supplementary_data=
                }).ToList();
   */

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
            return  PayPalService.GetExpressCheckoutDetails(req,Startup.PayPalSettings.Accounts[0].ApiUsername);
        }
        public static async Task<PaymentInfo> ConfirmPayment(
             this ApplicationDbContext context,
             string userId,
             string payerId,
             string token)
        {
             var details = GetExpressCheckoutDetails(token);
            var payment = new PayPalPayment{
                ExecutorId = userId,
                PaypalPayerId = payerId,
                CreationToken = token
            };
             context.PayPalPayments.Add(payment);
             await context.SaveChangesAsync(userId);
             // GetCheckoutInfo(,token);
             return new PaymentInfo { DbContent = payment, DetailsFromPayPal = details };
        }
        public static async Task<PaymentInfo> CreatePaymentViewModel (
            this ApplicationDbContext context,
            string token, GetExpressCheckoutDetailsResponseType fromPayPal)
        {
            return new PaymentInfo {
              DbContent = await context.PayPalPayments.SingleOrDefaultAsync(
                  p=>p.CreationToken==token),
                  DetailsFromPayPal = fromPayPal
            };
        }

    }
}
