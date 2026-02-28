namespace Yavsc.Abstract.Workflow
{
    public interface ITaskRunnerProvider
    {
         ITaskRunner[] FindRunner(string runnerName);
         
    }
}