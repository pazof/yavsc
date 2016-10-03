
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace BookAStar
{
    public class LocalEntity<V, K> : ObservableCollection<V> where K : IEquatable<K>
    {
        public V CurrentItem { get; protected set; }
        protected Func<V, K> GetKey { get; set; }
        public LocalEntity(Func<V, K> getKey) : base()
        {
            if (getKey == null) throw new InvalidOperationException();
            GetKey = getKey;
        }
        public virtual void Merge(V item)
        {
            var key = GetKey(item);
            if (this.Any(x => GetKey(x).Equals(key)))
            {
                Remove(LocalGet(key));
            }
            Add(item);
            CurrentItem = item;
        }
        public V LocalGet(K key)
        {
            CurrentItem = this.Single(x => GetKey(x).Equals(key));
            return CurrentItem;
        }
    }

}