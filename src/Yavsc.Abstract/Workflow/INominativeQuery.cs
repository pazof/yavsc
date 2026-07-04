using System;

namespace Yavsc.Abstract.Workflow
{
    public interface IDecidableQuery: ITrackedEntity, IQuery
    {
         bool Reviewed { get; set; }
         bool Accepted { get; set; }

    }
}
