using System;
using System.Runtime.Versioning;
using Google.Apis.Util.Store;

using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Yavsc;
using Yavsc.Models;

using Yavsc.Services;
using cli_2;
using Yavsc.Server.Helpers;
using Microsoft.AspNet.Identity;
using Microsoft.Data.Entity;
using cli.Modules;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System.Security.Policy;
using System.Security;
using System.Security.Permissions;
using System.Reflection;
using System.Collections;
using RazorEngine;

public class Program
{
    private static IServiceProvider serviceProvider;
    public string ConnectionString
    {
        get { return DbHelpers.ConnectionString; }
        private set { DbHelpers.ConnectionString = value; }
    }

    public static SiteSettings SiteSetup { get; private set; }
    public static SmtpSettings SmtpSettup { get; private set; }
    public IConfigurationRoot Configuration { get; set; }

    public static IdentityOptions AppIdentityOptions { get; private set; }
    public Program()
    {
        ConfigureServices(new ServiceCollection());
    }

    private void ConfigureServices(IServiceCollection services, string environmentName = "Development")
    {



        var confbuilder = new ConfigurationBuilder()
                   .AddJsonFile("appsettings.json")
                   .AddJsonFile($"appsettings.{environmentName}.json", optional: true);

        Configuration = confbuilder.Build();


        var siteSettingsconf = Configuration.GetSection("Site");
        services.Configure<SiteSettings>(siteSettingsconf);
        var smtpSettingsconf = Configuration.GetSection("Smtp");
        services.Configure<SmtpSettings>(smtpSettingsconf);

        services.AddTransient(typeof(IModule), typeof(MonthlyEMailGenerator));
        // services.AddTransient( typeof(Microsoft.AspNet.Mvc.Razor.Compilation.ICompilationService), typeof (Microsoft.AspNet.Mvc.Razor.Compilation.RazorCompilationService) );
        services.AddLogging();

        // services.AddTransient(typeof(Microsoft.AspNet.Mvc.Razor.Compilation.IRazorCompilationService), typeof(Microsoft.AspNet.Mvc.Razor.Compilation.RazorCompilationService));


        services.AddEntityFramework()
              .AddNpgsql()
              .AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(Configuration["Data:DefaultConnection:ConnectionString"]))
              ;
        services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });
        services.AddOptions();

        services.AddIdentity<ApplicationUser, IdentityRole>(
                option =>
                {
                    option.User.AllowedUserNameCharacters += " ";
                    option.User.RequireUniqueEmail = true;
                    // option.Cookies.ApplicationCookieAuthenticationScheme = Constants.ApplicationAuthenticationSheme;
                    option.Cookies.ApplicationCookie.LoginPath = "/signin";
                    // option.Cookies.ApplicationCookie.AuthenticationScheme = Constants.ApplicationAuthenticationSheme;
                    /*
                     option.Cookies.ApplicationCookie.DataProtectionProvider = protector;
                     option.Cookies.ApplicationCookie.LoginPath = new PathString(Constants.LoginPath.Substring(1));
                     option.Cookies.ApplicationCookie.AccessDeniedPath = new PathString(Constants.AccessDeniedPath.Substring(1));
                     option.Cookies.ApplicationCookie.AutomaticAuthenticate = true;
                     option.Cookies.ApplicationCookie.AuthenticationScheme = Constants.ApplicationAuthenticationSheme;
                     option.Cookies.ApplicationCookieAuthenticationScheme = Constants.ApplicationAuthenticationSheme;
                     option.Cookies.TwoFactorRememberMeCookie.ExpireTimeSpan = TimeSpan.FromDays(30);
                     option.Cookies.TwoFactorRememberMeCookie.DataProtectionProvider = protector;
                     option.Cookies.ExternalCookieAuthenticationScheme = Constants.ExternalAuthenticationSheme;
                     option.Cookies.ExternalCookie.AutomaticAuthenticate = true;
                     option.Cookies.ExternalCookie.AuthenticationScheme = Constants.ExternalAuthenticationSheme;
                     option.Cookies.ExternalCookie.DataProtectionProvider = protector;
                     */
                }
            ).AddEntityFrameworkStores<ApplicationDbContext>()
                 .AddTokenProvider<EmailTokenProvider<ApplicationUser>>(Yavsc.Constants.DefaultFactor)
            // .AddTokenProvider<UserTokenProvider>(Constants.DefaultFactor)
            //  .AddTokenProvider<UserTokenProvider>(Constants.SMSFactor)
            //  .AddTokenProvider<UserTokenProvider>(Constants.EMailFactor)
            //  .AddTokenProvider<UserTokenProvider>(Constants.AppFactor)
            // .AddDefaultTokenProviders()
            ;






        // Add application services.
        services.AddTransient<IEmailSender, MessageSender>();
        services.AddTransient<IGoogleCloudMessageSender, MessageSender>();
        services.AddTransient<IBillingService, BillingService>();
        services.AddTransient<IDataStore, FileDataStore>((sp) => new FileDataStore("googledatastore", false));
        services.AddTransient<ICalendarManager, CalendarManager>();

        // TODO for SMS: services.AddTransient<ISmsSender, AuthMessageSender>();

        services.AddLocalization(options =>
        {
            options.ResourcesPath = "Resources";
        });

        services.AddIdentity<ApplicationUser, IdentityRole>();

        // Razor

        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        // FIXME null ref  var appName = AppDomain.CurrentDomain.ApplicationIdentity.FullName;


        var config = new TemplateServiceConfiguration();
        // TODO .. configure your instance
        
        // config.DisableTempFileLocking = true; // loads the files in-memory (gives the templates full-trust permissions)
        // config.CachingProvider = new DefaultCachingProvider(t => { }); //disables the warnings
                                                                       // Use the config
        var razorService = RazorEngineService.Create(config);
        Engine.Razor = razorService;
        services.AddInstance(typeof(IRazorEngineService), razorService);

        // Razor.SetTemplateService(new TemplateService(config)); // legacy API
        serviceProvider = services.BuildServiceProvider();

        //configure console logging
        // needs a logger factory ...
        var loggerFactory = serviceProvider
            .GetService<ILoggerFactory>()
            .AddConsole(LogLevel.Verbose);
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogInformation("Configuring application  ...");

        var projectRoot = "/home/paul/workspace/yavsc/Yavsc";


        var targetFramework = new FrameworkName("dnx", new Version(4, 5, 1));
        //  
        // 
        ApplicationEnvironment appEnv = new ApplicationEnvironment(targetFramework, projectRoot);





        ConnectionString = Configuration["Data:DefaultConnection:ConnectionString"];


        logger.LogInformation($"cx: {ConnectionString} ({environmentName})");

        var sitesetupjson = Configuration.GetSection("Site");
        services.Configure<SiteSettings>(sitesetupjson);

        var smtpsetupjson = Configuration.GetSection("Smtp");
        services.Configure<SmtpSettings>(smtpsetupjson);

        IOptions<SiteSettings> siteSettings = serviceProvider.GetService(typeof(IOptions<SiteSettings>)) as IOptions<SiteSettings>;
        IOptions<SmtpSettings> smtpSettings = serviceProvider.GetService(typeof(IOptions<SmtpSettings>)) as IOptions<SmtpSettings>;


        SiteSetup = siteSettings.Value;
        SmtpSettup = smtpSettings.Value;

        logger.LogInformation($"done with {SiteSetup.Title} config.");
    }


    public static void Main(string[] args)
    {

        ILoggerFactory loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogInformation("Hello.");
        var mods = serviceProvider.GetServices<IModule>();
        foreach (var mod in mods) {
            logger.LogInformation($"running {mod.GetType().Name}");
            mod.Run(args);
        }
    }
}

