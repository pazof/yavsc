using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Razor;
using Microsoft.Extensions.PlatformAbstractions;
using cli.Services;
using Yavsc;
using Yavsc.Models;
using Yavsc.Services;
using Microsoft.Data.Entity;
using Microsoft.AspNet.Authentication;
using Microsoft.Extensions.WebEncoders;

namespace cli
{
    public class Startup
    {
        public string ConnectionString
        {
            get ; set;
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
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
            Configuration = builder.Build();
            ConnectionString = Configuration["Data:DefaultConnection:ConnectionString"];
            AppDomain.CurrentDomain.SetData("YAVSC_CONNECTION", ConnectionString);
        }

        public void ConfigureServices (IServiceCollection services)
        {
            services.AddOptions();
            var siteSettingsconf = Configuration.GetSection("Site");
            services.Configure<SiteSettings>(siteSettingsconf);
            var smtpSettingsconf = Configuration.GetSection("Smtp");
            services.Configure<SmtpSettings>(smtpSettingsconf);
            services.AddInstance(typeof(ILoggerFactory), new LoggerFactory());
            services.AddTransient(typeof(IEmailSender), typeof(MailSender));
            services.AddTransient(typeof(RazorEngineHost), typeof(YaRazorEngineHost));
            services.AddEntityFramework().AddNpgsql().AddDbContext<ApplicationDbContext>();
            services.AddTransient((s) => new RazorTemplateEngine(s.GetService<RazorEngineHost>()));
            services.AddLogging();
            services.AddTransient<EMailer>();
            services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });
           
            services.AddEntityFramework()
              .AddNpgsql() 
              .AddDbContext<ApplicationDbContext>(
                  db => db.UseNpgsql(ConnectionString)
              );
              services.Configure<SharedAuthenticationOptions>(options =>
            {
                options.SignInScheme = "Bearer";
            });

            services.AddTransient<Microsoft.Extensions.WebEncoders.UrlEncoder, UrlEncoder>();

            services.AddAuthentication();

        }

        public void Configure (IApplicationBuilder app, IHostingEnvironment env,
        IOptions<SiteSettings> siteSettings, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            logger = loggerFactory.CreateLogger<Startup>();
            logger.LogInformation(env.EnvironmentName);
            var authConf = Configuration.GetSection("Authentication").GetSection("Yavsc");
            var clientId = authConf.GetSection("ClientId").Value;
            var clientSecret = authConf.GetSection("ClientSecret").Value;

        }

    }
}
