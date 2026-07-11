using IdentityServer8.EntityFramework.Entities;
using IdentityServer8.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Yavsc;
using Yavsc.Extensions;
using Yavsc.Interfaces;
using Yavsc.Models;
using Yavsc.Server.Helpers;
using Yavsc.Tests.Shared;
using Client = IdentityServer8.EntityFramework.Entities.Client;
using Yavsc.Org.Tests.Fakes;

namespace Yavsc.Org.Tests;

/// <summary>
/// Specialisation of <see cref="WebHostFixture"/> for the Yavsc.Org
/// host. Adds:
/// <list type="bullet">
///   <item><description>In-memory configuration (<c>ConnectionStrings</c>,
///   <c>Smtp</c>) before <c>ConfigureWebAppServices</c> runs.</description></item>
///   <item><description>Test-only <see cref="TestAuthPolicyProvider"/>
///   (from <c>Yavsc.Tests.Shared</c>) swapped in for
///   <see cref="IAuthorizationPolicyProvider"/>.</description></item>
///   <item><description>Recording <see cref="RecordingSmtpClientFactory"/>
///   fake for <see cref="ISmtpClientFactory"/>.</description></item>
///   <item><description>IdentityServer8 client + API scope + test user
///   seeded into the in-memory database.</description></item>
/// </list>
/// All cross-cutting Kestrel / cert / address plumbing is inherited
/// from <see cref="WebHostFixture"/>.
/// </summary>
[CollectionDefinition("Yavsc Server")]
public sealed class WebServerFixture : WebHostFixture
{
    private static IConfiguration? _sharedConfiguration;
    private static SiteSettings? _sharedSiteSettings;
    private static ILogger? _sharedLogger;
    private static string? _sharedTestClientId;
    private static string? _sharedTestClientSecret;
    private static string? _sharedTestingUserName;
    private static string? _sharedTestingUserPassword;
    private static string? _sharedTestingUserEmail;
    private static RecordingSmtpClientFactory? _sharedSmtpClientFactory;

    public IConfiguration? Configuration { get; private set; }
    public string? TestClientId { get; private set; }
    public string? TestClientSecret { get; set; }
    public string? TestingUserName { get; private set; }
    public string? TestingUserPassword { get; private set; }
    public string? TestingUserEmail { get; set; }
    public string? ProtectedTestingApiKey { get; internal set; }
    public ApplicationUser? TestingUser { get; private set; }
    public bool DbCreated { get; internal set; }
    public SiteSettings? SiteSettings { get; set; }
    public RecordingSmtpClientFactory? SmtpClientFactory { get; private set; }
    public ILogger? Logger { get; internal set; }

