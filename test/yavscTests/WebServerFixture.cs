using System.Net;
using Microsoft.AspNetCore;
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

namespace isnd.tests
{

    [CollectionDefinition("Web server collection")]
    public class WebServerFixture : IDisposable
    {
        public IWebHost Host { get; private set;}
        public List<string> Addresses { get; private set; } = new List<string>();
        public Microsoft.Extensions.Logging.ILogger Logger { get; internal set; }

        private SiteSettings siteSettings;


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
            SetupHost();
        }

        public void Dispose()
        {
            Host.StopAsync().Wait();
            Host.Dispose();
        }

        public void SetupHost()
        {
            var builder = WebHost.CreateDefaultBuilder(new string[0]);

            //    .UseContentRoot("../../../../../src/isnd")
            builder.UseStartup(typeof(Startup))
            .ConfigureAppConfiguration((builderContext, config) =>
            {
                config.AddJsonFile("appsettings.json", true);
                config.AddJsonFile("appsettings.Development.json", false);
            });

            Host = builder.Build();

            var logFactory = Host.Services.GetRequiredService<ILoggerFactory>();
            Logger = logFactory.CreateLogger<WebServerFixture>();

            Host.Start(); //Starts listening on the configured addresses.
            var server = Host.Services.GetRequiredService<IServer>();

            var addressFeatures = server.Features.Get<IServerAddressesFeature>();

            foreach (var address in addressFeatures.Addresses)
            {
                Addresses.Add(address);
            }
            SiteSettings = Host.Services.GetRequiredService<IOptions<SiteSettings>>().Value;

            using IServiceScope scope = Host.Services.CreateScope();
            ApplicationDbContext dbContext =
                scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

          
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
                using IServiceScope scope = Host.Services.CreateScope();

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

        internal void DropTestDb()
        {
            throw new NotImplementedException();
        }

        internal bool EnsureTestDb()
        {
            throw new NotImplementedException();
        }

        internal int UpgradeDb()
        {
            throw new NotImplementedException();
        }
    }
}
