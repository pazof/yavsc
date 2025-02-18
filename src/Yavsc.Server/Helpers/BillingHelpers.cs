using System.Globalization;
using Yavsc.Billing;
using Yavsc.Models.Billing;
using Yavsc.Server.Helpers;
using Yavsc.Services;

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

         public static FileInfo GetBillInfo(this IBillable bill, IBillingService service)
         {
             var suffix = bill.GetIsAcquitted() ? "-ack":null;
             var filename = bill.GetFileBaseName(service)+".pdf";
            return new FileInfo(Path.Combine(AbstractFileSystemHelpers.UserBillsDirName, filename));
         }
    }
}
