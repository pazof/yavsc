
using IdentityServer8.EntityFramework.Entities;
using IdentityServer8.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Yavsc;
using Yavsc.Extensions;
using Yavsc.Models;
using Client = IdentityServer8.EntityFramework.Entities.Client;

namespace isnd.tests
{

    [CollectionDefinition("Web server collection")]
    public class WebServerFixture : IDisposable
    {
        private static readonly Lazy<X509Certificate2> _selfSignedCertificate = new Lazy<X509Certificate2>(CreateSelfSignedCertificate);
        private static readonly object _sync = new object();
        private static WebApplication? _app;
        private static bool _isInitialized = false;
        private static int _instanceCount = 0;
        private static readonly List<string> _sharedAddresses = new List<string>();
        private static string? _sharedTestClientId;
        private static string? _sharedTestClientSecret;
        private static string? _sharedTestingUserName;
        private static string? _sharedTestingUserPassword;
        private static string? _sharedTestingUserEmail;
        private static IServiceProvider? _sharedServices;
        private static IConfiguration? _sharedConfiguration;
        private static SiteSettings? _sharedSiteSettings;
        private static Microsoft.Extensions.Logging.ILogger? _sharedLogger;

        public List<string> Addresses { get; private set; } = new List<string>();
        public Microsoft.Extensions.Logging.ILogger? Logger { get; internal set; }

        private SiteSettings? siteSettings;

        public IConfiguration? Configuration { get; private set; }

        public string? TestClientId { get; private set; }

        public IServiceProvider? Services { get; private set; }
        public string? TestingUserName { get; private set; }
        public string? TestingUserPassword { get; private set; }

        public string? ProtectedTestingApiKey { get; internal set; }
        public ApplicationUser? TestingUser { get; private set; }
        public bool DbCreated { get; internal set; }
        public SiteSettings? SiteSettings { get => siteSettings; set => siteSettings = value; }
        public string? TestClientSecret { get; set; }
        public string? TestingUserEmail { get; set; }
        public WebServerFixture()
        {
            lock (_sync)
            {
                _instanceCount++;
                if (!_isInitialized)
                {
                    SetupHost().Wait();
                    _isInitialized = true;
                }

                CopySharedState();
            }
        }

        public void Dispose()
        {
            lock (_sync)
            {
                _instanceCount--;
                if (_instanceCount == 0 && _app != null)
                {
                    _app.StopAsync().Wait();
                    _app = null;
                    _isInitialized = false;
                    _sharedAddresses.Clear();
                    _sharedServices = null;
                    _sharedConfiguration = null;
                    _sharedSiteSettings = null;
                    _sharedLogger = null;
                    _sharedTestClientId = null;
                    _sharedTestClientSecret = null;
                    _sharedTestingUserName = null;
                    _sharedTestingUserPassword = null;
                    _sharedTestingUserEmail = null;
                }
            }
        }

        private void CopySharedState()
        {
            Addresses = new List<string>(_sharedAddresses);
            Logger = _sharedLogger;
            Configuration = _sharedConfiguration;
            Services = _sharedServices;
            SiteSettings = _sharedSiteSettings;
            TestClientId = _sharedTestClientId;
            TestClientSecret = _sharedTestClientSecret;
            TestingUserName = _sharedTestingUserName;
            TestingUserPassword = _sharedTestingUserPassword;
            TestingUserEmail = _sharedTestingUserEmail;
        }
       
        public async Task SetupHost()
        {

            var builder = WebApplication.CreateBuilder();
            builder.Environment.EnvironmentName = "Development";
            // Set ContentRoot to the Yavsc.Org project directory so WebRootPath resolves correctly
            var testAssemblyLocation = AppDomain.CurrentDomain.BaseDirectory;
            var yavscOrgPath = Path.GetFullPath(Path.Combine(testAssemblyLocation, "../../src/Yavsc.Org"));
            builder.Environment.ContentRootPath = yavscOrgPath;
            
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [$"ConnectionStrings:{YavscConstants.YavscConnectionStringName}"] = "InMemory"
                });

