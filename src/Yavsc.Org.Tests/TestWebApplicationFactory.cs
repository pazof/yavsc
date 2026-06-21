using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Yavsc.Org.Tests;

/// <summary>
/// WebApplicationFactory-based fixture for integration tests that need
/// to override services registered by the production <c>Program</c>.
/// Uses the in-memory <see cref="TestServer"/> so tests can hit real
/// HTTP endpoints without sockets or self-signed certificates.
///
/// Currently overrides <see cref="TestAuthPolicyProvider"/> so that
/// <c>[Authorize("AdministratorOnly")]</c> (and any other policy
/// requiring a role) is satisfied by sending an
/// <c>X-Test-Role: Administrator</c> header, without a real login.
/// Also injects a middleware that promotes the same header into a
/// real <see cref="ClaimsPrincipal"/> on <c>HttpContext.User</c> so
/// that user code reading <c>User.GetUserId()</c> sees a logged-in
/// identity.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // UseDevelopmentEnvironment triggers the dev signing credential
        // path in the production startup, so we don't need a real cert
        // to satisfy IdentityServer at boot.
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            // Replace the production IAuthorizationPolicyProvider with
            // the test one. The default registered by AddAuthorization
            // becomes irrelevant: any GetPolicyAsync call is routed here.
            services.AddSingleton<IAuthorizationPolicyProvider, TestAuthPolicyProvider>();
        });

        // Promote the X-Test-Role header to an authenticated identity
        // on the request, so anything that reads User.GetUserId() (or
        // any other claim-based helper) downstream sees a logged-in
        // user. The policy provider above only short-circuits
        // [Authorize(...)] checks; it does not touch HttpContext.User.
        builder.Configure(app =>
        {
            app.Use(InjectTestUser);
        });
    }

    private static RequestDelegate InjectTestUser(RequestDelegate next)
    {
        return async ctx =>
        {
            var role = ctx.Request.Headers[TestAuthPolicyProvider.HeaderName].ToString();
            if (!string.IsNullOrEmpty(role) &&
                (ctx.User.Identity is null || !ctx.User.Identity.IsAuthenticated))
            {
                var identity = new ClaimsIdentity(
                    new[]
                    {
                        new Claim(
                            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                            role),
                        new Claim(ClaimTypes.NameIdentifier, "test-user"),
                    },
                    authenticationType: "TestAuth");
                ctx.User = new ClaimsPrincipal(identity);
            }
            await next(ctx);
        };
    }
}
