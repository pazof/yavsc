
using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Localization;
using Yavsc.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Yavsc.Models;
using Yavsc.Services;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Yavsc.Interface;
using Yavsc.Settings;
using Microsoft.AspNetCore.DataProtection;

namespace Yavsc
{

    public partial class Startup
    {
        public static string AvatarsDirName { private set; get; }
        public static string GitDirName { private set; get; }
        public static string Authority { get; private set; }
        public static string Temp { get; set; }
        public static SiteSettings SiteSetup { get; private set; }
        public SmtpSettings SmtpSetup { get; private set; }
        public static GoogleServiceAccount GServiceAccount { get; private set; }

        public static string HostingFullName { get; set; }

        public static PayPalSettings PayPalSettings { get; private set; }
        private static ILogger _logger;



        public Startup(IWebHostEnvironment env)
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnHandledException;

            string devtag = env.IsDevelopment() ? "D" : "";
            string prodtag = env.IsProduction() ? "P" : "";
            string stagetag = env.IsStaging() ? "S" : "";

            HostingFullName = $"{env.EnvironmentName} [{prodtag}/{devtag}/{stagetag}]";
            // Set up configuration sources.
            IConfigurationBuilder builder = new ConfigurationBuilder()
            .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            _ = builder.AddEnvironmentVariables();
            Configuration = builder.Build();

       
            ConnectionString = Configuration["ConnectionStrings:Default"];

            AppDomain.CurrentDomain.SetData(Constants.YavscConnectionStringEnvName, ConnectionString);

            string? googleClientFile = Configuration["Authentication:Google:GoogleWebClientJson"];
            string? googleServiceAccountJsonFile = Configuration["Authentication:Google:GoogleServiceAccountJson"];
            if (googleClientFile != null)
            {
                GoogleWebClientConfiguration = new ConfigurationBuilder().AddJsonFile(googleClientFile).Build();
            }

            if (googleServiceAccountJsonFile != null)
            {
                FileInfo safile = new FileInfo(googleServiceAccountJsonFile);
                GServiceAccount = JsonConvert.DeserializeObject<GoogleServiceAccount>(safile.OpenText().ReadToEnd());
            }
        }

