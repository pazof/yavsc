using System.Collections.Generic;
using System.Linq;
using YavscLib.Billing;

namespace Yavsc.Helpers
{
    public static class BillingHelpers
    {
        public static decimal Addition(this List<IBillItem> items) => items.Aggregate<IBillItem, decimal>(0m, (t, l) => t + l.Count * l.UnitaryCost);

    }
}
