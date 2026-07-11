using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Yavsc.Models;
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
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    // SQLite in-memory: the connection must stay open for the lifetime
    // of the host, otherwise the in-memory database is destroyed and
    // every new DbContext sees an empty store. We hold the connection
    // here so it is disposed only when the factory is disposed. The
    // Microsoft.Data.Sqlite pool reuses the underlying in-memory store
    // across additional connections opened against the same connection
    // string, as long as the original connection is alive. This is the
    // SQLite equivalent of the EF Core InMemoryDatabaseRoot pattern.
    private readonly SqliteConnection _sharedSqliteConnection = new("Data Source=:memory:");

    public TestWebApplicationFactory()
    {
        _sharedSqliteConnection.Open();
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
            // The production Program.Main calls AddConfiguration("org")
            // and then AddIdentityDBAndStores which calls
            // GetConnectionString("YavscConnection"). The result is
            // "Data Source=:memory:" (from appsettings-org.Testing.json),
            // and the production code path in HostingExtensions routes
            // that to UseSqlite. However, the EF Core in-memory test
            // pattern needs all DbContext instances to see the same
            // store; with a raw "Data Source=:memory:" connection string,
            // each connection opens its own private database. We
            // therefore drop the production DbContext registration and
            // re-register ApplicationDbContext with the shared
            // SqliteConnection held by this factory. Tests that need
            // the schema to exist call EnsureCreated on the resulting
            // DbContext (e.g. ClientControllerCollectionTests seeds a
            // Client row in its constructor).
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(opt =>
                opt.UseSqlite(_sharedSqliteConnection));

            // Replace the production IAuthorizationPolicyProvider with
            // the test one. The default registered by AddAuthorization
            // becomes irrelevant: any GetPolicyAsync call is routed here.
            services.AddSingleton<IAuthorizationPolicyProvider, TestAuthPolicyProvider>();

            // Register the test middleware and its startup filter.
            // The startup filter wraps the production pipeline so
            // TestUserMiddleware runs after UseAuthentication/Authorization.
            services.AddTransient<TestUserMiddleware>();
            services.AddTransient<IStartupFilter, TestUserStartupFilter>();

            // Run EnsureCreated once at host start. With SQLite in-memory
            // and a shared connection, this creates the schema once
            // and the schema persists for the host lifetime. The
            // test code (e.g. ClientControllerCollectionTests seed) can
            // then write rows without having to call EnsureCreated
            // itself. EnsureCreated is idempotent: re-running it on
            // an existing schema is a no-op.
            services.AddHostedService<SqliteEnsureCreatedHostedService>();
        });
    }

    private sealed class SqliteEnsureCreatedHostedService : IHostedService
    {
        private readonly IServiceProvider _services;
        public SqliteEnsureCreatedHostedService(IServiceProvider services)
        {
            _services = services;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return db.Database.EnsureCreatedAsync(cancellationToken);
        }
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _sharedSqliteConnection.Dispose();
        base.Dispose(disposing);
    }
}