    protected override WebApplication BuildApp(WebApplicationBuilder builder)
    {
        // WebApplication.CreateBuilder defaults WebRootPath to
        // {ContentRoot}/wwwroot. The test assembly runs from
        // src/Yavsc.Org.Tests/bin/.../, which has no wwwroot of
        // its own — so point the host at the Yavsc.Org project's
        // wwwroot so that ConfigurePipeline's static-assets middleware
        // can resolve it. The AddConfiguration extension takes care of
        // that plus the in-memory overrides below.
        builder.AddConfiguration(null).AddInMemoryCollection(new Dictionary<string, string?>
            {
                // The EF Core in-memory provider cannot materialise
                // entity types from IdentityServer8 (see forgejo#3):
                // it crashes with IndexOutOfRangeException on the
                // multi-Include query in ClientController.LoadClientAsync
                // and on per-collection LoadAsync. SQLite in-memory is
                // a transient, file-less store that uses the same
                // connection string semantics as the InMemory provider
                // ("keep the connection open for the host lifetime")
                // but actually executes SQL, so it handles
                // navigation-property entities correctly. The
                // HostingExtensions code path detects this connection
                // string and routes to UseSqlite. See
                // doc/testing.md for the test-driver policy.
                [$"ConnectionStrings:{YavscConstants.YavscConnectionStringName}"] = "Data Source=:memory:",
                // SMTP test config: UserName non-null so MailSender
                // exercises the Authenticate branch — the
                // RecordingSmtpClient captures it.
                ["Smtp:Host"] = "smtp.test.local",
                ["Smtp:Port"] = "465",
                ["Smtp:UserName"] = "test-user",
                ["Smtp:Password"] = "test-pass",
            });

        Configuration = builder.Configuration;

        // Swap the production authorization policy provider for
        // TestAuthPolicyProvider BEFORE ConfigureWebAppServices
        // runs. ConfigureWebAppServices calls builder.Build() at
        // the end, which freezes the service collection. Tests
        // can satisfy [Authorize("AdministratorOnly")] (and any
        // other policy that requires a role) by sending an
        // X-Test-Role header; the production policy is replaced
        // by the test one via the last-write-wins semantics of
        // IServiceCollection.AddSingleton.
        builder.Services.AddSingleton<IAuthorizationPolicyProvider, TestAuthPolicyProvider>();

        // Replace the production ISmtpClientFactory (added later
        // by ConfigureWebAppServices via TryAddSingleton) with a
        // recording fake. By pre-registering here, the prod
        // TryAdd becomes a no-op and tests get a single shared
        // fake they can assert against.
        var smtpFactory = new RecordingSmtpClientFactory();
        builder.Services.AddSingleton<ISmtpClientFactory>(smtpFactory);
        _sharedSmtpClientFactory = smtpFactory;

        var app = builder.ConfigureWebAppServices();
        SiteSettings = app.Services.GetRequiredService<IOptions<SiteSettings>>().Value;

        using (var migrationScope = app.Services.CreateScope())
        {
            var db = migrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            TestingUserName = "Tester";
            TestingUserPassword = "Test123!";
            TestClientId = "testClientId";
            TestingUserEmail = "test@no-reply.com";
            TestingUser = null;
            TestClientSecret = Guid.CreateVersion7().ToString();
            EnsureUser(TestingUserName, TestingUserPassword, TestingUserEmail, migrationScope);
            AddAuthorizedClient(migrationScope, TestClientId, TestClientSecret);
            TestingUser = db.Users.FirstOrDefaultAsync(u => u.UserName == TestingUserName).Result;

            // Add test API scope if it doesn't exist
            var testScope = db.ApiScopes.FirstOrDefault(s => s.Name == "test");
            if (testScope == null)
            {
                db.ApiScopes.Add(new IdentityServer8.EntityFramework.Entities.ApiScope
                {
                    Name = "test",
                    Enabled = true,
                    DisplayName = "Test API Scope",
                    Description = "Scope for testing purposes",
                    UserClaims = new List<IdentityServer8.EntityFramework.Entities.ApiScopeClaim>
                    {
                        new IdentityServer8.EntityFramework.Entities.ApiScopeClaim { Type = "role" },
                        new IdentityServer8.EntityFramework.Entities.ApiScopeClaim { Type = "email" }
                    }
                });

                // Add a basic API resource for the test scope
                var apiResource = new IdentityServer8.EntityFramework.Entities.ApiResource
                {
                    Name = "testapi",
                    DisplayName = "Test API",
                    Enabled = true,
                    Scopes = new List<IdentityServer8.EntityFramework.Entities.ApiResourceScope>
                    {
                        new IdentityServer8.EntityFramework.Entities.ApiResourceScope { Scope = "test" }
                    }
                };
                db.ApiResources.Add(apiResource);
                db.SaveChanges();
            }
        }

        _sharedConfiguration = Configuration;
        _sharedSiteSettings = SiteSettings;
        _sharedTestClientId = TestClientId;
        _sharedTestClientSecret = TestClientSecret;
        _sharedTestingUserName = TestingUserName;
        _sharedTestingUserPassword = TestingUserPassword;
        _sharedTestingUserEmail = TestingUserEmail;
        _sharedLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger<WebServerFixture>();
        Logger = _sharedLogger;
        SmtpClientFactory = smtpFactory;

        return app;
    }

