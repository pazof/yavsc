using YavscLib.Billing;

namespace Yavsc.Models.Billing {
    public interface IBillingClause { 
        string Description {get; set;}
        IBillingImpacter Impacter { get; }
    }

}
