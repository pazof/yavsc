using System;

namespace Yavsc.Abstract.Workflow
{
    public interface IDecidableQuery: ITrackedEntity, IQuery
    {
         bool Decided { get; set; }
         bool Accepted { get; set; }
         
    }
}
