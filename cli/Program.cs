



using System;
using System.Runtime.Versioning;
using Google.Apis.Util.Store;
using Microsoft.AspNet.Builder.Internal;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Hosting.Builder;
using Microsoft.AspNet.Hosting.Internal;
using Microsoft.AspNet.Server;

using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.PlatformAbstractions;
using Yavsc;
using Yavsc.Models;

using Yavsc.Services;
using cli_2;
using Yavsc.Server.Helpers;
using Microsoft.AspNet.Identity;
using Microsoft.Data.Entity;
using Newtonsoft.Json;

public class Program
{
    private IServiceProvider serviceProvider;
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



        IHostingEnvironment hosting = new HostingEnvironment { EnvironmentName = environmentName };
        if (hosting.IsDevelopment())
        {
            // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
            // builder.AddUserSecrets();
        }

        var confbuilder = new ConfigurationBuilder()
                   .AddJsonFile("appsettings.json")
                   .AddJsonFile($"appsettings.{hosting.EnvironmentName}.json", optional: true);
                
        confbuilder.AddEnvironmentVariables();
        Configuration = confbuilder.Build();


            var siteSettingsconf = Configuration.GetSection("Site");
            services.Configure<SiteSettings>(siteSettingsconf);
            var smtpSettingsconf = Configuration.GetSection("Smtp");
            services.Configure<SmtpSettings>(smtpSettingsconf);


        services.Add(new ServiceDescriptor(typeof(IHostingEnvironment), hosting));

        services.AddLogging();


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
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        // FIXME null ref  var appName = AppDomain.CurrentDomain.ApplicationIdentity.FullName;

        // var rtdcontext = new WindowsRuntimeDesignerContext (new string [] { "../Yavsc" }, "Yavsc");


        serviceProvider = services.BuildServiceProvider();

        //configure console logging
        // needs a logger factory ...
        var loggerFactory = serviceProvider
            .GetService<ILoggerFactory>()
            .AddConsole(LogLevel.Verbose);
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogInformation("Configuring application  ...");

        var projectRoot = "/home/paul/workspace/yavsc/Yavsc";

        hosting.Initialize(projectRoot, null);

        var targetFramework = new FrameworkName("dnx", new Version(4, 5, 1));
        //  
        // 
        ApplicationEnvironment appEnv = new ApplicationEnvironment(targetFramework, projectRoot);




        ApplicationBuilderFactory applicationBuilderFactory
         = new ApplicationBuilderFactory(serviceProvider);

        var builder = applicationBuilderFactory.CreateBuilder(null);
        
        ConnectionString = Configuration["Data:DefaultConnection:ConnectionString"];


        logger.LogInformation($"cx: {ConnectionString} ({hosting.EnvironmentName})");

        var sitesetupjson = Configuration.GetSection("Site");
        services.Configure<SiteSettings>(sitesetupjson);

        var smtpsetupjson = Configuration.GetSection("Smtp");
        services.Configure<SmtpSettings>(smtpsetupjson);

        builder.ApplicationServices = serviceProvider;

 logger.LogInformation(Configuration["Site:Title"]);
//logger.LogInformation(Configuration["Smtp"]);
        IOptions<SiteSettings> siteSettings = serviceProvider.GetService(typeof(IOptions<SiteSettings>)) as IOptions<SiteSettings>;
         IOptions<SmtpSettings> smtpSettings = serviceProvider.GetService(typeof(IOptions<SmtpSettings>)) as IOptions<SmtpSettings>;


        SiteSetup = siteSettings.Value;
        SmtpSettup = smtpSettings.Value;

        logger.LogInformation($"done with {SiteSetup.Title}");



    }


    public static void Main(string[] args)
    {

    }
}

