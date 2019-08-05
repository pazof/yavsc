using Yavsc.Billing;

namespace Yavsc.Models.Billing {
    public interface IBillingClause { 
        string Description {get; set;}
        IBillingImpacter Impacter { get; }

        // TODO
        // Conditions de ventes relatives à l'impact
        // IEnumerable<long> CGV,CPV 
    }

}
