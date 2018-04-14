using System.Threading.Tasks;

namespace Yavsc.Abstract.Workflow
{
    public interface ITaskRunner
    {
        IExecutionData Run( ITaskMetaData taskMetaData, string [] args);
    }
}