            // Configure Kestrel for HTTPS with self-signed certificate on a dynamic port
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Listen(IPAddress.Loopback, 0, listenOptions =>
                {
                    listenOptions.UseHttps(_selfSignedCertificate.Value);
                });
            });

            Configuration = builder.Configuration;

            _app = builder.ConfigureWebAppServices();
            Services = _app.Services;
            SiteSettings = _app.Services.GetRequiredService<IOptions<SiteSettings>>().Value;

            using (var migrationScope = _app.Services.CreateScope())
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
                TestingUser = await db.Users.FirstOrDefaultAsync(u => u.UserName == TestingUserName);
            }

            // Seed IdentityServer ConfigurationDbContext with API resources and scopes
            using (var configScope = _app.Services.CreateScope())
            {
                try
                {
                    var configDbContext = configScope.ServiceProvider.GetService<IdentityServer8.EntityFramework.DbContexts.ConfigurationDbContext>();
                    if (configDbContext != null)
                    {
                        configDbContext.Database.EnsureCreated();
                        
                        // Add test API scope if it doesn't exist
                        var testScope = configDbContext.ApiScopes.FirstOrDefault(s => s.Name == "test");
                        if (testScope == null)
                        {
                            configDbContext.ApiScopes.Add(new IdentityServer8.EntityFramework.Entities.ApiScope 
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
                                    new IdentityServer8.EntityFramework.Entities.ApiResourceScope
                                    {
                                        Scope = "test"
                                    }
                                }
                            };
                            configDbContext.ApiResources.Add(apiResource);
                            configDbContext.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _sharedLogger?.LogWarning($"Failed to seed ConfigurationDbContext: {ex.Message}");
                    // Don't fail the fixture if seeding fails
                }
            }

            await _app!.ConfigurePipeline();
            _app.UseSession();
            await _app.StartAsync();

            _sharedServices = _app.Services;
            _sharedConfiguration = Configuration;
            _sharedSiteSettings = SiteSettings;
            _sharedTestClientId = TestClientId;
            _sharedTestClientSecret = TestClientSecret;
            _sharedTestingUserName = TestingUserName;
            _sharedTestingUserPassword = TestingUserPassword;
            _sharedTestingUserEmail = TestingUserEmail;
            _sharedLogger = _app.Services.GetRequiredService<ILoggerFactory>().CreateLogger<WebServerFixture>();
            Logger = _sharedLogger;

            var server = _app.Services.GetRequiredService<IServer>();
            var addressFeatures = server.Features.Get<IServerAddressesFeature>();

            if (addressFeatures?.Addresses != null)
            {
                _sharedAddresses.Clear();
                foreach (var address in addressFeatures.Addresses)
                {
                    _sharedAddresses.Add(address);
                    Addresses.Add(address);
                }
            }
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

                var userManager =
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                TestingUser = new ApplicationUser
                {
                    UserName = testingUserName,
                    Email = testingUserName + "@example.com",
                    EmailConfirmed = true
                };

                var result = userManager.CreateAsync(TestingUser, password).Result;

                Assert.True(result.Succeeded);

                ApplicationDbContext dbContext =
                scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                TestingUser = dbContext.Users.FirstOrDefault(u => u.UserName == testingUserName);
            }
        }

        private static X509Certificate2 CreateSelfSignedCertificate()
        {
            var rsa = RSA.Create(2048);
            var certRequest = new CertificateRequest("CN=localhost", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            certRequest.CertificateExtensions.Add(
                new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, false));

            certRequest.CertificateExtensions.Add(
                new X509EnhancedKeyUsageExtension(
                    new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false));

            var certificate = certRequest.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)), new DateTimeOffset(DateTime.UtcNow.AddDays(3650)));
            return certificate;
        }
    }
}
