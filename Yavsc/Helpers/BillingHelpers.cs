using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Yavsc.Billing;
using Yavsc.Models.Billing;

namespace Yavsc.Helpers
{
    public static class BillingHelpers
    {
        public static decimal Addition(this List<IBillItem> items) => items.Aggregate<IBillItem, decimal>(0m, (t, l) => t + l.Count * l.UnitaryCost);

         public static decimal Addition(this List<CommandLine> items) => items.Select(i=>((IBillItem)i)).ToList().Addition();

         public static string GetBillText(this IBillable query) {
            string total = query.GetBillItems().Addition().ToString("C", CultureInfo.CurrentUICulture);
            string bill = string.Join("\n", query.GetBillItems().Select(l=> $"{l.Name} {l.Description} : {l.UnitaryCost} € " + ((l.Count != 1) ? "*"+l.Count.ToString() : ""))) +
                $"\n\nTotal: {total}";
            return bill;
         }

         public static FileInfo GetBillInfo(string billingcode, long id)
         {
             var filename = $"facture-{billingcode}-{id}.pdf";
            return new FileInfo(Path.Combine(Startup.UserBillsDirName, filename));
         }
    }
}
