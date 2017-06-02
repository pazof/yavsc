using System.Collections.Generic;
using System.Linq;
using Yavsc.Billing;
using Yavsc.Models.Billing;

namespace Yavsc.Helpers
{
    public static class BillingHelpers
    {
        public static decimal Addition(this List<IBillItem> items) => items.Aggregate<IBillItem, decimal>(0m, (t, l) => t + l.Count * l.UnitaryCost);

         public static decimal Addition(this List<CommandLine> items) => items.Select(i=>((IBillItem)i)).ToList().Addition();

    }
}
