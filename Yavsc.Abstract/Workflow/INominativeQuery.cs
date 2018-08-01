using System;

namespace Yavsc.Abstract.Workflow
{
    public interface IDecidableQuery: IQuery
    {
         bool Rejected { get; set; }
         DateTime RejectedAt { get; set; }
    }
}