using System;

namespace ZicMoove
{
    public class ServiceNotAvailable : Exception
    {
        public ServiceNotAvailable(string message, Exception innerException) : base(message, innerException) { }

    }
}