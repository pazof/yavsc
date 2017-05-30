using System.Collections.Generic;

namespace Yavsc.Billing
{
    public interface IBillable {
        string GetDescription ();
        List<IBillItem> GetBillItems();
        long Id { get; set; }

    }
}
