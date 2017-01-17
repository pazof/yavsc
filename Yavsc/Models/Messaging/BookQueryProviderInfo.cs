using System;

namespace Yavsc.Model
{
    using Models.Messaging;

    public class BookQueryProviderInfo
    {
        public ClientProviderInfo Client { get; set; }
        public Location Location { get; set; }

        public long Id { get; set; }

        public DateTime EventDate { get; set; }
        public decimal? Previsional { get; set; }

        public string Reason { get; set; }

        public string ActivityCode { get; set; } 
    }
    
}
