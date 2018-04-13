using System.Threading.Tasks;

namespace Yavsc.Abstract.Workflow
{
    public interface ITaskRunner
    {
        Task Run(string [] args);
    }
}