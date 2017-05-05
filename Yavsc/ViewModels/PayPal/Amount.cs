
namespace Yavsc.ViewModels.PayPal
{
    public class Amount
    {
        public string total { get; set; }
        public string currency { get; set; } = "EUR";
        public class Details
        {
            public string subtotal { get; set; }
            public string shipping { get; set; }
            public string tax { get; set; }
            public string shipping_discount { get; set; }

        }
        public Details details;
        public class ItemList
        {
            public Item[] items;
            public string description { get; set; }
            public string invoice_number { get; set; }
            public string custom { get; set; }
        }

    }
}
