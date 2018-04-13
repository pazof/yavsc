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
            // and check it is determinised
            // Automate a = new Automate();
            Add(candide);
        }

        public bool Validate()
        {
            // this is a n*n task

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

        private class Automate
        {
            int State { get; set; }

            Dictionary<int, Dictionary<int,int>> Transitions { get; set; }

            CodeFromChars Code { get; set; }

            void Compute(CharArray chars)
            {
                if (!Code.Contains(chars)) {
                    State = -1;
                    return;
                }

                if (!Transitions.ContainsKey(State)) {
                    State = -2;
                    return;
                }

                var states = Transitions[State];

                int letter = Code.IndexOf(chars);

                if (!states.ContainsKey(letter))
                    {
                    State = -3;
                    return;
                    }
                    
                State = states[letter];

            }
        }
    }
}