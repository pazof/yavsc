namespace Yavsc.Server.Exceptions;

[Serializable]
public class AuthorizationFailureException : Exception
{
    public AuthorizationFailureException(Microsoft.AspNetCore.Authorization.AuthorizationResult auth) : base(auth?.Failure?.ToString()??auth?.ToString()??"AuthorizationResult failure")
    {
    }

    public AuthorizationFailureException(string? message) : base(message)
    {
    }

    public AuthorizationFailureException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
