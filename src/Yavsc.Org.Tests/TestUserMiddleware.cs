using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Yavsc.Tests.Shared;

namespace Yavsc.Org.Tests;

/// <summary>
/// Middleware that promotes the <c>X-Test-Role</c> request header to
/// an authenticated <see cref="ClaimsPrincipal"/> on
/// <see cref="HttpContext.User"/>. Lets downstream code (controllers,
/// services) call <c>User.GetUserId()</c> and other claim-based
/// helpers as if a real login had happened, while
/// <see cref="TestAuthPolicyProvider"/> independently short-circuits
/// <c>[Authorize(...)]</c> policy checks.
///
/// This middleware is added to the pipeline by
/// <see cref="TestUserStartupFilter"/> so it runs after the
/// production authentication middleware; setting User before
/// <c>UseAuthentication</c> would have it overwritten on the next
/// middleware.
/// </summary>
public class TestUserMiddleware : IMiddleware
{
    public const string UserId = "test-user";

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var role = context.Request.Headers[TestAuthPolicyProvider.HeaderName].ToString();
        if (!string.IsNullOrEmpty(role) &&
            (context.User.Identity is null || !context.User.Identity.IsAuthenticated))
        {
            var identity = new ClaimsIdentity(
                new[]
                {
                    new Claim(
                        "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                        role),
                    new Claim(ClaimTypes.NameIdentifier, UserId),
                },
                authenticationType: "TestAuth");
            context.User = new ClaimsPrincipal(identity);
        }
        return next(context);
    }
}
