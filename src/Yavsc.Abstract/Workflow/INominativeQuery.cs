using System;

namespace Yavsc.Abstract.Workflow
{
    public interface IDecidableQuery: IBaseTrackedEntity, IQuery
    {
         bool Decided { get; set; }
         bool Accepted { get; set; }
         
    }
}
