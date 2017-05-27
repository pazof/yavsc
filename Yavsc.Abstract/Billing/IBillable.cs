using System.Collections.Generic;

namespace Yavsc.Billing
{
    public interface IBillable {
        string Description { get; set; }
        List<IBillItem> GetBillItems();
        long Id { get; set; }

    }
}
