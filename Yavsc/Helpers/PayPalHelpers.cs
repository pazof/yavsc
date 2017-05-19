using System.Collections.Generic;

using Microsoft.Extensions.Logging;
using PayPal.Api;
using PayPal.Exception;
using Yavsc.Models.Billing;
using Microsoft.AspNet.Http;

namespace Yavsc.Helpers
{
    public static class PayPalHelpers
    {
        private static OAuthTokenCredential payPaylCredential=null;
        public static OAuthTokenCredential PayPaylCredential
        {
            get
            {
                if (payPaylCredential==null)
                {
                    Dictionary<string,string> config = new Dictionary<string,string>();
              //      config.Add("mode",Startup.PayPalSettings.Mode);
              //      config.Add("clientId",Startup.PayPalSettings.ClientId);
              //      config.Add("clientSecret",Startup.PayPalSettings.Secret);
              //      config.Add("user",Startup.PayPalSettings.APIUserId);
              // https://api-3t.paypal.com/nvp
                    config.Add("USER",Startup.PayPalSettings.APIUserId);
                    config.Add("SIGNATURE",Startup.PayPalSettings.APISignature);
                    config.Add("PWD",Startup.PayPalSettings.APIPassword);
                    payPaylCredential = new OAuthTokenCredential(config);
                }
                return payPaylCredential;
            }
        }

        public static  APIContext CreateAPIContext()
        {
            var accessToken = PayPaylCredential.GetAccessToken();
            var apiContext = new APIContext(accessToken);
            return apiContext;
        }

        public class PaymentUrls {
            public string Details { get; set; }
            public string Cancel { get; set; }

            public string CGV { get; set; }
        }
        public static PaymentUrls GetPaymentUrls(this HttpRequest request, string controllerName, string id )
        {
            var result =new PaymentUrls {
                Details = request.ToAbsolute($"{controllerName}/Details/{id}") ,
                Cancel = request.ToAbsolute($"{controllerName}/ClientCancel/{id}"),
                CGV = request.ToAbsolute($"{controllerName}/CGV")
                };
            return result;
        }

        public static Payment CreatePayment(this HttpRequest request, string controllerName, APIContext apiContext, NominativeServiceCommand query, string intent = "sale", ILogger logger=null)
        {
            var queryType = query.GetType().Name;
            var transaction = new Transaction
            {
                description = query.Description,
                invoice_number = query.Id.ToString(),
                custom = query.GetType().Name + "/"+ query.Id.ToString()
            };

            var urls = request.GetPaymentUrls(controllerName, query.Id.ToString() );

            transaction.order_url = urls.Details;

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
*/             var total = query.GetBillItems().Addition().ToString("F2");
             transaction.amount = new Amount {
                currency = "EUR",
                total = total
             };
             var payment = new Payment
                {
                    intent = intent, // "sale", "order", "authorize"
                    payer = new Payer
                    {
                        payment_method = "paypal"
                    },
                    transactions = new List<Transaction> { transaction },
                    redirect_urls = new RedirectUrls
                    {
                        return_url = urls.Details,
                        cancel_url = urls.Cancel
                    }
                };
            Payment result = null;
            try {
                result = Payment.Create(apiContext,payment);
            }
            catch (PaymentsException ex) {
                logger.LogError (ex.Message);
            }
            return result;
        }

        public static Payment CreatePayment(this HttpRequest request, string controllerName, APIContext apiContext, Estimate estimation)
        {
            var urls = request.GetPaymentUrls( controllerName, estimation.Id.ToString() );
            var payment = Payment.Create(apiContext,
            new Payment
            {
                intent = "order", // "sale", "order", "authorize"
                payer = new Payer
                {
                    payment_method = "paypal"
                },
                transactions = new List<Transaction>
    {
        new Transaction
        {
            description = "Transaction description.",
            invoice_number = estimation.Id.ToString(),
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
                    return_url = urls.Details,
                    cancel_url = urls.Cancel
                }
            });

            return payment;
        }
    }
}
