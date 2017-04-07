namespace Yavsc.Models.Workflow
{
    using YavscLib;

    public interface IQuery: IBaseTrackedEntity
    {
        QueryStatus Status { get; set; }
    }

}