        // never hit ...
        private void OnUnHandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.LogError(sender.ToString());
            _logger.LogError(JsonConvert.SerializeObject(e.ExceptionObject));
        }

        public static string? ConnectionString { get; private set; }
        public static GoogleAuthSettings? GoogleSettings { get; private set; }
        public IConfigurationRoot Configuration { get; private set; }
        public static IConfigurationRoot? GoogleWebClientConfiguration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {


            IConfigurationSection siteSettings = Configuration.GetSection("Site");
            _ = services.Configure<SiteSettings>(siteSettings);
            IConfigurationSection smtpSettings = Configuration.GetSection("Smtp");
            _ = services.Configure<SmtpSettings>(smtpSettings);
            IConfigurationSection protectionSettings = Configuration.GetSection("DataProtection");
            _ = services.Configure<DataProtectionSettings>(smtpSettings);

            IConfigurationSection googleSettings = Configuration.GetSection("Authentication").GetSection("Google");
            _ = services.Configure<GoogleAuthSettings>(googleSettings);
            IConfigurationSection cinfoSettings = Configuration.GetSection("Authentication").GetSection("Societeinfo");
            _ = services.Configure<CompanyInfoSettings>(cinfoSettings);
            IConfigurationSection oauthFacebookSettings = Configuration.GetSection("Authentication").GetSection("Facebook");
            _ = services.Configure<FacebookOAuth2AppSettings>(oauthFacebookSettings);
            IConfigurationSection paypalSettings = Configuration.GetSection("Authentication").GetSection("PayPal");
            _ = services.Configure<PayPalSettings>(paypalSettings);
            

            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<SiteSettings>), typeof(OptionsManager<SiteSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<SmtpSettings>), typeof(OptionsManager<SmtpSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<GoogleAuthSettings>), typeof(OptionsManager<GoogleAuthSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<CompanyInfoSettings>), typeof(OptionsManager<CompanyInfoSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<RequestLocalizationOptions>), typeof(OptionsManager<RequestLocalizationOptions>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IDiskUsageTracker), typeof(DiskUsageTracker)));

            _ = services.Configure<RequestLocalizationOptions>(options =>
            {
                CultureInfo[] supportedCultures = new[]
                {
                    new CultureInfo("en"),
                    new CultureInfo("fr"),
                    new CultureInfo("pt")
                };

                CultureInfo[] supportedUICultures = new[]
                {
                    new CultureInfo("fr"),
                    new CultureInfo("en"),
                    new CultureInfo("pt")
                };

                // You must explicitly state which cultures your application supports.
                // These are the cultures the app supports for formatting numbers, dates, etc.
                options.SupportedCultures = supportedCultures;

                // These are the cultures the app supports for UI strings, i.e. we have localized resources for.
                options.SupportedUICultures = supportedUICultures;

                options.RequestCultureProviders = new List<IRequestCultureProvider>
                    {
                        new QueryStringRequestCultureProvider { Options = options },
                        new CookieRequestCultureProvider { Options = options, CookieName="ASPNET_CULTURE" },
                        new AcceptLanguageHeaderRequestCultureProvider { Options = options }
                    };
            });

            services.AddSignalR();
            
            services.AddOptions();

            _ = services.AddCors(options =>
                {
                    options.AddPolicy("CorsPolicy", builder =>
                    {
                        _ = builder.WithOrigins("*");
                    });
                });


            // Add the system clock service
            _ = services.AddSingleton<ISystemClock, SystemClock>();

           _ = services.AddSingleton<IConnexionManager, HubConnectionManager>();
            _ = services.AddSingleton<ILiveProcessor, LiveProcessor>();
            _ = services.AddSingleton<IFileSystemAuthManager, FileSystemAuthManager>();



          // Add framework services.
          

            /*services.AddSingleton<IAuthorizationHandler, HasBadgeHandler>();


            
            
            services.AddSingleton<IAuthorizationHandler, HasTemporaryPassHandler>();
            services.AddSingleton<IAuthorizationHandler, BlogEditHandler>();
            services.AddSingleton<IAuthorizationHandler, BlogViewHandler>();
            services.AddSingleton<IAuthorizationHandler, BillEditHandler>();
            services.AddSingleton<IAuthorizationHandler, BillViewHandler>();
            services.AddSingleton<IAuthorizationHandler, PostUserFileHandler>();
            services.AddSingleton<IAuthorizationHandler, ViewFileHandler>();
            services.AddSingleton<IAuthorizationHandler, SendMessageHandler>();*/
  _ = services.AddDbContext<ApplicationDbContext>()
            .AddIdentity<ApplicationUser, IdentityRole>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultUI()
            .AddDefaultTokenProviders();

           
           
            _ = services.AddMvc(config =>
            {
                /* var policy = new AuthorizationPolicyBuilder()
                  .RequireAuthenticatedUser()
                    .Build();
                config.Filters.Add(new AuthorizeFilter(policy)); */
                config.Filters.Add(new ProducesAttribute("application/json"));
                // config.ModelBinders.Insert(0,new MyDateTimeModelBinder());
                // config.ModelBinders.Insert(0,new MyDecimalModelBinder());
                config.EnableEndpointRouting = false;
            }).AddFormatterMappings(
                config => config.SetMediaTypeMappingForFormat("text/pdf",
                new MediaTypeHeaderValue("text/pdf"))
            ).AddFormatterMappings(
                config => config.SetMediaTypeMappingForFormat("text/x-tex",
                new MediaTypeHeaderValue("text/x-tex"))
            )
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix,
            options =>
            {
                options.ResourcesPath = "Resources";
            }).AddDataAnnotationsLocalization();

           

            // services.AddScoped<LanguageActionFilter>();

            // Inject ticket formatting
            //   services.AddTransient(typeof(ISecureDataFormat<>), typeof(SecureDataFormat<>));
            //  services.AddTransient<ISecureDataFormat<AuthenticationTicket>, SecureDataFormat<AuthenticationTicket>>();
            //  services.AddTransient<ISecureDataFormat<AuthenticationTicket>, TicketDataFormat>();

            // Add application services.
            _ = services.AddTransient<ITrueEmailSender, MailSender>();
            _ = services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, MailSender>();
            _ = services.AddTransient<IYavscMessageSender, YavscMessageSender>();
            _ = services.AddTransient<IBillingService, BillingService>();
            _ = services.AddTransient<IDataStore, FileDataStore>((sp) => new FileDataStore("googledatastore", false));
            _ = services.AddTransient<ICalendarManager, CalendarManager>();
            

            // TODO for SMS: services.AddTransient<ISmsSender, AuthMessageSender>();

            _ = services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });
            var datadi = new DirectoryInfo(Configuration["Keys:Dir"]);
            // Add session related services.
            services.AddSession();
            services.AddDataProtection().PersistKeysToFileSystem(datadi);

            services.AddAuthorization(options =>
            {

                options.AddPolicy("AdministratorOnly", policy =>
                {
                    _ = policy.RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", Constants.AdminGroupName);
                });

                options.AddPolicy("FrontOffice", policy => policy.RequireRole(Constants.FrontOfficeGroupName));
                options.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes("Bearer")
                    .RequireAuthenticatedUser().Build());
                // options.AddPolicy("EmployeeId", policy => policy.RequireClaim("EmployeeId", "123", "456"));
                // options.AddPolicy("BuildingEntry", policy => policy.Requirements.Add(new OfficeEntryRequirement()));
                options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
            });

            services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = siteSettings.GetValue<string>("Authority");
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false
                };
            });

            _ = services.AddControllersWithViews()
            .AddNewtonsoftJson();

        }

        public static IServiceProvider Services { get; private set; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IOptions<SiteSettings> siteSettings,
             IOptions<SmtpSettings> smtpSettings,
        IAuthorizationService authorizationService,
        IOptions<PayPalSettings> payPalSettings,
        IOptions<GoogleAuthSettings> googleSettings,
        IStringLocalizer<YavscLocalisation> localizer,
         ILoggerFactory loggerFactory,
         IWebHostEnvironment env)
        {
            Services = app.ApplicationServices;
            GoogleSettings = googleSettings.Value;
            ResourcesHelpers.GlobalLocalizer = localizer;
            SiteSetup = siteSettings.Value;
            SmtpSetup = smtpSettings.Value;
            Authority = siteSettings.Value.Authority;
            string blogsDir = siteSettings.Value.Blog ?? throw new Exception("blogsDir is not set.");
            string billsDir = siteSettings.Value.Bills ?? throw new Exception("billsDir is not set.");
            AbstractFileSystemHelpers.UserFilesDirName = new DirectoryInfo(blogsDir).FullName;
            AbstractFileSystemHelpers.UserBillsDirName = new DirectoryInfo(billsDir).FullName;
            Temp = siteSettings.Value.TempDir;
            PayPalSettings = payPalSettings.Value;

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            // TODO implement an installation & upgrade procedure
            // Create required directories
            foreach (string dir in new string[] { AbstractFileSystemHelpers.UserFilesDirName, AbstractFileSystemHelpers.UserBillsDirName, SiteSetup.TempDir })
            {
                if (dir == null)
                {
                    throw new Exception(nameof(dir));
                }

                DirectoryInfo di = new(dir);
                if (!di.Exists)
                {
                    di.Create();
                }
            }

            _logger = loggerFactory.CreateLogger<Startup>();
            _ = app.UseStatusCodePagesWithReExecute("/Home/Status/{0}");
            if (env.IsDevelopment())
            {
                _ = app.UseDeveloperExceptionPage();
                _ = app.UseWelcomePage("/welcome");
                app.UseMigrationsEndPoint();
            }
            else
            {
                // For more details on creating database during deployment see http://go.microsoft.com/fwlink/?LinkID=615859

                _ = app.UseExceptionHandler("/Home/Error");
                try
                {
                    using IServiceScope serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                            .CreateScope();
                    serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
                            .Database.Migrate();
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException is InvalidOperationException)
                    // nothing to do ?
                    {
                        // TODO (or not) Hit the developper
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }
            // before fixing the security protocol, let beleive our lib it's done with it.
            // _ = PayPal.Manager.ConnectionManager.Instance;
            // then, fix it.
            // ServicePointManager.SecurityProtocol = (SecurityProtocolType)0xC00; // Tls12, required by PayPal


            // _ = app.UseSession();

            ConfigureFileServerApp(app, SiteSetup, env, authorizationService);

            _ = app.UseRequestLocalization();

            ConfigureWorkflow();
            ConfigureWebSocketsApp(app);


            _logger.LogInformation("LocalApplicationData: " + Environment.GetFolderPath(
Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify));

            CheckApp(env, loggerFactory);

            app.UseSession();

            _ = app.UseStatusCodePages().UseStaticFiles().UseAuthentication();
            _ = app.UseMvcWithDefaultRoute();

        }

    }
}
//
