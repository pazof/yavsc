using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using PayPal.Api;
using PayPal.Exception;
using Yavsc.Models.Billing;

namespace Yavsc.Helpers
{
    public static class PayPalHelpers
    {
        public static  APIContext CreateAPIContext(this PayPalSettings settings)
        {
            Dictionary<string,string> config = new Dictionary<string,string>();
            config.Add("mode",settings.Mode);
            config.Add("clientId",settings.ClientId);
            config.Add("clientSecret",settings.Secret);
            var accessToken = new OAuthTokenCredential(config).GetAccessToken();
            var apiContext = new APIContext(accessToken);
            return apiContext;
        }

        public static Payment CreatePayment(this APIContext apiContext, NominativeServiceCommand query, string controllerName, string intent = "sale", ILogger logger=null)
        {
            var queryType = query.GetType().Name;
            var transaction = new Transaction
            {
                description = query.Description,
                invoice_number = query.Id.ToString(),
                custom = query.GetType().Name + "/"+ query.Id.ToString()
            };
            transaction.order_url = Startup.Audience + "/"  +controllerName + "/Details/" + query.Id;

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
                        return_url = Startup.Audience+ $"/{controllerName}/Details/"+query.Id.ToString(),
                        cancel_url = Startup.Audience+ $"/{controllerName}/ClientCancel/"+query.Id.ToString()
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

        public static Payment CreatePayment(this APIContext apiContext, Estimate estimation)
        {
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
                    return_url = Startup.Audience+ "/Manage/Credit/Return",
                    cancel_url = Startup.Audience+ "/Manage/Credit/Cancel"
                }
            });
            return payment;
        }
    }
}
