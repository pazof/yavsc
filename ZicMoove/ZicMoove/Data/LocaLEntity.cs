
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZicMoove.Settings;

namespace ZicMoove.Data
{
    public class LocalEntity<V, K> : ObservableCollection<V>, ILocalEntity<V, K> where K : IEquatable<K>
    {
        public V CurrentItem { get; protected set; }

        public Func<V, K> GetKey { get; set; }

        public LocalEntity(Func<V, K> getKey) : base()
        {
            if (getKey == null)
                // choose please, because of the genesis 
                throw new InvalidOperationException ("A key must be defined");
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

        public virtual bool Load()
        {
            return this.Populate<V,K>();
        }

        public virtual bool Load(string subKey)
        {
            return (this.Populate<V,K>(subKey));
        }
        public virtual bool Seek(int index)
        {
            
            if (this.Count>index && index >=0)
            {
                CurrentItem = this[index];
                return true;
            }
            return false;
        }
    }

}