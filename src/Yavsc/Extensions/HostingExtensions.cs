using System.Globalization;
using System.Security.Permissions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using IdentityServer4;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Yavsc.Abstract.Workflow;
using Yavsc.Billing;
using Yavsc.Helpers;
using Yavsc.Interface;
using Yavsc.Models;
using Yavsc.Models.Billing;
using Yavsc.Models.Haircut;
using Yavsc.Models.Market;
using Yavsc.Models.Workflow;
using Yavsc.Services;
using Yavsc.Settings;

namespace Yavsc.Extensions;


internal static class HostingExtensions
{

    public static IApplicationBuilder ConfigureFileServerApp(this IApplicationBuilder app,
                 bool enableDirectoryBrowsing = false)
        {

            var userFilesDirInfo = new DirectoryInfo(Config.SiteSetup.Blog);
            AbstractFileSystemHelpers.UserFilesDirName = userFilesDirInfo.FullName;

            if (!userFilesDirInfo.Exists) userFilesDirInfo.Create();

            Config.UserFilesOptions = new FileServerOptions()
            {
                FileProvider = new PhysicalFileProvider(AbstractFileSystemHelpers.UserFilesDirName),
                RequestPath = PathString.FromUriComponent(Constants.UserFilesPath),
                EnableDirectoryBrowsing = enableDirectoryBrowsing,
            };
            Config.UserFilesOptions.EnableDefaultFiles = true;
            Config.UserFilesOptions.StaticFileOptions.ServeUnknownFileTypes = true;

            var avatarsDirInfo = new DirectoryInfo(Config.SiteSetup.Avatars);
            if (!avatarsDirInfo.Exists) avatarsDirInfo.Create();
            Config.AvatarsDirName = avatarsDirInfo.FullName;

            Config.AvatarsOptions = new FileServerOptions()
            {
                FileProvider = new PhysicalFileProvider(Config.AvatarsDirName),
                RequestPath = PathString.FromUriComponent(Constants.AvatarsPath),
                EnableDirectoryBrowsing = enableDirectoryBrowsing
            };


            var gitdirinfo = new DirectoryInfo(Config.SiteSetup.GitRepository);
            Config.GitDirName = gitdirinfo.FullName;
            if (!gitdirinfo.Exists) gitdirinfo.Create();
            Config.GitOptions = new FileServerOptions()
            {
                FileProvider = new PhysicalFileProvider(Config.GitDirName),
                RequestPath = PathString.FromUriComponent(Constants.GitPath),
                EnableDirectoryBrowsing = enableDirectoryBrowsing,
            };
            Config.GitOptions.DefaultFilesOptions.DefaultFileNames.Add("index.md");
            Config.GitOptions.StaticFileOptions.ServeUnknownFileTypes = true;

            app.UseFileServer(Config.UserFilesOptions);

            app.UseFileServer(Config.AvatarsOptions);
 
            app.UseFileServer(Config.GitOptions);
            app.UseStaticFiles();
            return app;
        }
    public static void ConfigureWorkflow()
        {
            foreach (var a in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var c in a.GetTypes())
                {
                    if (c.IsClass && !c.IsAbstract &&                    
                        c.GetInterface("ISpecializationSettings") != null)
                    {
                        Config.ProfileTypes.Add(c);
                    }
                }
            }

            foreach (var propertyInfo in typeof(ApplicationDbContext).GetProperties())
            {
                foreach (var attr in propertyInfo.CustomAttributes)
                {
                    // something like a DbSet?
                    if (typeof(Yavsc.Attributes.ActivitySettingsAttribute).IsAssignableFrom(attr.AttributeType))
                    {
                            BillingService.UserSettings.Add(propertyInfo);
                    }
                }
            }

            RegisterBilling<HairCutQuery>(BillingCodes.Brush, new Func<ApplicationDbContext,long,IDecidableQuery>
            ( ( db, id) => 
            {
              var query = db.HairCutQueries.Include(q=>q.Prestation).Include(q=>q.Regularisation).Single(q=>q.Id == id)  ; 
              query.SelectedProfile = db.BrusherProfile.Single(b=>b.UserId == query.PerformerId);
              return query;
            })) ;

            RegisterBilling<HairMultiCutQuery>(BillingCodes.MBrush,new Func<ApplicationDbContext,long,IDecidableQuery>
            ( (db, id) =>  db.HairMultiCutQueries.Include(q=>q.Regularisation).Single(q=>q.Id == id)));
            
