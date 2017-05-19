namespace YavscLib.Workflow
{
    using YavscLib;
    using YavscLib.Billing;

    public interface IQuery: IBaseTrackedEntity, IBillable
    {
        QueryStatus Status { get; set; }
        string PaymentId { get; set; }
    }
}
