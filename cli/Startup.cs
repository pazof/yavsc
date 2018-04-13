using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Data.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.OptionsModel;
using Newtonsoft.Json;
using Yavsc;
using Yavsc.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Threading;
using Yavsc.Server.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNet.Http;

public class Startup
{
    private RequestDelegate app;

    public Startup(string hostingFullName, IConfigurationRoot configuration, string connectionString)
    {
        this.HostingFullName = hostingFullName;
        this.Configuration = configuration;
        this.ConnectionString = connectionString;

    }
    public string HostingFullName { get; private set; }

    public static SiteSettings SiteSetup { get; private set; }
    public static SmtpSettings SmtpSettup { get; private set; }
    public IConfigurationRoot Configuration { get; set; }
    public string ConnectionString { 
        get { return DbHelpers.ConnectionString; }
        private set {  DbHelpers.ConnectionString = value; } }
    public static IdentityOptions AppIdentityOptions { get; private set; }

    public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
    {
        var devtag = env.IsDevelopment() ? "D" : "";
        var prodtag = env.IsProduction() ? "P" : "";
        var stagetag = env.IsStaging() ? "S" : "";

        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

        builder.AddEnvironmentVariables();

        HostingFullName = $" [{env.EnvironmentName}:{prodtag}{devtag}{stagetag}]";
        // Set up configuration sources.


        if (env.IsDevelopment())
        {
            // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
            // builder.AddUserSecrets();
        }
        Configuration = builder.Build();
        ConnectionString = Configuration["Data:DefaultConnection:ConnectionString"];
    }

    // Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();
        var siteSettings = Configuration.GetSection("Site");
        services.Configure<SiteSettings>(siteSettings);
        var smtpSettings = Configuration.GetSection("Smtp");
        services.Configure<SmtpSettings>(smtpSettings);

        services.AddEntityFramework()
              .AddNpgsql()
              .AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(Configuration["Data:DefaultConnection:ConnectionString"]))
              ;
        services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });
        services.AddOptions();

        services.Configure<SiteSettings>((o) => JsonConvert.PopulateObject(Configuration["Site"], o));
        services.Configure<SmtpSettings>((o) => JsonConvert.PopulateObject(Configuration["Smtp"], o));
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

    }


    // Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder builder, ILoggerFactory loggerFactory,  IOptions<SiteSettings> siteSettingsOptions, IOptions<SmtpSettings> smtpSettingsOptions)
    {
        

        var logger = loggerFactory.CreateLogger<Startup>();
        logger.LogInformation("Configuring application startup ...");

        SiteSetup = siteSettingsOptions.Value;
        SmtpSettup = smtpSettingsOptions.Value;
        DbHelpers.ConnectionString = Configuration["Data:DefaultConnection:ConnectionString"];
        
logger.LogInformation("done");
    }

    public void Main(string[] args)
    {
        var dbContext = new ApplicationDbContext();
         foreach (var user in dbContext.Users) {
            Console.WriteLine($"UserName/{user.UserName} FullName/{user.FullName} Email/{user.Email} ");
        }
    }
}



internal class TerminalHost
{
    private Action<char> _inputDelegate;

    public TerminalHost()
    {
        // Initializes the first delegate to be invoked in the chain.
        _inputDelegate = Console.Write;
    }

    internal void Start()
    {
        CancellationTokenSource tokenSource = new CancellationTokenSource();

        while (!tokenSource.IsCancellationRequested) {
            ConsoleKeyInfo keyInfo = Console.ReadKey();

            _inputDelegate(keyInfo.KeyChar);
        }
    }

    /// <summary>
    /// Adds the middleware to the invocation chain. 
    /// </summary>
    /// <param name="middleware"> The middleware to be invoked. </param>
    /// <remarks>
    /// The middleware function takes an instance of delegate that was previously invoked as an input and returns the currently invoked delegate instance as an output.
    /// </remarks>
    internal void Use(Func<Action<char>, Action<char>> middleware)
    {
        // Keeps a reference to the currently invoked delegate instance.
        _inputDelegate = middleware(_inputDelegate);
    }
}
