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

namespace test
{
    public class BaseTestContext {
        protected IApplicationEnvironment applicationEnvironment = null;
        protected IServiceProvider serviceProvider = null;
        protected IConfigurationRoot configurationRoot;
        protected BeforeCompileContext beforeCompileContext;
        protected string testprojectAssetPath = "/home/paul/workspace/yavsc/Yavsc";

        protected IServiceProvider provider;
        protected IConfigurationRoot configuration;
    }

    [Collection("Yavsc mandatory success story")]
    public class YavscMandatory: BaseTestContext
    {
        private readonly ITestOutputHelper output;
        public YavscMandatory(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        void TestStartNodeJsansitohtml()
        {
            var procStart = new ProcessStartInfo("env", "/usr/bin/nodejs node_modules/ansi-to-html/bin/ansi-to-html");
            procStart.UseShellExecute = false;
            procStart.RedirectStandardInput = true;
            procStart.RedirectStandardOutput = true;
            procStart.RedirectStandardError = true;
            var proc = Process.Start(procStart);

            proc.StandardInput.WriteLine("\x001b[30mblack\x1b[37mwhite");
            proc.StandardInput.Close();
            while (!proc.StandardOutput.EndOfStream)
            {
                output.WriteLine(proc.StandardOutput.ReadLine());
            }
        }

        // actually uses node's ansi-to-html
        [Fact]
        void AnsiToHtml()
        {
            var procStart = new ProcessStartInfo("ls", "-l --color=always");
            procStart.UseShellExecute = false;
            procStart.RedirectStandardInput = false;
            procStart.RedirectStandardOutput = true;
            var proc = Process.Start(procStart);
            var encoded =  AnsiToHtmlEncoder.GetStream(proc.StandardOutput);
            using (var reader = new StreamReader(encoded))
            {
                var txt = reader.ReadToEnd();
                output.WriteLine(txt);
            }
        }

        [Fact]
        public void ApplicationDbContextExists()
        {
            var dbc = new ApplicationDbContext();
            Assert.NotNull(dbc.GCMDevices);
        }

        [Fact]
        public void MvcRazorHostAndParser()
        {
            string cache = System.IO.Directory.GetCurrentDirectory();
            MvcRazorHost host = new MvcRazorHost(cache);
            var parser = host.CreateMarkupParser();
        }

        [Fact]
        void HaveDependecyInjection()
        {
            var services = new ServiceCollection();
        }

        [Fact]
        void HaveHost()
        {
            beforeCompileContext = CreateYavscCompilationContext();
        }

        [Fact]
        public void AConfigurationRoot()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile(Path.Combine(testprojectAssetPath, "appsettings.json"), false);
            builder.AddJsonFile(Path.Combine(testprojectAssetPath, "appsettings.Development.json"), true);
            configurationRoot = builder.Build();
        }

        internal static BeforeCompileContext CreateYavscCompilationContext()
        {
            var projectContext = new ProjectContext();
            projectContext.Name = "Yavsc";
            projectContext.ProjectDirectory = "/home/paul/workspace/yavsc/Yavsc";
            projectContext.ProjectFilePath = "/home/paul/workspace/yavsc/Yavsc/project.json";
            projectContext.TargetFramework = new FrameworkName("DNX", new Version(4, 5, 1));
            projectContext.Configuration = "Development";

            return new BeforeCompileContext(
                null, projectContext, () => null, () => null, () => null);
        }

        internal static IConfigurationRoot CreateConfiguration(string prjDir)
        {
            var builder = new ConfigurationBuilder();

            builder.AddJsonFile(Path.Combine(prjDir, "appsettings.json"), true);
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
            var connectionString = configuration["Data:DefaultConnection:ConnectionString"];
            AppDomain.CurrentDomain.SetData("YAVSC_DB_CONNECTION", connectionString);
            serviceCollection.AddEntityFramework()
              .AddNpgsql()
              .AddDbContext<ApplicationDbContext>(
                                 db => db.UseNpgsql(connectionString)
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

            HaveHost();
            var prjDir = this.beforeCompileContext.ProjectContext.ProjectDirectory;
            ConfigureServices(services, prjDir, out configurationRoot, out serviceProvider);

            IApplicationBuilder app = new ApplicationBuilder(serviceProvider);
            var rtd = app.Build();
            
        }


        [Fact]
        public void MessageSenderFromLib()
        {
            ARequestAppDelegate();
            ILoggerFactory factory = ActivatorUtilities.GetServiceOrCreateInstance<ILoggerFactory>(serviceProvider);
            var dbc = new ApplicationDbContext();
          
            IOptions<SiteSettings> siteOptions = 
                ActivatorUtilities.GetServiceOrCreateInstance<IOptions<SiteSettings>>(serviceProvider);
           ;
            IOptions<SmtpSettings> smtpOptions = ActivatorUtilities.GetServiceOrCreateInstance<IOptions<SmtpSettings>>(serviceProvider);
            ;
            IOptions<GoogleAuthSettings> googleOptions = ActivatorUtilities.GetServiceOrCreateInstance<IOptions<GoogleAuthSettings>>(serviceProvider);
            ;
               IEmailSender eSender = new MailSender
                (factory, siteOptions, smtpOptions, googleOptions);
        }

        [Fact]
        public void InitApplicationBuilder()
        {

            var services = new ServiceCollection();

            services.AddTransient<IRuntimeEnvironment>(
                svs => PlatformServices.Default.Runtime
            );
            beforeCompileContext = YavscMandatory.CreateYavscCompilationContext();
            var prjDir = beforeCompileContext.ProjectContext.ProjectDirectory;
            YavscMandatory.ConfigureServices(services, prjDir, out configuration, out provider);

            IApplicationBuilder app = new ApplicationBuilder(provider);
            app.UseMvc();
            var rtd = app.Build();
            IOptions<LocalizationOptions> localOptions = ActivatorUtilities.GetServiceOrCreateInstance<IOptions<LocalizationOptions>>(provider); ;

        }
    }
}
