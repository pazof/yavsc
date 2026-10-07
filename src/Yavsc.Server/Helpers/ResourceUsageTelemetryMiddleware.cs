#nullable enable
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Models;

namespace Yavsc.Server.Helpers;

public sealed class ResourceUsageTelemetryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;

    public ResourceUsageTelemetryMiddleware(
        RequestDelegate next,
        IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _scopeFactory = scopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            await _next(context);
            return;
        }

        var userName = context.User.Identity?.Name ?? userId;
        var requestBytes = context.Request.ContentLength ?? 0L;

        try
        {
            await _next(context);
        }
        finally
        {
            var responseBytes = context.Response.ContentLength ?? 0L;
            var totalBytes = requestBytes + responseBytes;
            var bandwidthMb = totalBytes <= 0 ? 0m : (decimal)totalBytes / (1024m * 1024m);

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await ResourceUsageTracker.RecordAsync(
                db,
                userId,
                userName,
                apiCalls: 1m,
                bandwidthMb: bandwidthMb,
                actorUserId: userId);
        }
    }
}
