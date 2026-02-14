using System.ComponentModel.DataAnnotations;
using Yavsc.Attributes.Validation;

namespace Yavsc.ViewModels.Manage
{

        public class DoDirectCreditViewModel {
            [YaRequired]
            public string PaymentType  { get; set;}
            [YaRequired]
            public string PayerName  { get; set;}
            [YaRequired]
            public string FirstName  { get; set;}
            [YaRequired]
            public string LastName  { get; set;}
            [YaRequired]
            public string CreditCardNumber  { get; set;}
            public string CreditCardType  { get; set;}
            public string Cvv2Number  { get; set;}
            public string CardExpiryDate  { get; set;}
            public string IpnNotificationUrl { get; set; }
            [YaRequired]
            public string Street1 { get; set; }
            public string Street2 { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string Country { get; set; }
            [YaRequired]
            public string PostalCode { get; set; }
            public string Phone { get; set; }
            [YaRequired]
            public string CurrencyCode { get; set; }
            [YaRequired]
            public string Amount { get; set; }
        }
}
