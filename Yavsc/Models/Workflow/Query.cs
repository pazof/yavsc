using System;

namespace Yavsc.Models.Workflow
{
  using Interfaces.Workflow;
  using Models.Market;
  using YavscLib;

    public interface IQuery: IBaseTrackedEntity
    {
        QueryStatus Status { get; set; }
    }

}
