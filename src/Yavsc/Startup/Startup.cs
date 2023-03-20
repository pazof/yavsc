
using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Yavsc.Helpers;
using static System.Environment;

namespace Yavsc
{
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Models;
    using Services;

    public partial class Startup
    {
        public static string AvatarsDirName { private set; get; }
        public static string GitDirName { private set; get; }
        public static string Authority { get; private set; }
        public static string Temp { get; set; }
        public static SiteSettings SiteSetup { get; private set; }
        public static GoogleServiceAccount GServiceAccount { get; private set; }

        public static string HostingFullName { get; set; }

        public static PayPalSettings PayPalSettings { get; private set; }
        private static ILogger _logger;



        public Startup( IWebHostEnvironment env)
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnHandledException;

            var devtag = env.IsDevelopment() ? "D" : "";
            var prodtag = env.IsProduction() ? "P" : "";
            var stagetag = env.IsStaging() ? "S" : "";

            HostingFullName = $"{env.EnvironmentName} [{prodtag}/{devtag}/{stagetag}]";
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);


            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

            var auth = Configuration["Site:Authority"];
            var cxstr = Configuration["ConnectionStrings:Default"];
            ConnectionString = cxstr;

            AppDomain.CurrentDomain.SetData(Constants.YavscConnectionStringEnvName, ConnectionString);

