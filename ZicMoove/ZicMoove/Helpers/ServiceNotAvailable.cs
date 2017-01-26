using System;

namespace BookAStar
{
    public class ServiceNotAvailable : Exception
    {
        public ServiceNotAvailable(string message, Exception innerException) : base(message, innerException) { }

    }
}