            RegisterBilling<RdvQuery>(BillingCodes.Rdv, new Func<ApplicationDbContext,long,IDecidableQuery>
            ( (db, id) =>  db.RdvQueries.Include(q=>q.Regularisation).Single(q=>q.Id == id)));
        }
  public static void RegisterBilling<T>(string code, Func<ApplicationDbContext,long,IDecidableQuery> getter) where T : IBillable
        {
            BillingService.Billing.Add(code,getter) ;
            BillingService.GlobalBillingMap.Add(typeof(T).Name,code);
        }

    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
            var siteSection = builder.Configuration.GetSection("Site");
            var smtpSection = builder.Configuration.GetSection("Smtp");
            var paypalSection = builder.Configuration.GetSection("Authentication:PayPal");
            // OAuth2AppSettings
            var googleAuthSettings = builder.Configuration.GetSection("Authentication:Google");
            
            string? googleClientFile = builder.Configuration["Authentication:Google:GoogleWebClientJson"];
            string? googleServiceAccountJsonFile = builder.Configuration["Authentication:Google:GoogleServiceAccountJson"];
            if (googleClientFile != null)
            {
                Config.GoogleWebClientConfiguration = new ConfigurationBuilder().AddJsonFile(googleClientFile).Build();
            }

            if (googleServiceAccountJsonFile != null)
            {
                FileInfo safile = new FileInfo(googleServiceAccountJsonFile);
                Config.GServiceAccount = JsonConvert.DeserializeObject<GoogleServiceAccount>(safile.OpenText().ReadToEnd());
            }
            string? googleClientId = builder.Configuration["Authentication:Google:ClientId"];
            string? googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        var services = builder.Services;

        services.Configure<SiteSettings>(siteSection);
        services.Configure<SmtpSettings>(smtpSection);
        services.Configure<PayPalSettings>(paypalSection);
        services.Configure<GoogleAuthSettings>(googleAuthSettings);
        
        services.AddRazorPages();
        services.AddSignalR(o =>
        {
            o.EnableDetailedErrors = true;
        });

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                options.EmitStaticAudienceClaim = true;
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients)
            ;
        services.AddSession();
          // TODO .AddServerSideSessionStore<YavscServerSideSessionStore>()
            
        
        services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                // register your IdentityServer with Google at https://console.developers.google.com
                // enable the Google+ API
                // set the redirect URI to https://localhost:5001/signin-google
                options.ClientId = googleClientId;
                options.ClientSecret = googleClientSecret;
            });
            services.Configure<RequestLocalizationOptions>(options =>
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

            services.AddCors(options =>
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
            _ = services.AddTransient<IFileSystemAuthManager, FileSystemAuthManager>();

            services.AddMvc(config =>
            {
                /* var policy = new AuthorizationPolicyBuilder()
                  .RequireAuthenticatedUser()
                    .Build();
                config.Filters.Add(new AuthorizeFilter(policy)); */
                config.Filters.Add(new ProducesAttribute("application/json"));
                // config.ModelBinders.Insert(0,new MyDateTimeModelBinder());
                // config.ModelBinders.Insert(0,new MyDecimalModelBinder());
                config.EnableEndpointRouting = true;
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
            var dataDir = new DirectoryInfo(builder.Configuration["Site:DataDir"]);
            // Add session related services.
            
            services.AddDataProtection().PersistKeysToFileSystem(dataDir);
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
            _ = services.AddControllersWithViews()
            .AddNewtonsoftJson();
            LoadGoogleConfig(builder.Configuration);
            
        return builder.Build();
    }
    public static WebApplication ConfigurePipeline(this WebApplication app)
    { 
    
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        
        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
        //app.UseSignalR();
        app.UseAuthorization();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages()
        .RequireAuthorization();
        app.MapHub<ChatHub>("/chatHub");
        app.MapAreaControllerRoute("api", "api", "~/api/{controller}/{action}/{id?}");
        ConfigureWorkflow();
        var services = app.Services;
        ILoggerFactory loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var siteSettings = services.GetRequiredService<IOptions<SiteSettings>>();
        var smtpSettings = services.GetRequiredService<IOptions<SmtpSettings>>();
        var payPalSettings = services.GetRequiredService<IOptions<PayPalSettings>>();
        var googleAuthSettings = services.GetRequiredService<IOptions<GoogleAuthSettings>>();
        var authorizationService = services.GetRequiredService<IAuthorizationService>();
        var localization = services.GetRequiredService<IStringLocalizer<YavscLocalization>>();
        Startup.Configure(app, siteSettings, smtpSettings, authorizationService, 
            payPalSettings, googleAuthSettings, localization, loggerFactory,
            app.Environment.EnvironmentName );
        app.ConfigureFileServerApp();
        
        return app;
    }

  static void LoadGoogleConfig(IConfigurationRoot configuration)
    {
        string? googleClientFile = configuration["Authentication:Google:GoogleWebClientJson"];
        string? googleServiceAccountJsonFile = configuration["Authentication:Google:GoogleServiceAccountJson"];
        if (googleClientFile != null)
        {
            Config.GoogleWebClientConfiguration = new ConfigurationBuilder().AddJsonFile(googleClientFile).Build();
        }

        if (googleServiceAccountJsonFile != null)
        {
            FileInfo safile = new FileInfo(googleServiceAccountJsonFile);
            Config.GServiceAccount = JsonConvert.DeserializeObject<GoogleServiceAccount>(safile.OpenText().ReadToEnd());
        }
    }
}
