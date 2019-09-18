using System;
using Yavsc.Abstract.Identity;
using Yavsc.Models.Relationship;

namespace Yavsc.Models
{

    public class RdvQueryProviderInfo
    {
        /// <summary>
        /// User querying
        /// </summary>
        /// <value></value>
        public ClientProviderInfo Client { get; set; }
        public Location Location { get; set; }
        public long Id { get; set; }
        public DateTime? EventDate { get; set; }
        public decimal? Previsional { get; set; }
        public string Reason { get; set; }
        public string ActivityCode { get; set; }
        public string BillingCode { get; set; }
    }

}
