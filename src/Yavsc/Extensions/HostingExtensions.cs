using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.Util.Store;
using IdentityServer8;
using IdentityServer8.Services;
using IdentityServerHost.Quickstart.UI;
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
using Yavsc.Server.Helpers;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols.Configuration;

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


    internal static WebApplication ConfigureWebAppServices(this WebApplicationBuilder builder)
    {
        IServiceCollection services = LoadConfiguration(builder);

        //services.AddRazorPages();

      
        services.AddSession();

        // TODO .AddServerSideSessionStore<YavscServerSideSessionStore>()


        // Add the system clock service
        _ = services.AddSingleton<ISystemClock, SystemClock>();
        _ = services.AddSingleton<IConnexionManager, HubConnectionManager>();
        _ = services.AddSingleton<ILiveProcessor, LiveProcessor>();
        _ = services.AddTransient<IFileSystemAuthManager, FileSystemAuthManager>();
        
        AddIdentityDBAndStores(builder).AddDefaultTokenProviders();
        AddIdentityServer(builder);

        services.AddSignalR(o =>
        {
            o.EnableDetailedErrors = true;
        });
        
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

        services.AddScoped<IAuthorizationHandler, PermissionHandler>();


        AddAuthentication(builder);
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

    private static void AddAuthentication(WebApplicationBuilder builder)
    {
        IServiceCollection services=builder.Services;
        IConfigurationRoot configurationRoot=builder.Configuration;
        string? googleClientId = configurationRoot["Authentication:Google:ClientId"];
        string? googleClientSecret = configurationRoot["Authentication:Google:ClientSecret"];
     
        var authenticationBuilder = services.AddAuthentication();

        if (googleClientId!=null && googleClientSecret!=null)
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
        builder.Services.AddTransient<IProfileService,ProfileService>();
        var identityServerBuilder = builder.Services.AddIdentityServer(options =>
         {
             options.Events.RaiseErrorEvents = true;
             options.Events.RaiseInformationEvents = true;
             options.Events.RaiseFailureEvents = true;
             options.Events.RaiseSuccessEvents = true;

             // see https://IdentityServer8.readthedocs.io/en/latest/topics/resources.html
             options.EmitStaticAudienceClaim = true;
         })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryClients(Config.Clients)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddAspNetIdentity<ApplicationUser>()
            .AddProfileService<ProfileService>()
           ;
        if (builder.Environment.IsDevelopment())
        {
            identityServerBuilder.AddDeveloperSigningCredential();
        }
        else
        {
            var path = builder.Configuration["SigningCert:Path"];
            var pass = builder.Configuration["SigningCert:Password"];
            if (path == null)
                throw new InvalidConfigurationException("No signing cert path");
            FileInfo certFileInfo = new FileInfo(path);
            Debug.Assert(certFileInfo.Exists);
            RSA rsa = RSA.Create();
            rsa.ImportFromPem(File.ReadAllText(certFileInfo.FullName));
            var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
        {
          CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
        };
            identityServerBuilder.AddSigningCredential(signingCredentials);
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


    internal static WebApplication ConfigurePipeline(this WebApplication app)
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
        app.MapDefaultControllerRoute();
        //pp.MapRazorPages();
        app.MapHub<ChatHub>("/chatHub");
       
        WorkflowHelpers.ConfigureBillingService();
        
        var services = app.Services;
        ILoggerFactory loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var siteSettings = services.GetRequiredService<IOptions<SiteSettings>>();
        var smtpSettings = services.GetRequiredService<IOptions<SmtpSettings>>();
        var payPalSettings = services.GetRequiredService<IOptions<PayPalSettings>>();
        var googleAuthSettings = services.GetRequiredService<IOptions<GoogleAuthSettings>>();
        var localization = services.GetRequiredService<IStringLocalizer<YavscLocalization>>();
        Startup.Configure(app, siteSettings, smtpSettings,
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
