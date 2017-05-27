using Yavsc.Billing;

namespace Yavsc.Models.Billing   {
    public class ProportionalImpacter : IBillingImpacter
    {
        public decimal K { get; set; }
        public decimal Impact(decimal orgValue)
        {
            return orgValue * K;
        }
    }

}
