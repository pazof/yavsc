using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Razor;
using Microsoft.Extensions.PlatformAbstractions;
using Yavsc;
using Yavsc.Models;
using Yavsc.Services;
using Microsoft.Data.Entity;
using Microsoft.Extensions.WebEncoders;
using test.Settings;
namespace test
{
    public class Startup
    {

        public static IConfiguration Configuration { get; set; }

        public static string HostingFullName { get; private set; }
        public static Testing Testing { get; private set; }

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
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }
            Configuration = builder.Build();
        }

        public void ConfigureServices (IServiceCollection services)
        {
            services.AddOptions();
            var siteSettingsconf = Configuration.GetSection("Site");
            services.Configure<SiteSettings>(siteSettingsconf);
            var smtpSettingsconf = Configuration.GetSection("Smtp");
            services.Configure<SmtpSettings>(smtpSettingsconf);
            var dbSettingsconf = Configuration.GetSection("ConnectionStrings");
            services.Configure<DbConnectionSettings>(dbSettingsconf);
            var testingconf = Configuration.GetSection("Testing");
            services.Configure<Testing>(testingconf);
            
            services.AddInstance(typeof(ILoggerFactory), new LoggerFactory());
            services.AddTransient(typeof(IEmailSender), typeof(MailSender));
            services.AddEntityFramework().AddNpgsql().AddDbContext<ApplicationDbContext>();
            services.AddTransient((s) => new RazorTemplateEngine(s.GetService<RazorEngineHost>()));
            services.AddLogging();
            services.AddTransient<MailSender>();
            services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });
           
            services.AddEntityFramework()
              .AddNpgsql() 
              .AddDbContext<ApplicationDbContext>(
                  db => db.UseNpgsql(Testing.ConnectionStrings.Default)
              );

            services.AddTransient<Microsoft.Extensions.WebEncoders.UrlEncoder, UrlEncoder>();

        }

        public void Configure (IApplicationBuilder app, IHostingEnvironment env,
        IOptions<Testing> testingSettings,
         ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            logger = loggerFactory.CreateLogger<Startup>();
            logger.LogInformation(env.EnvironmentName);
            Testing = testingSettings.Value;
            if (Testing.ConnectionStrings==null)
                logger.LogInformation($" Testing.ConnectionStrings is null : ");
            else {
                AppDomain.CurrentDomain.SetData("YAVSC_DB_CONNECTION", Testing.ConnectionStrings.Default);
            }
        
            var authConf = Configuration.GetSection("Authentication").GetSection("Yavsc");
            var clientId = authConf.GetSection("ClientId").Value;
            var clientSecret = authConf.GetSection("ClientSecret").Value;

        }

    }
}
