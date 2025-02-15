using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.Util.Store;
using IdentityServer8;
using IdentityServer8.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
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
using Yavsc.Models.Workflow;
using Yavsc.Services;
using Yavsc.Settings;
using Yavsc.ViewModels.Auth;

namespace Yavsc.Extensions;


public static class HostingExtensions
{
    #region files config
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

    #endregion

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

        RegisterBilling<HairCutQuery>(BillingCodes.Brush, new Func<ApplicationDbContext, long, IDecidableQuery>
        ((db, id) =>
        {
            var query = db.HairCutQueries.Include(q => q.Prestation).Include(q => q.Regularisation).Single(q => q.Id == id);
            query.SelectedProfile = db.BrusherProfile.Single(b => b.UserId == query.PerformerId);
            return query;
        }));

        RegisterBilling<HairMultiCutQuery>(BillingCodes.MBrush, new Func<ApplicationDbContext, long, IDecidableQuery>
        ((db, id) => db.HairMultiCutQueries.Include(q => q.Regularisation).Single(q => q.Id == id)));

        RegisterBilling<RdvQuery>(BillingCodes.Rdv, new Func<ApplicationDbContext, long, IDecidableQuery>
        ((db, id) => db.RdvQueries.Include(q => q.Regularisation).Single(q => q.Id == id)));
    }
    public static void RegisterBilling<T>(string code, Func<ApplicationDbContext, long, IDecidableQuery> getter) where T : IBillable
    {
        BillingService.Billing.Add(code, getter);
        BillingService.GlobalBillingMap.Add(typeof(T).Name, code);
    }

    internal static WebApplication ConfigureWebAppServices(this WebApplicationBuilder builder)
    {
        IServiceCollection services = LoadConfiguration(builder);

        services.AddRazorPages();

        services.AddSignalR(o =>
        {
            o.EnableDetailedErrors = true;
        });

        AddIdentityDBAndStores(builder).AddDefaultTokenProviders();;

        AddIdentityServer(builder);
        //services.AddScoped<IProfileService, ProfileService>();

        services.AddSession();

        // TODO .AddServerSideSessionStore<YavscServerSideSessionStore>()

        AddAuthentication(services, builder.Configuration);

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

        services.AddTransient<ITrueEmailSender, MailSender>()
        .AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, MailSender>()
        .AddTransient<IYavscMessageSender, YavscMessageSender>()
        .AddTransient<IBillingService, BillingService>()
        .AddTransient<IDataStore, FileDataStore>((sp) => new FileDataStore("googledatastore", false))
        .AddTransient<ICalendarManager, CalendarManager>();

        // TODO for SMS: services.AddTransient<ISmsSender, AuthMessageSender>();

        _ = services.AddLocalization(options =>
        {
            options.ResourcesPath = "Resources";
        });
        var dataDirConfig = builder.Configuration["Site:DataDir"] ?? "DataDir";

        var dataDir = new DirectoryInfo(dataDirConfig);
        // Add session related services.

        services.AddDataProtection().PersistKeysToFileSystem(dataDir);
        AddYavscPolicies(services);

        services.AddSingleton<IAuthorizationHandler, PermissionHandler>();


        // accepts any access token issued by identity server

        return builder.Build();
    }

    public static IdentityBuilder AddIdentityDBAndStores(this WebApplicationBuilder builder)
    {
        IServiceCollection services = builder.Services;
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        return services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
            
    }

    private static void AddYavscPolicies(IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("ApiScope", policy =>
            {
                policy.RequireAuthenticatedUser()
                .RequireClaim("scope", "scope2");
            });
            options.AddPolicy("Performer", policy =>
            {
                policy
                    .RequireAuthenticatedUser()
                    .RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Performer");
            });
            options.AddPolicy("AdministratorOnly", policy =>
            {
                _ = policy.RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", Constants.AdminGroupName);
            });

            options.AddPolicy("FrontOffice", policy => policy.RequireRole(Constants.FrontOfficeGroupName));

            // options.AddPolicy("EmployeeId", policy => policy.RequireClaim("EmployeeId", "123", "456"));
            // options.AddPolicy("BuildingEntry", policy => policy.Requirements.Add(new OfficeEntryRequirement()));
            options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
            options.AddPolicy("IsTheAuthor", policy => policy.Requirements.Add(new EditPermission()));
        })
        .AddCors(options =>
        {
            options.AddPolicy("default", builder =>
            {
                _ = builder.WithOrigins("*")
                .AllowAnyHeader()
                .AllowAnyMethod();
            });

        });
    }

    public static IServiceCollection LoadConfiguration(this WebApplicationBuilder builder)
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
        var services = builder.Services;
        _ = services.AddControllersWithViews()
            .AddNewtonsoftJson();
        LoadGoogleConfig(builder.Configuration);

        services.Configure<SiteSettings>(siteSection);
        services.Configure<SmtpSettings>(smtpSection);
        services.Configure<PayPalSettings>(paypalSection);
        services.Configure<GoogleAuthSettings>(googleAuthSettings);
        ConfigureRequestLocalization(services);

        return services;
    }

    private static void AddAuthentication(IServiceCollection services, IConfigurationRoot configurationRoot)
    {
           string? googleClientId = configurationRoot["Authentication:Google:ClientId"];
        string? googleClientSecret = configurationRoot["Authentication:Google:ClientSecret"];
     
        var authenticationBuilder = services.AddAuthentication()
        .AddJwtBearer("Bearer", options =>
            {
                options.IncludeErrorDetails = true;
                options.Authority = "https://localhost:5001";
                options.TokenValidationParameters =
                    new() { ValidateAudience = false };
            });
            
        authenticationBuilder.AddGoogle(options =>
        {
            options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

            // register your IdentityServer with Google at https://console.developers.google.com
            // enable the Google+ API
            // set the redirect URI to https://localhost:5001/signin-google
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
        });
    }
    private static IIdentityServerBuilder AddIdentityServer(WebApplicationBuilder builder)
    {
        var identityServerBuilder = builder.Services.AddIdentityServer()
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryClients(Config.Clients)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddAspNetIdentity<ApplicationUser>()
            .AddJwtBearerClientAuthentication()
          //  .AddProfileService<ProfileService>()
            ;
        if (builder.Environment.IsDevelopment())
        {
            identityServerBuilder.AddDeveloperSigningCredential();
        }
        else
        {
            var key = builder.Configuration["YOUR-KEY-NAME"];
            var pfxBytes = Convert.FromBase64String(key);
            var cert = new X509Certificate2(pfxBytes, (string)null, X509KeyStorageFlags.MachineKeySet);
            identityServerBuilder.AddSigningCredential(cert);
        }
        return identityServerBuilder;
    }

    private static void ConfigureRequestLocalization(IServiceCollection services)
    {
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
    }


    public static WebApplication ConfigurePipeline(this WebApplication app)
    {

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();
        app.UseCors("default");
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
            app.Environment.EnvironmentName);
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
