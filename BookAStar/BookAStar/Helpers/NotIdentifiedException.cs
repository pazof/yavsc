using System;

namespace BookAStar
{
    public class NotIdentifiedException : Exception
    {
        public NotIdentifiedException(string message) : base(message) { }
    }
}

