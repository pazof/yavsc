
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Web.Optimization;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Localization;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Net.Http.Headers;

namespace Yavsc
{
    using System.Collections.Generic;
    using System.Net;
    using Formatters;
    using Google.Apis.Util.Store;
    using Microsoft.Extensions.Localization;
    using Models;
    using PayPal.Manager;
    using Services;
    using ViewModels.Auth.Handlers;
    using static System.Environment;

    public partial class Startup
    {
        public static string ConnectionString { get; private set; }
        public static string UserBillsDirName { private set; get; }
        public static string AvatarsDirName { private set; get; }
        public static string Authority { get; private set; }
        public static string Temp { get; set; }
        public static SiteSettings SiteSetup { get; private set; }

        public static string HostingFullName { get; set; }

        public static PayPalSettings PayPalSettings { get; private set; }
        private static ILogger logger;
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

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
                BundleTable.EnableOptimizations = false;
            }

            BundleConfig.RegisterBundles(BundleTable.Bundles);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            ConnectionString = Configuration["Data:DefaultConnection:ConnectionString"];

        }
        public static GoogleAuthSettings GoogleSettings { get; set; }
        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            var siteSettings = Configuration.GetSection("Site");
            services.Configure<SiteSettings>(siteSettings);
            var smtpSettings = Configuration.GetSection("Smtp");
            services.Configure<SmtpSettings>(smtpSettings);
            var googleSettings = Configuration.GetSection("Authentication").GetSection("Google");
            services.Configure<GoogleAuthSettings>(googleSettings);
            var cinfoSettings = Configuration.GetSection("Authentication").GetSection("Societeinfo");
            services.Configure<CompanyInfoSettings>(cinfoSettings);
            var oauthLocalAppSettings = Configuration.GetSection("Authentication").GetSection("OAuth2LocalApp");
            services.Configure<OAuth2AppSettings>(oauthLocalAppSettings);
            var oauthFacebookSettings = Configuration.GetSection("Authentication").GetSection("Facebook");
            services.Configure<FacebookOAuth2AppSettings>(oauthFacebookSettings);
            var paypalSettings = Configuration.GetSection("Authentication").GetSection("PayPal");
            services.Configure<PayPalSettings>(paypalSettings);

            /*    services.Configure<MvcOptions>(options =>
                {
                    options.Filters.Add(new ProducesAttribute("text/x-tex"));
                    options.Filters.Add(new ProducesAttribute("text/pdf"));
                });*/

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en")
                };
                var supportedUICultures = new[]
                {
                    new CultureInfo("fr"),
                    new CultureInfo("en")
                };

                // You must explicitly state which cultures your application supports.
                // These are the cultures the app supports for formatting numbers, dates, etc.
                options.SupportedCultures = supportedCultures;

                // These are the cultures the app supports for UI strings, i.e. we have localized resources for.
                options.SupportedUICultures = supportedUICultures;

                // You can change which providers are configured to determine the culture for requests, or even add a custom
                // provider with your own logic. The providers will be asked in order to provide a culture for each request,
                // and the first to provide a non-null result that is in the configured supported cultures list will be used.
                // By default, the following built-in providers are configured:
                // - QueryStringRequestCultureProvider, sets culture via "culture" and "ui-culture" query string values, useful for testing
                // - CookieRequestCultureProvider, sets culture via "ASPNET_CULTURE" cookie
                // - AcceptLanguageHeaderRequestCultureProvider, sets culture via the "Accept-Language" request header

                //options.RequestCultureProviders.Insert(0, new CustomRequestCultureProvider(async context =>
                //{
                //  // My custom request culture logic
                //  return new ProviderCultureResult("fr");
                //}));

                options.RequestCultureProviders = new List<IRequestCultureProvider>
                    {
                        new QueryStringRequestCultureProvider { Options = options },
                        new CookieRequestCultureProvider { Options = options, CookieName="ASPNET_CULTURE" },
                        new AcceptLanguageHeaderRequestCultureProvider { Options = options }
                    };
            });

            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<SiteSettings>), typeof(OptionsManager<SiteSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<SmtpSettings>), typeof(OptionsManager<SmtpSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<GoogleAuthSettings>), typeof(OptionsManager<GoogleAuthSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<CompanyInfoSettings>), typeof(OptionsManager<CompanyInfoSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<RequestLocalizationOptions>), typeof(OptionsManager<RequestLocalizationOptions>)));
            // DataProtection
            ConfigureProtectionServices(services);

            // Add framework services.
            services.AddEntityFramework()
              .AddNpgsql()
              .AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(ConnectionString))
              ;

            ConfigureOAuthServices(services);

            services.AddCors(

            options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.WithOrigins("*");
                });
            }

            );
            // Add memory cache services
            services.AddCaching();

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
                // options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
            });

            services.AddSingleton<IAuthorizationHandler, HasBadgeHandler>();
            services.AddSingleton<IAuthorizationHandler, HasTemporaryPassHandler>();
            services.AddSingleton<IAuthorizationHandler, BlogEditHandler>();
            services.AddSingleton<IAuthorizationHandler, BlogViewHandler>();
            services.AddSingleton<IAuthorizationHandler, BillEditHandler>();
            services.AddSingleton<IAuthorizationHandler, BillViewHandler>();
            services.AddSingleton<IAuthorizationHandler, PostUserFileHandler>();
            services.AddSingleton<IAuthorizationHandler, ViewFileHandler>();

            services.AddMvc(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                  .RequireAuthenticatedUser()
                    .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
                config.Filters.Add(new ProducesAttribute("application/json"));
               // config.ModelBinders.Insert(0,new MyDateTimeModelBinder());
               // config.ModelBinders.Insert(0,new MyDecimalModelBinder());
                config.OutputFormatters.Add(new PdfFormatter());

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
            services.AddTransient<Microsoft.AspNet.Authentication.ISecureDataFormat<AuthenticationTicket>, Microsoft.AspNet.Authentication.SecureDataFormat<AuthenticationTicket>>();
            services.AddTransient<ISecureDataFormat<AuthenticationTicket>, TicketDataFormat>();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<IGoogleCloudMessageSender, AuthMessageSender>();
            services.AddTransient<IBillingService, BillingService>();
            services.AddTransient<IDataStore, FileDataStore>( (sp) => new FileDataStore("googledatastore",false) );
            services.AddTransient<ICalendarManager, CalendarManager>();
             
            // TODO for SMS: services.AddTransient<ISmsSender, AuthMessageSender>();

            services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });
            CheckServices(services);
        }


        public static IStringLocalizer GlobalLocalizer { get; private set; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
        IOptions<SiteSettings> siteSettings,
        IOptions<RequestLocalizationOptions> localizationOptions,
        IOptions<OAuth2AppSettings> oauth2SettingsContainer,
        RoleManager<IdentityRole> roleManager,
        IAuthorizationService authorizationService,
        IOptions<PayPalSettings> payPalSettings,
        IOptions<GoogleAuthSettings> googleSettings,
        IStringLocalizer<Yavsc.Resources.YavscLocalisation> localizer,
         ILoggerFactory loggerFactory)
        {
            GoogleSettings = googleSettings.Value;
            GlobalLocalizer = localizer;
            SiteSetup = siteSettings.Value;
            Authority = siteSettings.Value.Authority;
            Startup.UserFilesDirName =  new DirectoryInfo(siteSettings.Value.UserFiles.Blog).FullName;
            Startup.UserBillsDirName =  new DirectoryInfo(siteSettings.Value.UserFiles.Bills).FullName;
            Startup.Temp = siteSettings.Value.TempDir;
            PayPalSettings = payPalSettings.Value;

            // TODO implement an installation & upgrade procedure
            // Create required directories
            foreach (string dir in new string[] { UserFilesDirName, UserBillsDirName, SiteSetup.TempDir })
            {
                DirectoryInfo di = new DirectoryInfo(dir);
                if (!di.Exists) di.Create();
            }

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            logger = loggerFactory.CreateLogger<Startup>();
            app.UseStatusCodePagesWithReExecute("/Home/Status/{0}");

            if (env.IsDevelopment())
            {
                var logenvvar = Environment.GetEnvironmentVariable("ASPNET_LOG_LEVEL");
                if (logenvvar!=null)
                switch (logenvvar) {
                    case "info":
                        loggerFactory.MinimumLevel = LogLevel.Information;
                        break;
                    case "warn":
                        loggerFactory.MinimumLevel = LogLevel.Warning;
                        break;
                    case "err":
                        loggerFactory.MinimumLevel = LogLevel.Error;
                        break;
                    default:
                        loggerFactory.MinimumLevel = LogLevel.Information;
                        break;
                }


                app.UseDeveloperExceptionPage();
                app.UseRuntimeInfoPage();
                var epo = new ErrorPageOptions();
                epo.SourceCodeLineCount = 20;
                app.UseDeveloperExceptionPage(epo);
                app.UseDatabaseErrorPage(
                  x =>
                  {
                      x.EnableAll();
                      x.ShowExceptionDetails = true;
                  }
                );
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
            var cxmgr = ConnectionManager.Instance;
            // then, fix it.
            ServicePointManager.SecurityProtocol = (SecurityProtocolType) 0xC00; // Tls12, required by PayPal

            app.UseIISPlatformHandler(options =>
            {
                options.AuthenticationDescriptions.Clear();
                options.AutomaticAuthentication = false;
            });

            ConfigureOAuthApp(app, SiteSetup, logger);
            ConfigureFileServerApp(app, SiteSetup, env, authorizationService);
            ConfigureWebSocketsApp(app, SiteSetup, env);
            ConfigureWorkflow(app, SiteSetup, logger);
            app.UseRequestLocalization(localizationOptions.Value, (RequestCulture) new RequestCulture((string)"en-US"));
            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            logger.LogInformation("LocalApplicationData: "+Environment.GetFolderPath(SpecialFolder.LocalApplicationData, SpecialFolderOption.DoNotVerify));
           
            CheckApp(app, SiteSetup, env, loggerFactory);
        }

        // Entry point for the application.
        public static void Main(string[] args) => Microsoft.AspNet.Hosting.WebApplication.Run<Startup>(args);
    }

}
//
