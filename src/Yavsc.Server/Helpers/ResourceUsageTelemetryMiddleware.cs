#nullable enable
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Yavsc.Models;

namespace Yavsc.Server.Helpers;

public sealed class ResourceUsageTelemetryMiddleware(
    RequestDelegate next,
    IServiceScopeFactory scopeFactory,
    ILogger<ResourceUsageTelemetryMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, IResourceUsageService usage)
    {
        var userId = context.User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId) || IsExempt(context))
        {
            await next(context);
            return;
        }

        var userName = context.User.Identity?.Name ?? userId;
        var requestBytes = context.Request.ContentLength ?? 0L;
        if (!await usage.TryAdmitAsync(userId, userName, 1m, ToMb(requestBytes), 0,
            context.RequestAborted))
        {
            logger.LogWarning("Resource quota refused a request for user {UserId}", userId);
            await WriteQuotaProblemAsync(context);
            return;
        }

        var originalRequest = context.Request.Body;
        var originalResponse = context.Response.Body;
        using var request = new ResourceUsageCountingStream(originalRequest);
        using var response = new ResourceUsageCountingStream(originalResponse);
        context.Request.Body = request;
        context.Response.Body = response;
        try
        {
            await next(context);
        }
        finally
        {
            context.Request.Body = originalRequest;
            context.Response.Body = originalResponse;
            // Known ingress was charged at admission. Count streamed responses
            // and unknown-length ingress once, even on failure or cancellation.
            var extraBytes = Math.Max(0L, request.BytesRead - requestBytes) + response.BytesWritten;
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await ResourceUsageTracker.RecordAsync(db, userId, userName,
                bandwidthMb: ToMb(extraBytes), actorUserId: userId);
        }
    }

    public static async Task WriteQuotaProblemAsync(HttpContext context)
    {
        var site = context.RequestServices.GetRequiredService<IOptions<SiteSettings>>().Value;
        var termsUrl = new Uri(new Uri(site.Authority.TrimEnd('/') + "/"), "Home/CGU").AbsoluteUri;
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Quota de consommation insuffisant",
            Detail = "Cette opération dépasse le quota disponible. Consultez votre consommation ou contactez l'administration.",
            Instance = context.Request.Path
        };
        problem.Extensions["code"] = "resource_quota_exceeded";
        problem.Extensions["usagePath"] = Constants.APIPrefix + "/" + Constants.ResourceUsagePath + "/me";
        problem.Extensions["termsUrl"] = termsUrl;
        problem.Extensions["subscriptionUrl"] = new Uri(new Uri(site.Authority.TrimEnd('/') + "/"), "ResourceSubscription").AbsoluteUri;
        await context.Response.WriteAsJsonAsync(problem,
            options: null, contentType: "application/problem+json", cancellationToken: context.RequestAborted);
    }

    private static decimal ToMb(long bytes) => (decimal)bytes / (1024m * 1024m);

    private static bool IsExempt(HttpContext context)
        => context.GetEndpoint()?.Metadata.GetMetadata<ResourceUsageExemptAttribute>() is not null
            || context.Request.Path.Equals("/login", StringComparison.OrdinalIgnoreCase)
            || context.Request.Path.Equals("/refresh", StringComparison.OrdinalIgnoreCase)
            || context.Request.Path.Equals("/logout", StringComparison.OrdinalIgnoreCase);
}
