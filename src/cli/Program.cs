
using System;
using System.Runtime.Versioning;
using cli.Commands;
using Microsoft.AspNet.Builder.Internal;
using Microsoft.AspNet.Hosting;
using Microsoft.Dnx.Runtime;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using Yavsc;

namespace cli
{
    public class CliAppEnv : IApplicationEnvironment
    {
        public CliAppEnv(IApplicationEnvironment defaultEnv)
        {
            var envAppVar = Environment.GetEnvironmentVariable("ASPNET_ENV");
            Configuration = envAppVar ?? defaultEnv.Configuration;
            ApplicationName = defaultEnv.ApplicationName;
            ApplicationVersion = defaultEnv.ApplicationVersion;
            ApplicationBasePath = defaultEnv.ApplicationBasePath;
            RuntimeFramework = defaultEnv.RuntimeFramework;
        }
        public string ApplicationName { get; private set; }

        public string ApplicationVersion { get; private set; }

        public string ApplicationBasePath { get; private set; }

        public string Configuration { get; private set; }

        public FrameworkName RuntimeFramework { get; private set; }

        public object GetData(string name)
        {
            throw new NotImplementedException();
        }

        public void SetData(string name, object value)
        {
            throw new NotImplementedException();
        }
    }
    public partial class Program
    {
        private static IApplicationEnvironment appEnv;
        public static IHostingEnvironment HostingEnvironment  { get; private set; }

        public Program()
        {
            appEnv = new CliAppEnv(PlatformServices.Default.Application);
        }

        /// <summary>
        /// Initializes the application by 
        /// </summary>
        private static ApplicationBuilder ConfigureApplication()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnHandledException;

            var services = new ServiceCollection();
            // create a service provider with the HostEnvironment.
            HostingEnvironment = new HostingEnvironment();
            HostingEnvironment.EnvironmentName = appEnv.Configuration;

            var startup = new Startup(HostingEnvironment, appEnv);
            startup.ConfigureServices(services);
            services.AddInstance<IHostingEnvironment>(HostingEnvironment);
            var serviceProvider = services.BuildServiceProvider();

            var app = new ApplicationBuilder(serviceProvider);
            app.ApplicationServices = serviceProvider;
            
            var siteSettings = serviceProvider.GetRequiredService<IOptions<SiteSettings>>();
            var cxSettings = serviceProvider.GetRequiredService<IOptions<ConnectionSettings>>();
            var userCxSettings = serviceProvider.GetRequiredService<IOptions<UserConnectionSettings>>();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            startup.Configure(app, HostingEnvironment, siteSettings, cxSettings, userCxSettings, loggerFactory);
            
            return app;
        }

        private static void OnUnHandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Unhandled Exception occured:");
            Console.WriteLine(JsonConvert.SerializeObject(e.ExceptionObject));
        }

        [STAThread]
        public static int Main(string[] args)
        {
            CommandLineApplication cliapp = new CommandLineApplication(false);
            cliapp.Name = "cli";
            cliapp.FullName = "Yavsc command line interface";
            cliapp.Description = "Dnx console app for yavsc server side";
            cliapp.ShortVersionGetter = () => "v1.0";
            cliapp.LongVersionGetter = () => "version 1.0 (stable)";

            // calling a Startup sequence
            var appBuilder = ConfigureApplication();
            var loggerFactory = appBuilder.ApplicationServices.GetRequiredService<ILoggerFactory>();
            var cxSettings = appBuilder.ApplicationServices.GetRequiredService<IOptions<ConnectionSettings>>();
            var usercxSettings = appBuilder.ApplicationServices.GetRequiredService<IOptions<UserConnectionSettings>>();

            CommandOption rootCommandHelpOption = cliapp.HelpOption("-? | -h | --help");

            (new SendMailCommandProvider()).Integrate(cliapp);
            (new GenerateJsonSchema()).Integrate(cliapp);
            (new AuthCommander(loggerFactory)).Integrate(cliapp);
            (new CiBuildCommand()).Integrate(cliapp);
            (new GenerationCommander()).Integrate(cliapp);
            (new Streamer(loggerFactory, cxSettings, usercxSettings )).Integrate(cliapp);

            if (args.Length == 0)
            {
                cliapp.ShowHint();
                return -1;
            }
            var result  = cliapp.Execute(args);

            if (cliapp.RemainingArguments.Count > 0)
            {
                cliapp.ShowHint();
                return -1;
            }
            return result;
        }
    }
}