    protected override async Task<WebApplication> ConfigurePipelineAsync(WebApplication app)
    {
        // The MSBuild target CopyYavscOrgStaticAssets in
        // Yavsc.Org.Tests.csproj mirrors the Yavsc.Org static
        // assets manifest into the test bin directory. Call
        // MapStaticAssets() with the explicit path so the test
        // host resolves the manifest by file location rather
        // than by {AssemblyName}.staticwebassets.* convention
        // (which would look for Yavsc.Org.Tests.staticwebassets.*,
        // a file we don't produce).
        var testRuntimeManifest = Path.Combine(AppContext.BaseDirectory,
            "Yavsc.Org.staticwebassets.runtime.json");
        return await app.ConfigurePipeline(testRuntimeManifest);
    }

    protected override void CopySpecialisedSharedState()
    {
        TestClientId = _sharedTestClientId;
        TestClientSecret = _sharedTestClientSecret;
        TestingUserName = _sharedTestingUserName;
        TestingUserPassword = _sharedTestingUserPassword;
        TestingUserEmail = _sharedTestingUserEmail;
        SmtpClientFactory = _sharedSmtpClientFactory;
        Configuration = _sharedConfiguration;
        SiteSettings = _sharedSiteSettings;
        Logger = _sharedLogger;
    }

    private void AddAuthorizedClient(IServiceScope scope, string testClientId, string testClientSecret)
    {
        var configDb = scope.ServiceProvider.GetRequiredService<IdentityServer8.EntityFramework.DbContexts.ConfigurationDbContext>();
        if (configDb == null)
            throw new InvalidOperationException("ConfigurationDbContext is not available for IdentityServer client seeding.");

        Client testingClient = new Client
        {
            ClientId = testClientId,
            AccessTokenLifetime = 3600000,
            AccessTokenType = 1,
            ClientName = "Testing client",
            Enabled = true,
            RequireClientSecret = true
        };
        configDb.Set<Client>().Add(testingClient);
        configDb.SaveChanges();

        var apiScope = new IdentityServer8.EntityFramework.Entities.ApiScope
        {
            Name = "test",
            DisplayName = "Test Scope",
            Description = "Scope for testing",
            Enabled = true,
            Required = false,
            ShowInDiscoveryDocument = true,
            Emphasize = false
        };
        configDb.Set<IdentityServer8.EntityFramework.Entities.ApiScope>().Add(apiScope);

        ClientSecret secret = new ClientSecret
        {
            Value = testClientSecret.Sha256(),
            Type = IdentityServer8.IdentityServerConstants.SecretTypes.SharedSecret,
            ClientId = testingClient.Id
        };
        configDb.Set<ClientSecret>().Add(secret);

        configDb.Set<ClientGrantType>().Add(new ClientGrantType
        {
            ClientId = testingClient.Id,
            GrantType = "client_credentials"
        });
        configDb.Set<ClientGrantType>().Add(new ClientGrantType
        {
            ClientId = testingClient.Id,
            GrantType = "password"
        });
        configDb.Set<ClientScope>().Add(new ClientScope
        {
            ClientId = testingClient.Id,
            Scope = "test"
        });

        configDb.SaveChanges();
    }

    public void EnsureUser(string testingUserName, string password, string email, IServiceScope scope)
    {
        if (TestingUser == null)
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            TestingUser = new ApplicationUser
            {
                UserName = testingUserName,
                Email = testingUserName + "@example.com",
                EmailConfirmed = true
            };

            var result = userManager.CreateAsync(TestingUser, password).Result;

            Assert.True(result.Succeeded);

            ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            TestingUser = dbContext.Users.FirstOrDefault(u => u.UserName == testingUserName);
        }
    }
}
