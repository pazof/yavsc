namespace Yavsc.Abstract.Workflow
{
    public class TaskManager : ITaskRunnerProvider
    {
        public ITaskRunner FindRunner(string runnerName)
        {
            throw new System.NotImplementedException();
        }
    }
}