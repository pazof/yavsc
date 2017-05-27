
namespace Yavsc.Models.IT.Modeling
{
    public abstract class Letter<T> : ILetter<T>
    {
        public abstract bool Equals(T x, T y);

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }

    }
}
