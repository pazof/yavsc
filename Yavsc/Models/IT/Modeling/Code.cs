using System;
using System.Collections;
using System.Collections.Generic;
using Yavsc;

namespace Yavsc.Models.IT.Modeling
{
    public abstract class Code<TLetter> : ICode<TLetter> where TLetter : ILetter<TLetter>
    {
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// !a Count^3 task len
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            foreach  (var letter in this) {
                    var word = this.CreateWord(letter);
                foreach  (var other in this) {
                    IWord<TLetter> first = word.Aggregate(other);
                    foreach  (var tierce in this)
                    {
                        var otherword = word.Aggregate(tierce);
                        if (first.Equals(otherword))
                            return false;
                    }
                }
            }
            return true;
        }

        public abstract IEnumerator<TLetter> GetEnumerator();

        public abstract IWord<TLetter> CreateWord(TLetter letter);
    }
}
