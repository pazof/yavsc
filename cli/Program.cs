using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Yavsc;
using Yavsc.Models;
using Yavsc.Server.Helpers;
using Yavsc.Services;
using Microsoft.Extensions.OptionsModel;

using System.Globalization;
using System.Reflection;
// using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Builder;
// using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Localization;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNet.Razor;
using Microsoft.Extensions.DependencyInjection.Abstractions;
using Microsoft.Extensions.PlatformAbstractions;
using cli.Services;

namespace cli
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder();

            var hostengnine = host.UseEnvironment("Develpment")
            .UseServer("cli")
            .UseStartup<Startup>()
             
            .UseServices(services => {
              Console.WriteLine($"> Using {services.Count} services:");
              foreach (var s in services) Console.WriteLine($"> * {s.ServiceType}");
            })
            .Build();

            var app = hostengnine.Start();
            app.Services.GetService<EMailer>();
            
        }
    }
}

namespace cli
{
    public class Startup
    {
        public string ConnectionString
        {
            get { return DbHelpers.ConnectionString; }
            private set { DbHelpers.ConnectionString = value; }
        }

        public static SiteSettings SiteSetup { get; private set; }
        public static SmtpSettings SmtpSettup { get; private set; }
        public static IConfiguration Configuration { get; set; }

        public static string HostingFullName { get; private set; }
        
        ILogger logger;
        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            var devtag = env.IsDevelopment()?"D":"";
            var prodtag = env.IsProduction()?"P":"";
            var stagetag = env.IsStaging()?"S":"";

            HostingFullName = $"{appEnv.RuntimeFramework.FullName} [{env.EnvironmentName}:{prodtag}{devtag}{stagetag}]";
            // Set up configuration sources.
        
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

        }
        public void ConfigureServices (IServiceCollection services)
        {
            services.AddOptions();
            var siteSettingsconf = Configuration.GetSection("Site");
            services.Configure<SiteSettings>(siteSettingsconf);
            var smtpSettingsconf = Configuration.GetSection("Smtp");
            services.Configure<SmtpSettings>(smtpSettingsconf);
            services.AddInstance(typeof(ILoggerFactory), new LoggerFactory());
            services.AddTransient(typeof(IEmailSender), typeof(MessageSender));
            services.AddTransient(typeof(RazorEngineHost), typeof(YaRazorEngineHost));


            services.AddEntityFramework().AddNpgsql().AddDbContext<ApplicationDbContext>();

            services.AddTransient((s) => new RazorTemplateEngine(s.GetService<RazorEngineHost>()));
            var serviceProvider = services.BuildServiceProvider();

            services.AddLogging();
            services.AddTransient<EMailer>();

            services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });
        }

        public void Configure (IApplicationBuilder app, IHostingEnvironment env,
        IOptions<SiteSettings> siteSettings, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            logger = loggerFactory.CreateLogger<Startup>();
            logger.LogInformation("Hello World from Configure ...");

        }

    }
}
