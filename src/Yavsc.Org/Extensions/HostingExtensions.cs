using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using Google.Apis.Util.Store;
using IdentityModel;
using IdentityServer8;
using IdentityServer8.EntityFramework.Entities;
using IdentityServer8.EntityFramework.Services;
using IdentityServer8.EntityFramework.Stores;
using IdentityServer8.Stores;
using IdentityServer8.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Yavsc.Helpers;
using Yavsc.Interface;
using Yavsc.Interfaces;
using Yavsc.Models;
using Yavsc.Server.Helpers;
using Yavsc.Services;
using Yavsc.Services.Kyc;
using Yavsc.Settings;
using Yavsc.ViewModels.Auth;
using static IdentityServer8.IdentityServerConstants;

namespace Yavsc.Extensions;


public static class HostingExtensions
{
    private const string InMemoryProviderName = "InMemory";

    public static WebApplication ConfigureWebAppServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen();

        IServiceCollection services = LoadConfiguration(builder);

        services.AddSession();

        // TODO .AddServerSideSessionStore<YavscServerSideSessionStore>()


        // Add the system clock service
        _ = services.AddSingleton<IConnexionManager, HubConnectionManager>();
        _ = services.AddSingleton<ILiveProcessor, LiveProcessor>();
        _ = services.AddTransient<IFileSystemAuthManager, FileSystemAuthManager>();

        AddIdentityDBAndStores(builder)
        .AddDefaultTokenProviders();
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
                .AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, MailSender>();
        

        services.AddTransient<IYavscMessageSender, YavscMessageSender>()
        .AddTransient<IBillingService, BillingService>()
        .AddTransient<IDataStore, FileDataStore>((sp) => new FileDataStore("googledatastore", false))
        .AddTransient<ICalendarManager, CalendarManager>()
        .AddTransient<BlogSpotService>()
        .AddTransient<ValidatingClientStore<ClientStore>>();

        // TODO for SMS: services.AddTransient<ISmsSender, AuthMessageSender>();

        _ = services.AddLocalization(options =>
        {
            options.ResourcesPath = "Resources";
        });
        var dataDirConfig = builder.Configuration["Site:DataDir"] ?? "DataDir";

        var dataDir = new DirectoryInfo(dataDirConfig);
        // Add session related services.

        services.AddDataProtection().PersistKeysToFileSystem(dataDir);
        AddYavscPolicies(services, builder.Configuration);

        services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        services.AddTransient<IExternalIdentityManager, ExternalIdentityManager>();


        services.AddAuthentication("Bearer")
                 .AddYavscJwtBearer(builder.Configuration,
                     configure: o => o.Audience = builder.Configuration.GetSection("Site")["ExternalUrl"]);

