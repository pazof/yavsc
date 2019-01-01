namespace Yavsc.Abstract.Workflow
{
    using Yavsc;
    using Yavsc.Billing;

    public interface IQuery: IBaseTrackedEntity, IBillable
    {
        QueryStatus Status { get; set; }
        string PaymentId { get; set; }
    }
}
