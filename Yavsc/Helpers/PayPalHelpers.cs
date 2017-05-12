using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using PayPal.Api;
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

        public static Payment CreatePaiment(this APIContext apiContext, NominativeServiceCommand query, string intent = "sale", ILogger logger=null)
        {
            var queryType = query.GetType().Name;
            var transaction = new Transaction
            {
                description ="Votre commande du "+query.DateCreated.ToLongDateString()+
                "\n"+query.Description,
                invoice_number = $"{query.ActivityCode}/{queryType}/{query.Id}"
            };
            logger?.LogInformation("transaction: "+transaction.invoice_number);
            // transaction.item_list.shipping_address.city
            // country_code default_address id
            // line1 line2 preferred_address recipient_name state status type
            transaction.item_list = new ItemList();
            if (query.Client.PostalAddress!=null) {
                transaction.item_list.shipping_address = new ShippingAddress();
                transaction.item_list.shipping_address.line1 = query.Client.PostalAddress.Address;
            }
            transaction.item_list.shipping_phone_number = query.Client.PhoneNumber;
            logger?.LogInformation("client address: "+transaction.item_list.shipping_address.line1);
            transaction.item_list.items = query.GetBillItems().Select(i => new Item {
                name = i.Name,
                description = i.Description,
                price = i.UnitaryCost.ToString("F2"),
                currency = "EUR",
                quantity = i.Count.ToString(),
                sku = query.ActivityCode
         /* postback_data=
            sku=
            supplementary_data= */
             }).ToList();


            return Payment.Create(apiContext, new Payment
            {
                intent = intent, // "sale", "order", "authorize"
                payer = new Payer
                {
                    payment_method = "paypal"
                },
                transactions = new List<Transaction> { transaction }
            });
        }

        public static Payment CreatePaiment(this APIContext apiContext, Estimate estimation)
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
