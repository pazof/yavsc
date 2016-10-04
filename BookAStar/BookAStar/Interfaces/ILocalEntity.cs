using System;

namespace BookAStar
{
    public interface ILoadable
    {
        void Load();
    }

    public interface ILocalEntity<V, K> : ILoadable where K : IEquatable<K>
    {
        V CurrentItem { get; }

        Func<V, K> GetKey { get; set; }

        V LocalGet(K key);

        void Merge(V item);

    }
}