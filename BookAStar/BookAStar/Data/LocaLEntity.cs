
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BookAStar.Data
{
    public class LocalEntity<V, K> : ObservableCollection<V>, ILocalEntity<V, K> where K : IEquatable<K>
    {
        public V CurrentItem { get; protected set; }

        public Func<V, K> GetKey { get; set; }

        public LocalEntity(Func<V, K> getKey) : base()
        {
            if (getKey == null) throw new InvalidOperationException();
            GetKey = getKey;
            IList<V> l = this;
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
            if (!this.Any(x => GetKey(x).Equals(key)))
                return default(V);
            CurrentItem = this.Single(x => GetKey(x).Equals(key));
            return CurrentItem;
        }

        public void Load()
        {
            this.Populate<V>();
        }

        public void Load(string subKey)
        {
            this.Populate<V>(subKey);
        }
    }

}