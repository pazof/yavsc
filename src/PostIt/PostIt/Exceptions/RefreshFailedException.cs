using System;

namespace PostIt.Services;

public sealed class RefreshFailedException : Exception
{
    public bool IsPermanent { get; }
    public RefreshFailedException(string message, bool isPermanent)
        : base(message) => IsPermanent = isPermanent;
}
