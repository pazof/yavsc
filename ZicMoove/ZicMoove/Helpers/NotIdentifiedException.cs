using System;

namespace ZicMoove
{
    public class NotIdentifiedException : Exception
    {
        public NotIdentifiedException(string message) : base(message) { }
    }
}

