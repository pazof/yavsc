using ZicMoove.Model.Social;

namespace ZicMoove.Model.Workflow.Marketing
{

    public class PerformerProfile {

        public string PerfomerId { get; set; }
 
        public string ActivityCode { get; set; }

        public Service Offer { get; set; }

        public string SIREN { get; set; }

        public long OrganisationAddressId { get; set; }

        public virtual Location OrganisationAddress { get; set; }

        public virtual Activity Activity { get; set; }

        public bool AcceptNotifications { get; set; }

        public bool AcceptPublicContact { get; set; }

        public bool AcceptGeoLocalisation { get; set; }

        public string WebSite { get; set; }

        public bool Active { get; set; }

        public int? MaxDailyCost { get; set; }

        public int? MinDailyCost { get; set; }

        public int Rate { get; set; }
        
    }
}