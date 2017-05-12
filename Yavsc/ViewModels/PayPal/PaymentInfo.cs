using PayPal.Api;
using Yavsc.Models.Payment;

namespace Yavsc.ViewModels.PayPal
{
    public class PaymentInfo
    {
        public PaypalPayment DbContent { get; set; }
        public Payment FromPaypal { get; set; }
    }
}
