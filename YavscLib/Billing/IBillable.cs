using System.Collections.Generic;

namespace YavscLib.Billing
{
    public interface IBillable {
        string Description { get; set; }
        List<IBillItem> GetBillItems();
        long Id { get; set; }

    }
}
