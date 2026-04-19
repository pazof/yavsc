using IdentityServer8.EntityFramework.Entities;
using IdentityServer8.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
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
        public List<string> Addresses { get; private set; } = new List<string>();
        public Microsoft.Extensions.Logging.ILogger? Logger { get; internal set; }

        private SiteSettings? siteSettings;

        public IConfiguration? Configuration { get; private set; }

        private WebApplication? app;
        public string? TestClientId { get; private set; }

        public IServiceProvider? Services { get; private set; }
        public string? TestingUserName { get; private set; }
        public string? TestingUserPassword { get; private set; }

        public string? ProtectedTestingApiKey { get; internal set; }
        public ApplicationUser? TestingUser { get; private set; }
        public bool DbCreated { get; internal set; }
        public SiteSettings? SiteSettings { get => siteSettings; set => siteSettings = value; }
        public string? TestClientSecret { get; set; }

        public WebServerFixture()
        {
            SetupHost().Wait();
        }

        public void Dispose()
        {
            if (app != null)
                app.StopAsync().Wait();
        }
        void ConfigureLogger() => Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
            .Enrich.FromLogContext()
    // uncomment to write to Azure diagnostics stream
    //.WriteTo.File(
    //    @"D:\home\LogFiles\Application\identityserver.txt",
    //    fileSizeLimitBytes: 1_000_000,
    //    rollOnFileSizeLimit: true,
    //    shared: true,
    //    flushToDiskInterval: TimeSpan.FromSeconds(1))
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
    .CreateLogger();
        public async Task SetupHost()
        {

            var builder = WebApplication.CreateBuilder();
            builder.Environment.EnvironmentName = "Development";
            ConfigureLogger();
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["UseInMemoryDatabase"] = "true",
                    ["UseTestEmailSender"] = "true",
                    ["Smtp:Host"] = "localhost",
                    ["Smtp:Port"] = "25",
                    ["Smtp:SenderName"] = "Yavsc Test",
                    ["Smtp:SenderEmail"] = "test@example.com"
                });

            Configuration = builder.Configuration;

            this.app = builder.ConfigureWebAppServices();
            Services = app.Services;
            SiteSettings = app.Services.GetRequiredService<IOptions<SiteSettings>>().Value;
            
            using (var migrationScope = app.Services.CreateScope())
            {
                var db = migrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                TestingUserName = "Tester";
                TestingUserPassword = "tesT456+*";
                TestClientId = "testClientId";
                TestClientSecret = Guid.CreateVersion7().ToString();
                EnsureUser(TestingUserName, TestingUserPassword);
                AddAuthorizedClient(TestClientId, TestClientSecret);
                TestingUser = await db.Users.FirstOrDefaultAsync(u => u.UserName == TestingUserName);
            }
            await app!.ConfigurePipeline();
            app.UseSession();
            await app.StartAsync();



            var logFactory = app.Services.GetRequiredService<ILoggerFactory>();
            Logger = logFactory.CreateLogger<WebServerFixture>();

            var server = app.Services.GetRequiredService<IServer>();

            var addressFeatures = server.Features.Get<IServerAddressesFeature>();

            if (addressFeatures?.Addresses != null)
            {
                foreach (var address in addressFeatures.Addresses)
                {
                    Addresses.Add(address);
                }
            }
        }

        private void AddAuthorizedClient(string testClientId, string testClientSecret)
        {
            using (IServiceScope scope = app!.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                Client testingClient = new Client
                {
                    ClientId = testClientId,
                    AccessTokenLifetime = 3600000,
                    AccessTokenType = 1,
                    BackChannelLogoutUri = SiteSettings!.Audience,
                    ClientName = "Testing client",
                    Enabled = true
                };
                db.Clients.Add(testingClient);
                db.SaveChanges();
                ClientSecret secret = new ClientSecret
                {
                    Value = testClientSecret.Sha256(),
                    ClientId = testingClient.Id
                };
                db.ClientSecrets.Add(secret);

                var testOrigin = new ClientCorsOrigin
                {
                    ClientId = testingClient.Id,
                    Origin = SiteSettings!.Audience

                };
                db.ClientCorsOrigins.Add(testOrigin);
                db.ClientGrantTypes.Add(new ClientGrantType
                {
                    ClientId = testingClient.Id,
                    GrantType = "client_credentials"
                });
                db.ClientGrantTypes.Add(new ClientGrantType
                {
                    ClientId = testingClient.Id,
                    GrantType = "password"
                });
                db.ClientGrantTypes.Add(new ClientGrantType
                {
                    ClientId = testingClient.Id,
                    GrantType = "code"
                });
                db.ClientScopes.Add(new ClientScope
                {
                    ClientId = testingClient.Id,
                    Scope = "test"
                });
                db.ApiScopes.Add(new IdentityServer8.EntityFramework.Entities.ApiScope
                {
                    Name = "test",
                    Enabled = true
                });
                db.ClientRedirectUris.Add(new ClientRedirectUri
                {
                    ClientId = testingClient.Id,
                    RedirectUri = SiteSettings!.Audience

                });

                db.SaveChanges();
            }
        }

        public void EnsureUser(string testingUserName, string password)
        {
            if (TestingUser == null)
            {
                using IServiceScope scope = app!.Services.CreateScope();

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
    }
}
