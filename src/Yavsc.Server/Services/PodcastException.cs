namespace Yavsc.Services;

public sealed class PodcastException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
