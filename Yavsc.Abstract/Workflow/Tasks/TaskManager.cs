using System.Collections.Generic;
using System.Linq;

namespace Yavsc.Abstract.Workflow
{
    public class TaskManager : ITaskRunnerProvider
    {
        List<ITaskRunner> runners = new List<ITaskRunner>();
        public void Register(ITaskRunner runner)
        {
            runners.Add(runner);
        }
        public ITaskRunner[] FindRunner(string runnerName)
        {
            if (string.IsNullOrWhiteSpace(runnerName))
            return runners.ToArray();
           return runners.Where(r => r.GetType().Name.IndexOf(runnerName.Trim())>=0).ToArray();
        }
    }
}