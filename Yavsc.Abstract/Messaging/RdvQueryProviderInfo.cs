using System;

namespace Yavsc.Models
{
    using Models.Messaging;

    public class RdvQueryProviderInfo
    {
        public ClientProviderInfo Client { get; set; }
        public ILocation Location { get; set; }

        public long Id { get; set; }

        public DateTime? EventDate { get; set; }
        public decimal? Previsional { get; set; }

        public string Reason { get; set; }

        public string ActivityCode { get; set; }
        public string BillingCode { get; set; }
    }

}
