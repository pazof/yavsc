
using System;

namespace ZicMoove.Interfaces
{
    public interface IComponentContext
    {
        T Resolve<T>() ;
        object Resolve(Type t);
    }
}
