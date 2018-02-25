using System;

namespace Yavsc.Abstract.Workflow
{
    public interface INominativeQuery: IQuery
    {
         bool Rejected { get; set; }
         DateTime RejectedAt { get; set; }
    }
}