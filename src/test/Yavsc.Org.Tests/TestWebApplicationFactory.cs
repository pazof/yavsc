using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Tests.Shared;

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
///
/// Each instance gets its own in-memory database, identified by a
/// GUID generated in the constructor. The connection string
/// (<c>ConnectionStrings:YavscConnection</c>) is set as an
/// environment variable (<c>ConnectionStrings__YavscConnection</c>)
/// in the constructor and unset in <see cref="Dispose"/>, so the
/// production <c>AddIdentityDBAndStores</c> registers <c>DbContext</c>
/// instances against this fixture's own store. Without this, the
/// <c>"InMemory"</c> connection string from
/// <c>appsettings-org.Testing.json</c> would route every
/// <see cref="TestWebApplicationFactory"/> instance — and any
/// <see cref="WebServerFixture"/> running in the same process — to
/// the same backing store, leaking state between fixtures.
///
/// Env vars are used (rather than <c>ConfigureAppConfiguration</c> or
/// <c>UseSetting</c>) because <c>WebApplicationFactory</c> applies
/// those too late: <c>Program.Main</c> has already captured the
/// connection string in <c>AddIdentityDBAndStores</c> by the time
/// the test host's overrides take effect. Env vars are the last
/// provider added in <c>AddConfiguration</c> (see
/// <c>Yavsc.Server/Helpers/ConfigHelpers.cs</c>), so they win.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _fixtureId = Guid.NewGuid().ToString("N");

    // ASP.NET Core's environment-variable configuration provider uses
    // the key ConnectionStrings__YavscConnection (double underscore
    // for the section separator). Set it before the host starts so
    // the per-fixture connection string wins over
    // appsettings-org.Testing.json. We do NOT touch the appsettings
    // file; env vars take precedence in the configuration pipeline
    // (see AddConfiguration in Yavsc.Server/Helpers/ConfigHelpers.cs,
    // which adds AddEnvironmentVariables last).
    private static readonly object _envLock = new();
    private bool _envSet;

    public TestWebApplicationFactory()
    {
        lock (_envLock)
        {
            Environment.SetEnvironmentVariable(
                "ConnectionStrings__YavscConnection",
                InMemoryDatabaseName.For(_fixtureId));
            _envSet = true;
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // UseEnvironment("Testing") puts the host in a dedicated
        // configuration environment so AddConfiguration("org") in
        // Program.Main loads the optional appsettings-org.Testing.json
        // file (which overrides the connection string and SMTP section
        // for the test host). See that file for the values.
        // We don't use "Development" because that environment is also
        // used by the dev launcher and would change the signing
        // credential path in IdentityServer; "Testing" is unambiguous.
        builder.UseEnvironment("Testing");

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

    protected override void Dispose(bool disposing)
    {
        if (disposing && _envSet)
        {
            lock (_envLock)
            {
                Environment.SetEnvironmentVariable(
                    "ConnectionStrings__YavscConnection", null);
                _envSet = false;
            }
        }
        base.Dispose(disposing);
    }
}
