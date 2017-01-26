using System;
using System.Collections;

namespace ZicMoove
{
    public interface ILoadable
    {
        bool Load();
    }

    public interface IPersistentOnDemand : ILoadable
    {
        void Save();
    }

    public interface ILocalEntity<V, K> : IList, ILoadable where K : IEquatable<K>
    {
        V CurrentItem { get; }

        Func<V, K> GetKey { get; set; }

        V LocalGet(K key);

        void Merge(V item);

        bool Seek(int index);
    }
}