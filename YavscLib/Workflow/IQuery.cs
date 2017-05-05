namespace YavscLib.Models.Workflow
{
    using System.Collections.Generic;
    using YavscLib;
    using YavscLib.Billing;

    public interface IQuery: IBaseTrackedEntity, IBillable
    {
        QueryStatus Status { get; set; }
    }
    public interface IBillable {
        string Description { get; set; }
        List<IBillItem> GetBillItems();

    }
}
