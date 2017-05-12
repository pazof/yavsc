using YavscLib.Billing;

namespace Yavsc.Models.Billing   {
public class FixedImpacter : IBillingImpacter
{
    public decimal ImpactedValue { get; set; }
    public FixedImpacter (decimal impact)
    {
        ImpactedValue = impact;
    }
    public decimal Impact(decimal orgValue)
    {
        return orgValue + ImpactedValue;
    }
}
}
