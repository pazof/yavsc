using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Yavsc;
using Yavsc.Models;
using Yavsc.Services;
using yavscTests.Settings;
using Yavsc.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Yavsc.Models.Auth;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace isnd.tests
{

    [CollectionDefinition("Web server collection")]
    public class WebServerFixture : IDisposable
    {
        public List<string> Addresses { get; private set; } = new List<string>();
        public Microsoft.Extensions.Logging.ILogger Logger { get; internal set; }

        private SiteSettings siteSettings;

        public IConfigurationRoot Configuration { get; private set; }

        private WebApplication app;
        public string TestClientId { get; private set; }

        public IServiceProvider Services { get; private set; }
        public string TestingUserName { get; private set; }
        public string TestingUserPassword { get; private set; }
        
        public string ProtectedTestingApiKey { get; internal set; }
        public ApplicationUser TestingUser { get; private set; }
        public bool DbCreated { get; internal set; }
        public SiteSettings SiteSettings { get => siteSettings; set => siteSettings = value; }
        public TestingSetup? TestingSetup { get; internal set; }
        public string TestClientSecret { get; private set; } = "TestClientSecret";

        public WebServerFixture()
        {
            SetupHost().Wait();
        }

        public void Dispose()
        {
            if (app!=null)
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
            ConfigureLogger();
            Configuration = builder.Configuration
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            this.app = builder.ConfigureWebAppServices();
            Services = app.Services;
            using (var migrationScope = app.Services.CreateScope())
            {
                var db = migrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Database.MigrateAsync();
            }
            await app.ConfigurePipeline();
            app.UseSession();
            await app.StartAsync();



            var logFactory = app.Services.GetRequiredService<ILoggerFactory>();
            Logger = logFactory.CreateLogger<WebServerFixture>();

            var server = app.Services.GetRequiredService<IServer>();

            var addressFeatures = server.Features.Get<IServerAddressesFeature>();

            foreach (var address in addressFeatures.Addresses)
            {
                Addresses.Add(address);
            }
            SiteSettings = app.Services.GetRequiredService<IOptions<SiteSettings>>().Value;

            using IServiceScope scope = app.Services.CreateScope();
            ApplicationDbContext dbContext =
                scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            //dbContext.Database.EnsureCreated();
            dbContext.Database.Migrate();

            TestingUserName = "Tester";
            TestingUserPassword = "test";
            TestClientId = "testClientId";
            TestingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.UserName == TestingUserName);
            EnsureUser(TestingUserName, TestingUserPassword);
           
        }

        public void EnsureUser(string testingUserName, string password)
        {
            if (TestingUser == null)
            {
                using IServiceScope scope = app.Services.CreateScope();

                var userManager  =
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                TestingUser = new ApplicationUser
                {
                    UserName = testingUserName,
                    Email = testingUserName + "@example.com",
                    EmailConfirmed = true
                };

                var result = userManager.CreateAsync(TestingUser,password).Result;

                Assert.True(result.Succeeded);

                ApplicationDbContext dbContext =
                scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                TestingUser = dbContext.Users.FirstOrDefault(u => u.UserName == testingUserName);
            }
        }
    }
}