        services.AddTransient<RoleManager<IdentityRole>>();
        services.AddTransient<IRoleStore<IdentityRole>, RoleStore<IdentityRole, ApplicationDbContext>>();
        services.Configure<KycOptions>(builder.Configuration.GetSection("Kyc"));
        services.AddScoped<ITrustTokenService, TrustTokenService>();
        return builder.Build();
    }

    public static IdentityBuilder AddIdentityDBAndStores(this WebApplicationBuilder builder)
    {
        IServiceCollection services = builder.Services;
        var connectionString = builder.Configuration.GetConnectionString(YavscConstants.YavscConnectionStringName);
       
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (UsesInMemoryProvider(connectionString))
            {
                options.UseInMemoryDatabase(connectionString);
            }
            else
            {
                options.UseNpgsql(connectionString,
                    options => options.MigrationsAssembly(typeof(Program).Assembly));
            }
        });

        var identityBuilder = services.AddIdentity<ApplicationUser, IdentityRole>(
            options =>
            {
                options.SignIn.RequireConfirmedAccount = builder.Environment.IsEnvironment(
                    builder.Environment.EnvironmentName);
                options.ClaimsIdentity.UserNameClaimType = JwtClaimTypes.PreferredUserName;
                options.ClaimsIdentity.RoleClaimType = YavscConstants.RoleClaimType;
            }
        )
        .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>>();

        // Dev-only: Chromium rejects SameSite=None without Secure on http://
        // (e.g. http://localhost:5000). The default Identity cookie policy
        // sets SameSite=None, which is invalid without Secure. Force Lax in
        // dev. In production (https://) the default SameSite=None is fine.
        if (builder.Environment.IsDevelopment())
        {
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });
            services.ConfigureExternalCookie(options =>
            {
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });
        }

        return identityBuilder;
    }

    private static void AddYavscPolicies(IServiceCollection services, IConfiguration configuration)
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
                    .RequireClaim(YavscConstants.RoleClaimType,
                    new string[] { YavscConstants.PerformerGroupName, YavscConstants.AdminGroupName })
                    ;
            });
            options.AddPolicy("AdministratorOnly", policy =>
            {
                _ = policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(YavscConstants.RoleClaimType, YavscConstants.AdminGroupName);
            });

            options.AddPolicy("FrontOffice", policy => policy.RequireRole(YavscConstants.FrontOfficeGroupName));

            // options.AddPolicy("EmployeeId", policy => policy.RequireClaim("EmployeeId", "123", "456"));
            // options.AddPolicy("BuildingEntry", policy => policy.Requirements.Add(new OfficeEntryRequirement()));
            options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
            options.AddPolicy("TheAuthor", policy => policy.Requirements.Add(new EditPermission()));
        });

        services.AddYavscCors(configuration);
    }

    public static IServiceCollection LoadConfiguration(this WebApplicationBuilder builder)
    {
        var siteSection = builder.Configuration.GetSection("Site");

        var smtpSection = builder.Configuration.GetSection("Smtp");
        var paypalSection = builder.Configuration.GetSection("Authentication:PayPal");
        // OAuth2AppSettings
        var googleAuthSettings = builder.Configuration.GetSection("Authentication:Google");

        LoadGoogleConfig(builder.Configuration);


        var services = builder.Services;
        _ = services.AddControllersWithViews()
            .AddNewtonsoftJson();

        services.Configure<SiteSettings>(siteSection);
        services.Configure<SmtpSettings>(smtpSection);
        services.Configure<PayPalSettings>(paypalSection);
        services.Configure<GoogleAuthSettings>(googleAuthSettings);
        ConfigureRequestLocalization(services);

        return services;
    }

    private static void AddAuthentication(WebApplicationBuilder builder)
    {
        IServiceCollection services = builder.Services;
        IConfigurationRoot configurationRoot = builder.Configuration;
        string? googleClientId = configurationRoot["Authentication:Google:ClientId"];
        string? googleClientSecret = configurationRoot["Authentication:Google:ClientSecret"];

        var authenticationBuilder = services.AddAuthentication();

        if (googleClientId != null && googleClientSecret != null)
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
    public static IIdentityServerBuilder AddIdentityServer(WebApplicationBuilder builder)
    {
        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.ClaimsIdentity.UserIdClaimType = JwtClaimTypes.Subject;
            options.ClaimsIdentity.UserNameClaimType = JwtClaimTypes.Name;
            options.ClaimsIdentity.RoleClaimType = YavscConstants.RoleClaimType;
        });
        var migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;
        var connectionString = builder.Configuration.GetConnectionString(YavscConstants.YavscConnectionStringName);
        
        string sqliteConnectionString = $"Data Source={Path.Combine(Path.GetTempPath(), "yavsc_test.db")}";

        var identityServerBuilder = builder.Services.AddIdentityServer(options =>
         {
             options.Events.RaiseErrorEvents = true;
             options.Events.RaiseInformationEvents = true;
             options.Events.RaiseFailureEvents = true;
             options.Events.RaiseSuccessEvents = true;

             // see https://IdentityServer8.readthedocs.io/en/latest/topics/resources.html
             options.EmitStaticAudienceClaim = true;

         })
            .AddAspNetIdentity<ApplicationUser>()
            .AddClientStore<ClientStore>()
            .AddClientConfigurationValidator<DefaultClientConfigurationValidator>()
            .AddCorsPolicyService<CorsPolicyService>()
            .AddResourceStore<ResourceStore>()
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = b =>
                {
                    if (UsesInMemoryProvider(connectionString))
                    {
                        b.UseInMemoryDatabase(connectionString);
                    }
                    else
                    {
                        b.UseNpgsql(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                    }

                    b.UseSeeding(EnsureDefaultConfiguration());
                };
            })
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = b =>
                {
                    if (UsesInMemoryProvider(connectionString))
                    {
                        b.UseInMemoryDatabase(connectionString);
                    }
                    else
                    {
                        b.UseNpgsql(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                    }
                };
                
            });

        if (builder.Environment.IsDevelopment())
        {
            identityServerBuilder.AddDeveloperSigningCredential();
        }
        return identityServerBuilder;
    }

    private static bool UsesInMemoryProvider(string connectionString)
    {
        return string.Equals(connectionString, InMemoryProviderName, StringComparison.OrdinalIgnoreCase);
    }

    private static Action<DbContext, bool> EnsureDefaultApplicationScopes()
    {
        return (context, _) =>
        {
            foreach (String scope in Constants.BuildInApiScopes)
            {
                var existentScope = context.Set<ApiScope>().FirstOrDefault(b => b.Name == scope);
                if (existentScope == null)
                {
                    context.Set<ApiScope>().Add(new ApiScope { Name = scope });
                    context.SaveChanges();
                }
            }
        };
    }

    private const string PostItClientId = "postit";

    private static readonly string[] PostItRedirectUris = new[]
    {
        // Loopback URI for desktop / browser-based PKCE flows.
        "http://127.0.0.1:7890/",
        // Custom-scheme URI for Android. The matching IntentFilter must be
        // declared in PostIt.Android/Properties/AndroidManifest.xml.
        "android://postit-signin",
    };

    private static readonly string[] PostItGrantTypes = new[]
    {
        "authorization_code",
        "client_credentials",
    };

    private static readonly string[] PostItScopes = new[]
    {
        "blog",
        IdentityServer8.IdentityServerConstants.StandardScopes.OpenId,
        IdentityServer8.IdentityServerConstants.StandardScopes.Profile,
    };

    private static Action<DbContext, bool> EnsureDefaultConfiguration()
    {
        return (context, _) =>
        {
            EnsureDefaultApplicationScopes()(context, _);

            var clients = context.Set<IdentityServer8.EntityFramework.Entities.Client>();
            var existingClient = clients.FirstOrDefault(c => c.ClientId == PostItClientId);

            if (existingClient is null)
            {
                SeedNewPostItClient(context);
                return;
            }

            MigratePostItClientToPublic(context, existingClient);
        };
    }

    /// <summary>
    /// Insert a brand new <c>postit</c> client configured as a public OIDC
    /// client using Authorization Code + PKCE. Used the first time the
    /// ConfigurationDb is seeded.
    /// </summary>
    private static void SeedNewPostItClient(DbContext context)
    {
        // PostIt is a public client (Authorization Code + PKCE).
        // No client secret is stored or transmitted; PKCE binds the
        // authorization code to the requesting device.
        var client = new IdentityServer8.EntityFramework.Entities.Client
        {
            ClientId = PostItClientId,
            Enabled = true,
            RequireClientSecret = false,
            RequirePkce = true,
            ProtocolType = "oidc",
            RequireConsent = false,
        };

        context.Set<Client>().Add(client);

        foreach (var grantType in PostItGrantTypes)
        {
            context.Set<ClientGrantType>().Add(new IdentityServer8.EntityFramework.Entities.ClientGrantType
            {
                Client = client,
                GrantType = grantType
            });
        }

        foreach (var scope in PostItScopes)
        {
            context.Set<ClientScope>().Add(new IdentityServer8.EntityFramework.Entities.ClientScope
            {
                Client = client,
                Scope = scope
            });
        }

        foreach (var redirectUri in PostItRedirectUris)
        {
            context.Set<ClientRedirectUri>().Add(new IdentityServer8.EntityFramework.Entities.ClientRedirectUri
            {
                Client = client,
                RedirectUri = redirectUri
            });
        }

        // No ClientSecret row: PKCE-only clients don't need one.
        context.SaveChanges();
    }

    /// <summary>
    /// Bring an existing <c>postit</c> client up to the current public-client
    /// configuration. Idempotent: each change is applied only when the row is
    /// currently in the legacy state.
    /// </summary>
    private static void MigratePostItClientToPublic(
        DbContext context,
        IdentityServer8.EntityFramework.Entities.Client client)
    {
        var changed = false;

        // 1. Drop the client secret. PKCE-only clients must not have one.
        var secrets = context.Set<ClientSecret>().Where(s => s.Client.Id == client.Id);
        if (secrets.Any())
        {
            context.Set<ClientSecret>().RemoveRange(secrets);
            changed = true;
        }

        // 2. Flip the security flags.
        if (client.RequireClientSecret)
        {
            client.RequireClientSecret = false;
            changed = true;
        }
        if (!client.RequirePkce)
        {
            client.RequirePkce = true;
            changed = true;
        }

        // 3. Ensure all expected grant types are present (don't remove extras
        //    that may have been added by hand).
        var existingGrantTypes = context.Set<ClientGrantType>()
            .Where(g => g.Client.Id == client.Id)
            .Select(g => g.GrantType)
            .ToHashSet();
        foreach (var grantType in PostItGrantTypes)
        {
            if (!existingGrantTypes.Contains(grantType))
            {
                context.Set<ClientGrantType>().Add(new IdentityServer8.EntityFramework.Entities.ClientGrantType
                {
                    Client = client,
                    GrantType = grantType
                });
                changed = true;
            }
        }

        // 4. Ensure all expected scopes are present.
        var existingScopes = context.Set<ClientScope>()
            .Where(s => s.Client.Id == client.Id)
            .Select(s => s.Scope)
            .ToHashSet();
        foreach (var scope in PostItScopes)
        {
            if (!existingScopes.Contains(scope))
            {
                context.Set<ClientScope>().Add(new IdentityServer8.EntityFramework.Entities.ClientScope
                {
                    Client = client,
                    Scope = scope
                });
                changed = true;
            }
        }

        // 5. Ensure all expected redirect URIs are present. Legacy entries
        //    pointing at the OP itself (e.g. https://yavsc.pschneider.fr/)
        //    are removed — they redirect back into IdentityServer's own home
        //    page and create a login loop.
        var existingRedirects = context.Set<ClientRedirectUri>()
            .Where(r => r.Client.Id == client.Id)
            .ToList();
        var existingRedirectUris = existingRedirects
            .Select(r => r.RedirectUri)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var redirectUri in PostItRedirectUris)
        {
            if (!existingRedirectUris.Contains(redirectUri))
            {
                context.Set<ClientRedirectUri>().Add(new IdentityServer8.EntityFramework.Entities.ClientRedirectUri
                {
                    Client = client,
                    RedirectUri = redirectUri
                });
                changed = true;
            }
        }

        var legacyRedirects = existingRedirects
            .Where(r => r.RedirectUri == "https://yavsc.pschneider.fr/"
                     || r.RedirectUri == "yavsc://callback")
            .ToList();
        if (legacyRedirects.Count > 0)
        {
            context.Set<ClientRedirectUri>().RemoveRange(legacyRedirects);
            changed = true;
        }

        if (changed)
        {
            context.SaveChanges();
        }
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


    public async static Task<WebApplication> ConfigurePipeline(this WebApplication app)
    {
        ILoggerFactory loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<Program>();

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        JwtSecurityTokenHandler.DefaultMapInboundClaims = true;
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.MigrateDatabase();
        }

        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/robots.txt"))
            {
                var robotsTxtPath = System.IO.Path.Combine(app.Environment.WebRootPath, $"robots.txt");
                string output = "User-agent: *  \nDisallow: /";
                if (File.Exists(robotsTxtPath))
                {
                    output = await File.ReadAllTextAsync(robotsTxtPath);
                }
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync(output);
            }
            else await next();
        });

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();
        app.UseCors("default");
        app.MapStaticAssets();
        app.MapDefaultControllerRoute();
        //app.MapRazorPages();
        app.MapHub<ChatHub>("/chatHub");

        WorkflowHelpers.ConfigureBillingService();

        var services = app.Services;
        var siteSettings = services.GetRequiredService<IOptions<SiteSettings>>();
        var smtpSettings = services.GetRequiredService<IOptions<SmtpSettings>>();
        var payPalSettings = services.GetRequiredService<IOptions<PayPalSettings>>();
        var googleAuthSettings = services.GetRequiredService<IOptions<GoogleAuthSettings>>();
        var localization = services.GetRequiredService<IStringLocalizer<Startup>>();
        Startup.Configure(app, siteSettings, smtpSettings,
            payPalSettings, googleAuthSettings, localization, loggerFactory,
            app.Environment.EnvironmentName);
        app.ConfigureFileServerApp();
        app.UseSession();
        return app;
    }

    private static void MigrateDatabase(this IApplicationBuilder app)
    {
        using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {

            try
            {
                foreach (Type contextType in new Type[]
                {
                    typeof(ApplicationDbContext)
                })
                {
                    ((DbContext)serviceScope.ServiceProvider
                    .GetRequiredService(contextType))
                    .Database.Migrate();
                }
            }
            catch (InvalidOperationException ex)
            {
                app.Properties["DegradedDBContext"] = ex.Message;
            }
        }
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

    public static IApplicationBuilder ConfigureFileServerApp(this IApplicationBuilder app,
                bool enableDirectoryBrowsing = false)
    {

        var userFilesDirInfo = new DirectoryInfo(Config.SiteSetup.Blog);
        AbstractFileSystemHelpers.UserFilesDirName = userFilesDirInfo.FullName;

        if (!userFilesDirInfo.Exists) userFilesDirInfo.Create();

        Config.UserFilesOptions = new FileServerOptions()
        {
            FileProvider = new PhysicalFileProvider(AbstractFileSystemHelpers.UserFilesDirName),
            RequestPath = PathString.FromUriComponent(YavscConstants.UserFilesPath),
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
            RequestPath = PathString.FromUriComponent(YavscConstants.AvatarsPath),
            EnableDirectoryBrowsing = enableDirectoryBrowsing
        };


        var gitdirinfo = new DirectoryInfo(Config.SiteSetup.GitRepository);
        Config.GitDirName = gitdirinfo.FullName;
        if (!gitdirinfo.Exists) gitdirinfo.Create();
        Config.GitOptions = new FileServerOptions()
        {
            FileProvider = new PhysicalFileProvider(Config.GitDirName),
            RequestPath = PathString.FromUriComponent(YavscConstants.GitPath),
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


}
