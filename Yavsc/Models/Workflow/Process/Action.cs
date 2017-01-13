using System.Threading.Tasks;

namespace Yavsc.Models.Process
{
     public abstract class Action<TResult,TInput>
    {
        public abstract  Task<TResult> GetTask(TInput data);
    }
}