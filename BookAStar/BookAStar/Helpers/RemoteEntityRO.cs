using BookAStar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookAStar.Helpers
{
    class RemoteEntityRO<V,K>: RemoteEntity<V,K> where K: IEquatable<K>
    {
        public RemoteEntityRO (string controllerName,
            Func<V,K> getKey) : base(controllerName,getKey)
        {
        }

        protected override void UpdateOrAdd(V item)
        {
            var key = GetKey(item);
            if (this.Any(x => GetKey(x).Equals(key))) { return; }
            Add(item);
        }
    }
}
