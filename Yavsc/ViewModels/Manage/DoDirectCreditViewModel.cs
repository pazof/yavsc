namespace Yavsc.ViewModels.Manage
{

        public class DoDirectCreditViewModel {
            public string PaymentType  { get; set;}
            public string PayerName  { get; set;}
            public string FirstName  { get; set;}
            public string LastName  { get; set;}
            public string CreditCardNumber  { get; set;}
            public string CreditCardType  { get; set;}
            public string Cvv2Number  { get; set;}
            public string CardExpiryDate  { get; set;}
            public string IpnNotificationUrl { get; set; }
            public string Street1 { get; set; }
            public string Street2 { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string Country { get; set; }
            public string PostalCode { get; set; }
            public string Phone { get; set; }
            public string CurrencyCode { get; set; }
            public string Amount { get; set; }
        }
}