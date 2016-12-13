
using Yavsc.Models.Billing;

namespace Yavsc.Helpers
{
    public static class EstimateHelpers { 
        public static decimal GetTotal(this Estimate estimate)
        {
            decimal result = 0;
            foreach (var l in estimate.Bill)
                result += l.Count*l.UnitaryCost;
            return result;
        }
    }

}