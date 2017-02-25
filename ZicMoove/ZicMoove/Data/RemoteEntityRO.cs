using System;
using System.Linq;

namespace ZicMoove.Data
{
    /// <summary>
    /// Use to not try and update any remote data ...
    /// TODO implementation ...
    /// </summary>
    /// <typeparam name="V"></typeparam>
    /// <typeparam name="K"></typeparam>
    public class RemoteEntityRO<V,K>: RemoteEntity<V,K> where K: IEquatable<K>
    {
        public RemoteEntityRO (string controllerName,
            Func<V,K> getKey) : base(controllerName,getKey)
        {
        }
    }
}