            var googleClientFile = Configuration["Authentication:Google:GoogleWebClientJson"];
            var googleServiceAccountJsonFile = Configuration["Authentication:Google:GoogleServiceAccountJson"];
            if (googleClientFile != null)
                GoogleWebClientConfiguration = new ConfigurationBuilder().AddJsonFile(googleClientFile).Build();
            if (googleServiceAccountJsonFile != null)
            {
                var safile = new FileInfo(googleServiceAccountJsonFile);
                GServiceAccount = JsonConvert.DeserializeObject<GoogleServiceAccount>(safile.OpenText().ReadToEnd());
            }
        }

        // never hit ...
        private void OnUnHandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.LogError(sender.ToString());
            _logger.LogError(JsonConvert.SerializeObject(e.ExceptionObject));
        }

        public static string ConnectionString { get; set; }
        public static GoogleAuthSettings GoogleSettings { get; set; }
        public IConfigurationRoot Configuration { get; set; }
        public static IConfigurationRoot GoogleWebClientConfiguration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();

            // Database connection

            services.AddOptions();
            var siteSettings = Configuration.GetSection("Site");
            services.Configure<SiteSettings>(siteSettings);
            var smtpSettings = Configuration.GetSection("Smtp");
            services.Configure<SmtpSettings>(smtpSettings);
            var googleSettings = Configuration.GetSection("Authentication").GetSection("Google");
            services.Configure<GoogleAuthSettings>(googleSettings);
            var cinfoSettings = Configuration.GetSection("Authentication").GetSection("Societeinfo");
            services.Configure<CompanyInfoSettings>(cinfoSettings);
            var oauthFacebookSettings = Configuration.GetSection("Authentication").GetSection("Facebook");
            services.Configure<FacebookOAuth2AppSettings>(oauthFacebookSettings);
            var paypalSettings = Configuration.GetSection("Authentication").GetSection("PayPal");
            services.Configure<PayPalSettings>(paypalSettings);

            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<SiteSettings>), typeof(OptionsManager<SiteSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<SmtpSettings>), typeof(OptionsManager<SmtpSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<GoogleAuthSettings>), typeof(OptionsManager<GoogleAuthSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<CompanyInfoSettings>), typeof(OptionsManager<CompanyInfoSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<RequestLocalizationOptions>), typeof(OptionsManager<RequestLocalizationOptions>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IDiskUsageTracker), typeof(DiskUsageTracker)));
            
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en"),
                    new CultureInfo("fr"),
                    new CultureInfo("pt")
                };

                var supportedUICultures = new[]
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

            // Add framework services.
            services.AddEntityFrameworkNpgsql()
              .AddDbContext<ApplicationDbContext>();


            services.AddCors(

            options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.WithOrigins("*");
                });
            }

            );

            // Add session related services.
            services.AddSession();

            // Add the system clock service
            services.AddSingleton<ISystemClock, SystemClock>();

            services.AddAuthorization(options =>
            {

                options.AddPolicy("AdministratorOnly", policy =>
                {
                    policy.RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", Constants.AdminGroupName);
                });

                options.AddPolicy("FrontOffice", policy => policy.RequireRole(Constants.FrontOfficeGroupName));
                options.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes("yavsc")
                    .RequireAuthenticatedUser().Build());
                // options.AddPolicy("EmployeeId", policy => policy.RequireClaim("EmployeeId", "123", "456"));
                // options.AddPolicy("BuildingEntry", policy => policy.Requirements.Add(new OfficeEntryRequirement()));
                options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
            });

        /* FIXME    services.AddSingleton<IAuthorizationHandler, HasBadgeHandler>();
            services.AddSingleton<IAuthorizationHandler, HasTemporaryPassHandler>();
            services.AddSingleton<IAuthorizationHandler, BlogEditHandler>();
            services.AddSingleton<IAuthorizationHandler, BlogViewHandler>();
            services.AddSingleton<IAuthorizationHandler, BillEditHandler>();
            services.AddSingleton<IAuthorizationHandler, BillViewHandler>();
            services.AddSingleton<IAuthorizationHandler, PostUserFileHandler>();
            services.AddSingleton<IAuthorizationHandler, ViewFileHandler>();
            services.AddSingleton<IAuthorizationHandler, SendMessageHandler>(); */

            services.AddSingleton<IConnexionManager, HubConnectionManager>();
            services.AddSingleton<ILiveProcessor, LiveProcessor>();
            services.AddSingleton<IFileSystemAuthManager, FileSystemAuthManager>();

            services.AddMvc(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                  .RequireAuthenticatedUser()
                    .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
                config.Filters.Add(new ProducesAttribute("application/json"));
                // config.ModelBinders.Insert(0,new MyDateTimeModelBinder());
                // config.ModelBinders.Insert(0,new MyDecimalModelBinder());
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
            services.AddTransient(typeof(ISecureDataFormat<>), typeof(SecureDataFormat<>));
            services.AddTransient<ISecureDataFormat<AuthenticationTicket>, SecureDataFormat<AuthenticationTicket>>();
            services.AddTransient<ISecureDataFormat<AuthenticationTicket>, TicketDataFormat>();

            // Add application services.
            services.AddTransient<IEmailSender, MailSender>();
            services.AddTransient<IYavscMessageSender, YavscMessageSender>();
            services.AddTransient<IBillingService, BillingService>();
            services.AddTransient<IDataStore, FileDataStore>((sp) => new FileDataStore("googledatastore", false));
            services.AddTransient<ICalendarManager, CalendarManager>();

            // TODO for SMS: services.AddTransient<ISmsSender, AuthMessageSender>();

            services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });
        }

        public static IServiceProvider Services { get; private set; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, 
            IOptions<SiteSettings> siteSettings,
        IAuthorizationService authorizationService,
        IOptions<PayPalSettings> payPalSettings,
        IOptions<GoogleAuthSettings> googleSettings,
        IStringLocalizer<Yavsc.YavscLocalisation> localizer,
         ILoggerFactory loggerFactory,
         IWebHostEnvironment env)
        {
            Services = app.ApplicationServices;
            GoogleSettings = googleSettings.Value;
            ResourcesHelpers.GlobalLocalizer = localizer;
            SiteSetup = siteSettings.Value;
            Authority = siteSettings.Value.Authority;
            var blogsDir = siteSettings.Value.Blog;
            if (blogsDir == null) throw new Exception("blogsDir is not set.");
            var billsDir = siteSettings.Value.Bills;
            if (billsDir == null) throw new Exception("billsDir is not set.");

            AbstractFileSystemHelpers.UserFilesDirName = new DirectoryInfo(blogsDir).FullName;
            AbstractFileSystemHelpers.UserBillsDirName = new DirectoryInfo(billsDir).FullName;
            Temp = siteSettings.Value.TempDir;
            PayPalSettings = payPalSettings.Value;

            // TODO implement an installation & upgrade procedure
            // Create required directories
            foreach (string dir in new string[] { AbstractFileSystemHelpers.UserFilesDirName, AbstractFileSystemHelpers.UserBillsDirName, SiteSetup.TempDir })
            {
                if (dir == null) throw new Exception(nameof(dir));

                DirectoryInfo di = new DirectoryInfo(dir);
                if (!di.Exists) di.Create();
            }

            _logger = loggerFactory.CreateLogger<Startup>();
            app.UseStatusCodePagesWithReExecute("/Home/Status/{0}");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWelcomePage("/welcome");
            }
            else
            {
                // For more details on creating database during deployment see http://go.microsoft.com/fwlink/?LinkID=615859

                app.UseExceptionHandler("/Home/Error");
                try
                {
                    using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                            .CreateScope())
                    {
                        serviceScope.ServiceProvider.GetService<ApplicationDbContext>()
                                .Database.Migrate();
                    }
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException is InvalidOperationException)
                    // nothing to do ?
                    {
                        // TODO (or not) Hit the developper
                    }
                    else throw ex;
                }
            }
            // before fixing the security protocol, let beleive our lib it's done with it.
            var cxmgr = PayPal.Manager.ConnectionManager.Instance;
            // then, fix it.
            // ServicePointManager.SecurityProtocol = (SecurityProtocolType)0xC00; // Tls12, required by PayPal


            app.UseSession();

            ConfigureFileServerApp(app, SiteSetup, env, authorizationService);
            
            app.UseRequestLocalization();

            ConfigureWorkflow();
            ConfigureWebSocketsApp(app);

           
            _logger.LogInformation("LocalApplicationData: " + Environment.GetFolderPath(SpecialFolder.LocalApplicationData, SpecialFolderOption.DoNotVerify));
            
            CheckApp(env, loggerFactory);
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>("/chat");
            });

        }

    }
}
//
