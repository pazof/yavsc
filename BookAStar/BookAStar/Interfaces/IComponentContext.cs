
using System;

namespace BookAStar.Interfaces
{
    public interface IComponentContext
    {
        T Resolve<T>() ;
        object Resolve(Type t);
    }
}
