using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

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
/// Also adds <see cref="TestUserStartupFilter"/> which installs
/// <see cref="TestUserMiddleware"/> so that <c>User.GetUserId()</c>
/// in user code sees a logged-in identity derived from the same
/// header.
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

            // Register the test middleware and its startup filter.
            // The startup filter wraps the production pipeline so
            // TestUserMiddleware runs after UseAuthentication/Authorization.
            services.AddTransient<TestUserMiddleware>();
            services.AddTransient<IStartupFilter, TestUserStartupFilter>();
        });
    }
}
