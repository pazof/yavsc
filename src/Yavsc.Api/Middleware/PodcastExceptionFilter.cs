using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Yavsc.Services;

namespace Yavsc.Api.Middleware;

public sealed class PodcastExceptionFilter(ILogger<PodcastExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;
        if (exception is not (PodcastException or IOException or JsonException))
            return;
        var status = exception is PodcastException podcast ? podcast.StatusCode : StatusCodes.Status503ServiceUnavailable;
        var detail = exception is PodcastException ? exception.Message : "Le stockage des podcasts est indisponible.";
        logger.LogWarning(exception, "Podcast request failed with status {Status}", status);
        context.Result = new ObjectResult(new ProblemDetails { Status = status, Title = detail }) { StatusCode = status };
        context.ExceptionHandled = true;
    }
}
