using System.ComponentModel.DataAnnotations;

namespace Yavsc.ViewModels.Manage
{

        public class DoDirectCreditViewModel {
            [Required]
            public string PaymentType  { get; set;}
            [Required]
            public string PayerName  { get; set;}
            [Required]
            public string FirstName  { get; set;}
            [Required]
            public string LastName  { get; set;}
            [Required]
            public string CreditCardNumber  { get; set;}
            public string CreditCardType  { get; set;}
            public string Cvv2Number  { get; set;}
            public string CardExpiryDate  { get; set;}
            public string IpnNotificationUrl { get; set; }
            [Required]
            public string Street1 { get; set; }
            public string Street2 { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string Country { get; set; }
            [Required]
            public string PostalCode { get; set; }
            public string Phone { get; set; }
            [Required]
            public string CurrencyCode { get; set; }
            [Required]
            public string Amount { get; set; }
        }
}
