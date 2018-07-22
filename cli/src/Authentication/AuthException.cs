using System;
using System.Runtime.Serialization;

namespace cli.Authentication
{
    [Serializable]
    internal class AuthException : Exception
    {
        private object p;

        public AuthException()
        {
        }

        public AuthException(object p)
        {
            this.p = p;
        }

        public AuthException(string message) : base(message)
        {
        }

        public AuthException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AuthException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}