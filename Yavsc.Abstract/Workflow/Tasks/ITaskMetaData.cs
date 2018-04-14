using System.Collections.Generic;
using Yavsc.Models;

namespace Yavsc.Abstract.Workflow
{
    public interface ITaskMetaData
    {
        string TaskName { get; }
        IEnumerable <IRequisition> Prerequisites { get; }
    }
}