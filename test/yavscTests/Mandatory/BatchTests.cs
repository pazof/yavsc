using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Xunit;
using Yavsc;
using Yavsc.Models;
using Yavsc.Services;
using System.Runtime.Versioning;
using System.Diagnostics;
using Yavsc.Helpers;
using Xunit.Abstractions;
using Yavsc.Server.Models.IT.SourceCode;
using yavscTests.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.DotNet.Scaffolding.Shared.ProjectModel;
using Microsoft.AspNetCore.Identity;
using Yavsc.Settings;
using Microsoft.AspNetCore.Razor.Language;
using isnd.tests;

namespace yavscTests
{
    [Collection("Yavsc mandatory success story")]
    [Trait("regression", "oui")]
    public class BaseTestContext:  IClassFixture<WebServerFixture>, IDisposable
    {
        public readonly WebServerFixture _serverFixture;
        private readonly TestingSetup _testingOptions;
        private readonly ITestOutputHelper _output;

        public BaseTestContext(ITestOutputHelper output, WebServerFixture fixture)
        {
            this._serverFixture = fixture;
            _testingOptions = fixture.TestingSetup;
            this._output = output;
        }

        [Fact]
        public void GitClone()
        {
            Assert.True(_serverFixture.EnsureTestDb());
            Assert.NotNull (_serverFixture.DbContext.Project);
            var firstProject = _serverFixture.DbContext.Project.Include(p=>p.Repository).FirstOrDefault();
            Assert.NotNull (firstProject);
            var di = new DirectoryInfo(_serverFixture.SiteSettings.GitRepository);
            if (!di.Exists) di.Create();

            var clone = new GitClone(_serverFixture.SiteSettings.GitRepository);
            clone.Launch(firstProject);
            gitRepo = di.FullName;
        }
        string gitRepo=null;
        private IConfigurationRoot configurationRoot;

   


        [Fact]
        public void HaveConfigurationRoot()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile( "appsettings.json", false);
            builder.AddJsonFile( "appsettings.Development.json", true);
            configurationRoot = builder.Build();
        }

       

        internal static IConfigurationRoot CreateConfiguration(string prjDir)
        {
            var builder = new ConfigurationBuilder();

            builder.AddJsonFile(Path.Combine(prjDir, "appsettings.json"), false);
            builder.AddJsonFile(Path.Combine(prjDir, "appsettings.Development.json"), true);
            return builder.Build();
        }
        static IConfigurationRoot ConfigurationRoot;
        internal static void ConfigureServices
        (ServiceCollection serviceCollection, 
         string prjDir,
         out IConfigurationRoot configuration,
         out IServiceProvider provider)
        {
            ConfigurationRoot = configuration = CreateConfiguration(prjDir);

            serviceCollection.AddOptions();
            var siteSettingsconf = configuration.GetSection("Site");
            serviceCollection.Configure<SiteSettings>(siteSettingsconf);
            var smtpSettingsconf = configuration.GetSection("Smtp");
            serviceCollection.Configure<SmtpSettings>(smtpSettingsconf);
            var locOptions = configuration.GetSection("Localization");
            serviceCollection.Configure<LocalizationOptions>(locOptions);

            serviceCollection.AddSingleton(typeof(ILoggerFactory), typeof(LoggerFactory));
            serviceCollection.AddTransient(typeof(IEmailSender<ApplicationUser>), typeof(MailSender));
        
            serviceCollection.AddLogging();
            serviceCollection.AddMvcCore();
            serviceCollection.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });
            serviceCollection.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(o => ConfigurationRoot.GetConnectionString("DefaultConnection")));
            provider = serviceCollection.BuildServiceProvider();
        }


        public void Dispose()
        {
            if (gitRepo!=null)
            {
                Directory.Delete(Path.Combine(gitRepo,"yavsc"), true);
            }
            _serverFixture.DropTestDb();
        }
    }
}
