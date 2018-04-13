using System;
using System.Collections;
using System.Collections.Generic;

namespace Yavsc.Abstract.IT
{

    public class CharArray : List<char>, IEnumerable<char>, IList<char>
    {

        public CharArray (char [] charArray) : base (charArray)
        {

        }
        public CharArray (IList<char> word): base(word) {
            
        }
        public CharArray (IEnumerable<char> word): base(word) {
            
        }

        public IList<char> Aggregate(char other)
        {
             this.Add(other);
             return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class CodeFromChars : List<CharArray>, ICode<char>
    {
        public void AddLetter(IEnumerable<char> letter)
        {
            var candide = new CharArray(letter);

            // TODO build new denied letters: compute the automate

            Add(candide);
        }


        public bool Validate()
        {
            throw new NotImplementedException();
        }

        IEnumerator<IEnumerable<char>> IEnumerable<IEnumerable<char>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}