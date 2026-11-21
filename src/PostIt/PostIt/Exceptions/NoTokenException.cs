using System;

namespace Yavsc.Exceptions;

public class NoTokenException : Exception
{
    public NoTokenException()
        : base("No token available.")
    {
    }

    public NoTokenException(string message)
        : base(message)
    {
    }

    public NoTokenException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
