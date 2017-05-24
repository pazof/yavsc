

using PayPal.PayPalAPIInterfaceService.Model;
using Yavsc.Models.Payment;

namespace Yavsc.ViewModels.PayPal
{
    public class PaymentInfo
    {
        public PayPalPayment DbContent { get; set; }

        public virtual GetExpressCheckoutDetailsResponseType DetailsFromPayPal { get; set; }
    }
}
