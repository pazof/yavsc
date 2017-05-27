using System.Collections.Generic;

namespace Yavsc
{
    public interface ILetter<T> : IEqualityComparer<T> {
    }
    public interface IWord<TLetter> where TLetter : ILetter<TLetter>
    {
        IWord<TLetter> Aggregate(TLetter other);
    }

    public interface ICode<TLetter> : IEnumerable<TLetter> where TLetter : ILetter<TLetter>
    {
        /// <summary>
        /// Checks that (b!=c) => a.b != a.c
        /// </summary>
        /// <returns></returns>
        bool Validate();

        IWord<TLetter> CreateWord(TLetter letter);
    }
}
