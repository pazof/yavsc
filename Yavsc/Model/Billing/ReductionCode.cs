
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Yavsc.Models.Billing {

public class ReductionCode : IBillingClause
{
    [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public ReductionCode(string descr, decimal impact) {
        Description = descr;
        impacter = new FixedImpacter(impact);
    }
    public string Description
    {
        get;
        set;
    }
    IBillingImpacter impacter;
    public IBillingImpacter Impacter
    {
        get
        {
            return impacter ;
        }
        private set {
            impacter  = value;
        }
    }
}
}