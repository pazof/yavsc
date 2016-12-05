using System.Threading.Tasks;

namespace Yavsc.Models
{
     public abstract class Action<TResult,TInput>
    {
        public abstract  Task<TResult> GetTask(TInput data);
    }
}