using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Builder.Internal;
using Microsoft.AspNet.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Xunit;
using Yavsc;
using Yavsc.Models;
using Yavsc.Services;
using System.Runtime.Versioning;
using Microsoft.AspNet.Mvc.Razor;
using System.Diagnostics;
using Microsoft.Dnx.Compilation.CSharp;
using Microsoft.Extensions.OptionsModel;
using Yavsc.Helpers;
using Microsoft.Data.Entity;
using Xunit.Abstractions;
using System.IO;
using System.Linq;
using Yavsc.Server.Models.IT.SourceCode;

namespace test
{


    [Collection("Yavsc mandatory success story")]
    [Trait("regres", "yes")]
    public class BatchTests: BaseTestContext, IClassFixture<ServerSideFixture>, IDisposable
    {
        ServerSideFixture _fixture;

        public  BatchTests(ITestOutputHelper output, ServerSideFixture fixture) : base (output, fixture)
        {
          _fixture = fixture;
        }

        [Fact]
        public void GitClone()
        {
            Assert.True(_serverFixture.EnsureTestDb());
            Assert.NotNull (_fixture.DbContext.Project);
            var firstProject = _fixture.DbContext.Project.Include(p=>p.Repository).FirstOrDefault();
            Assert.NotNull (firstProject);
            var di = new DirectoryInfo(_serverFixture.SiteSetup.GitRepository);
            if (!di.Exists) di.Create();

            var clone = new GitClone(_serverFixture.SiteSetup.GitRepository);
            clone.Launch(firstProject);
            gitRepo = di.FullName;
        }
        string gitRepo=null;

        [Fact]
        void AnsiToHtml()
        {
            var procStart = new ProcessStartInfo("ls", "-l --color=always")
            {
                UseShellExecute = false,
                RedirectStandardInput = false,
                RedirectStandardOutput = true
            };
            var proc = Process.Start(procStart);
            var encoded =  AnsiToHtmlEncoder.GetStream(proc.StandardOutput);
            using (var reader = new StreamReader(encoded))
            {
                var txt = reader.ReadToEnd();
                _output.WriteLine(txt);
            }
        }

        [Fact]
        public void MvcRazorHostAndParser()
        {
            string cache = System.IO.Directory.GetCurrentDirectory();
            MvcRazorHost host = new MvcRazorHost(cache);
            var parser = host.CreateMarkupParser();
        }

        [Fact]
        void HaveHost()
        {
            
            
        }

        [Fact]
        public void EnsureConfigurationRoot()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile( "appsettings.json", false);
            builder.AddJsonFile( "appsettings.Development.json", true);
            configurationRoot = builder.Build();
        }

        internal static BeforeCompileContext CreateYavscCompilationContext()
        {
            var projectContext = new ProjectContext
            {
                Name = "Yavsc",
                ProjectDirectory = "../Yavsc",
                ProjectFilePath = "../Yavsc/project.json",
                TargetFramework = new FrameworkName("DNX", new Version(4, 5, 1)),
                Configuration = Environment.GetEnvironmentVariable("ASPNET_ENV")
            };

            return new BeforeCompileContext(
                null, projectContext, () => null, () => null, () => null);
        }

        internal static IConfigurationRoot CreateConfiguration(string prjDir)
        {
            var builder = new ConfigurationBuilder();

            builder.AddJsonFile(Path.Combine(prjDir, "appsettings.json"), false);
            builder.AddJsonFile(Path.Combine(prjDir, "appsettings.Development.json"), true);
            return builder.Build();
        }

        internal static void ConfigureServices
        (ServiceCollection serviceCollection, 
         string prjDir,
         out IConfigurationRoot configuration,
         out IServiceProvider provider)
        {
            configuration = CreateConfiguration(prjDir);

            serviceCollection.AddOptions();
            var siteSettingsconf = configuration.GetSection("Site");
            serviceCollection.Configure<SiteSettings>(siteSettingsconf);
            var smtpSettingsconf = configuration.GetSection("Smtp");
            serviceCollection.Configure<SmtpSettings>(smtpSettingsconf);
            var locOptions = configuration.GetSection("Localization");
            serviceCollection.Configure<LocalizationOptions>(locOptions);

            serviceCollection.AddSingleton(typeof(ILoggerFactory), typeof(LoggerFactory));
            serviceCollection.AddTransient(typeof(IEmailSender), typeof(MailSender));
            serviceCollection.AddTransient(typeof(RazorEngineHost));
            serviceCollection.AddTransient((s) => new RazorTemplateEngine(s.GetService<RazorEngineHost>()));
            serviceCollection.AddLogging();
            serviceCollection.AddMvcCore();
            serviceCollection.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });
            AppDomain.CurrentDomain.SetData("YAVSC_DB_CONNECTION", Startup.Testing.ConnectionStrings.Default);
            serviceCollection.AddEntityFramework()
              .AddNpgsql()
              .AddDbContext<ApplicationDbContext>(
                                 db => db.UseNpgsql(Startup.Testing.ConnectionStrings.Default)
              );
            provider = serviceCollection.BuildServiceProvider();
        }


        [Fact]
        public void ARequestAppDelegate()
        {
            var services = new ServiceCollection();
            services.AddTransient<IRuntimeEnvironment>(
                svs => PlatformServices.Default.Runtime
            );

            beforeCompileContext = CreateYavscCompilationContext();
            var prjDir = this.beforeCompileContext.ProjectContext.ProjectDirectory;
            ConfigureServices(services, prjDir, out configurationRoot, out serviceProvider);

            IApplicationBuilder app = new ApplicationBuilder(serviceProvider);
            var rtd = app.Build();
            
        }


        [Fact]
        public void InitApplicationBuilder()
        {
            var services = new ServiceCollection();

            services.AddTransient<IRuntimeEnvironment>(
                svs => PlatformServices.Default.Runtime
            );
            beforeCompileContext = CreateYavscCompilationContext();
            var prjDir = beforeCompileContext.ProjectContext.ProjectDirectory;
            ConfigureServices(services, prjDir, out configuration, out provider);

            IApplicationBuilder app = new ApplicationBuilder(provider);
            app.UseMvc();
            var rtd = app.Build();
            IOptions<LocalizationOptions> localOptions = ActivatorUtilities.GetServiceOrCreateInstance<IOptions<LocalizationOptions>>(provider); ;

        }

        public void Dispose()
        {
            if (gitRepo!=null)
            {
                Directory.Delete(Path.Combine(gitRepo,"yavsc"), true);
            }
            _fixture.DropTestDb();
        }
    }
}
