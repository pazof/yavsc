
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
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
using Yavsc.Formatters;
using Yavsc.Models;
using Yavsc.Services;
using Yavsc.ViewModels.Auth;

namespace Yavsc
{
    public partial class Startup
    {
        public static string ConnectionString { get; private set; }
        public static string UserBillsDirName { private set; get; }
        public static string AvatarsDirName { private set; get; }
        public static string Authority { get; private set; }
        public static string Audience { get; private set; }
        public static SiteSettings SiteSetup { get; private set; }

        private static ILogger logger;
        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
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
                    new CultureInfo("en"),
                    new CultureInfo("fr")
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
                //  return new ProviderCultureResult("en");
                //}));
            });



            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<SiteSettings>), typeof(OptionsManager<SiteSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<SmtpSettings>), typeof(OptionsManager<SmtpSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<GoogleAuthSettings>), typeof(OptionsManager<GoogleAuthSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<CompanyInfoSettings>), typeof(OptionsManager<CompanyInfoSettings>)));

            // DataProtection
            ConfigureProtectionServices(services);

            // Add framework services.
            services.AddEntityFramework()
              .AddNpgsql()
              .AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(ConnectionString))
              ;

            ConfigureOAuthServices(services);

            services.AddCors(
            /*
            options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.WithOrigins("http://lua.pschneider.fr");
                });
            }
            */
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
            services.AddSingleton<IAuthorizationHandler, CommandEditHandler>();
            services.AddSingleton<IAuthorizationHandler, CommandViewHandler>();
            services.AddSingleton<IAuthorizationHandler, PostUserFileHandler>();
            services.AddSingleton<IAuthorizationHandler, EstimateViewHandler>();

            services.AddMvc(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                  .RequireAuthenticatedUser()
                    .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
                config.Filters.Add(new ProducesAttribute("application/json"));
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

            services.AddScoped<LanguageActionFilter>();

            // Inject ticket formatting
            services.AddTransient(typeof(ISecureDataFormat<>), typeof(SecureDataFormat<>));
            services.AddTransient<Microsoft.AspNet.Authentication.ISecureDataFormat<AuthenticationTicket>, Microsoft.AspNet.Authentication.SecureDataFormat<AuthenticationTicket>>();
            services.AddTransient<ISecureDataFormat<AuthenticationTicket>, TicketDataFormat>();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<IGoogleCloudMessageSender, AuthMessageSender>();
            // TODO for SMS: services.AddTransient<ISmsSender, AuthMessageSender>();

            services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
        IOptions<SiteSettings> siteSettings,
        IOptions<RequestLocalizationOptions> localizationOptions,
        IOptions<OAuth2AppSettings> oauth2SettingsContainer,
        RoleManager<IdentityRole> roleManager,
         ILoggerFactory loggerFactory)
        {
            SiteSetup = siteSettings.Value;
            Startup.UserFilesDirName =  new DirectoryInfo(siteSettings.Value.UserFiles.Blog).FullName;
            Startup.UserBillsDirName =  new DirectoryInfo(siteSettings.Value.UserFiles.Bills).FullName;

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
                loggerFactory.MinimumLevel = LogLevel.Verbose;
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
                        // TODO Send an email to the Admin
                    }
                    else throw ex;
                }
            }
            Task.Run(async () =>
            {
                // Creates roles when they don't exist
                foreach (string roleName in new string[] {Constants.AdminGroupName,
                Constants.StarGroupName, Constants.PerformerGroupName,
                Constants.FrontOfficeGroupName,
                Constants.StarHunterGroupName
                })
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        var role = new IdentityRole { Name = roleName };
                        var resultCreate = await roleManager.CreateAsync(role);
                        if (!resultCreate.Succeeded)
                        {
                            throw new Exception("The role '{roleName}' does not exist and could not be created.");
                        }
                    }
                // FIXME In a perfect world, connection records should be dropped at shutdown, but:

                using (var db = new ApplicationDbContext())
                {
                    foreach (var c in db.Connections)
                        db.Connections.Remove(c);
                    db.SaveChanges();
                }
            });


            app.UseIISPlatformHandler(options =>
            {
                options.AuthenticationDescriptions.Clear();
                options.AutomaticAuthentication = false;
            });

            Authority = siteSettings.Value.Authority;
            Audience = siteSettings.Value.Audience;

            ConfigureOAuthApp(app, siteSettings.Value);
            ConfigureFileServerApp(app, siteSettings.Value, env);
            ConfigureWebSocketsApp(app, siteSettings.Value, env);

            app.UseRequestLocalization(localizationOptions.Value, (RequestCulture)new RequestCulture((string)"en"));

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        // Entry point for the application.
        public static void Main(string[] args) => Microsoft.AspNet.Hosting.WebApplication.Run<Startup>(args);
    }

}
// 