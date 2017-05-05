using System.ComponentModel.DataAnnotations;

namespace Yavsc.ViewModels.PayPal
{
    public class CreatePaymentRequest
    {
        [Required]
        public string intent { get; set; } = "sale"; // sale,
        public string experience_profile_id { get; set; }
        public class RedirectUrls
        {
            public string return_url { get; set; } = "";
            public string cancel_url { get; set; }
        }
        public RedirectUrls redirect_urls;
        public class Payer
        {
            public string payment_method { get; set; } = "paypal";
        }
        Payer payer;
    }
}
