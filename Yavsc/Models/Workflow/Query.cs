namespace Yavsc.Models.Workflow
{
    using Interfaces.Workflow;
    using YavscLib;
    
    public interface IQuery: IBaseTrackedEntity
    {
        QueryStatus Status { get; set; }
    }

}
