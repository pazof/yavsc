using System.Collections.Generic;
using System.Threading.Tasks;


namespace Yavsc.Abstract.Workflow
{
    public interface IExecutionData
    {
         Task Payload { get; }

         ITaskMetaData MetaData { get; }
         
         string [] Args { get; }
        IList<IMayBeFixable> Faults { get; } 

    }
}