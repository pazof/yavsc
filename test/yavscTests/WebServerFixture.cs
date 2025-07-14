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

        public string TestingUserName { get; private set; }
        public string ProtectedTestingApiKey { get; internal set; }
        public ApplicationUser TestingUser { get; private set; }
        public bool DbCreated { get; internal set; }
        public SiteSettings SiteSettings { get => siteSettings; set => siteSettings = value; }
        public MailSender MailSender { get; internal set; }
        public TestingSetup? TestingSetup { get; internal set; }
        public ApplicationDbContext DbContext { get; internal set; }

        public WebServerFixture()
        {
            SetupHost().Wait();
        }

        public void Dispose()
        {
            if (app!=null)
                app.StopAsync().Wait();
        }

        public async Task SetupHost()
        {
            var builder = WebApplication.CreateBuilder();
       
            Configuration = builder.Configuration
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            this.app =  builder.ConfigureWebAppServices();
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
            TestingUser = dbContext.Users.FirstOrDefault(u => u.UserName == TestingUserName);
            EnsureUser(TestingUserName);
          
            ServicePointManager.ServerCertificateValidationCallback =
    (sender, cert, chain, sslPolicyErrors) => true;

        }

        public void EnsureUser(string testingUserName)
        {
            if (TestingUser == null)
            {
                using IServiceScope scope = app.Services.CreateScope();

                var userManager  =
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

          
                TestingUser = new ApplicationUser
                {
                    UserName = testingUserName
                };

                var result = userManager.CreateAsync(TestingUser).Result;

                Assert.True(result.Succeeded);

                ApplicationDbContext dbContext =
                scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                TestingUser = dbContext.Users.FirstOrDefault(u => u.UserName == testingUserName);
            }
        }


    }
}
