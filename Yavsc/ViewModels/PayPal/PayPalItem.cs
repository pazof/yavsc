
namespace Yavsc.ViewModels.PayPal
{
    public class Item
                    {
                        public string quantity { get; set; }
                        public string name { get; set; }
                        public string price { get; set; }
                        public string currency { get; set; } = "EUR";
                        public string description { get; set; }
                        public string tax { get; set; } = "1";

                    }
}
