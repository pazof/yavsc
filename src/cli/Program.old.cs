﻿
using System;
using System.Runtime.Versioning;
using cli.Commands;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            HostingEnvironment = new HostingEnvironment
            {
                EnvironmentName = appEnv.Configuration
            };

            var startup = new Startup(HostingEnvironment, appEnv);
            startup.ConfigureServices(services);
            services.AddInstance<IHostingEnvironment>(HostingEnvironment);
            var serviceProvider = services.BuildServiceProvider();

            var app = new ApplicationBuilder(serviceProvider)
            {
                ApplicationServices = serviceProvider
            };

            var siteSettings = serviceProvider.GetRequiredService<IOptions<SiteSettings>>();
            var cxSettings = serviceProvider.GetRequiredService<IOptions<ConnectionSettings>>();
            var userCxSettings = serviceProvider.GetRequiredService<IOptions<UserConnectionSettings>>();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            startup.Configure(cxSettings, userCxSettings, loggerFactory);
            
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
            CommandLineApplication cliapp = new CommandLineApplication(false)
            {
                Name = "cli",
                FullName = "Yavsc command line interface",
                Description = "Dnx console app for yavsc server side",
                ShortVersionGetter = () => "v1.0",
                LongVersionGetter = () => "version 1.0 (stable)"
            };

            // calling a Startup sequence
            var appBuilder = ConfigureApplication();
            var loggerFactory = appBuilder.ApplicationServices.GetRequiredService<ILoggerFactory>();
            var cxSettings = appBuilder.ApplicationServices.GetRequiredService<IOptions<ConnectionSettings>>();
            var usercxSettings = appBuilder.ApplicationServices.GetRequiredService<IOptions<UserConnectionSettings>>();
            var emailer = appBuilder.ApplicationServices.GetService<Emailer>();
            CommandOption rootCommandHelpOption = cliapp.HelpOption("-? | -h | --help");

            new SendMailCommandProvider().Integrate(cliapp);
            new GenerateJsonSchema().Integrate(cliapp);
            new AuthCommander(loggerFactory).Integrate(cliapp);
            new GenerationCommander().Integrate(cliapp);
            new Streamer(loggerFactory, cxSettings, usercxSettings ).Integrate(cliapp);
            new UserListCleanUp().Integrate(cliapp);

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
