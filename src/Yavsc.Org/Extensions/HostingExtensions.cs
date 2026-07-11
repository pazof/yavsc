using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
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
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Security;
using Microsoft.Extensions.Localization;
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
using IdentityServer8.Models;
using IdentityServer8.EntityFramework.Mappers;
using Yavsc.Server.Hubs;

namespace Yavsc.Extensions;


public static class HostingExtensions
{
    private const string InMemoryProviderName = "InMemory";

    private static void IgnoreKnownFalsePositiveMigrationWarnings(DbContextOptionsBuilder options)
    {
        options.ConfigureWarnings(w =>
            w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    private static async Task ApplyMigrationsAsync<TContext>(IServiceProvider services, ILogger logger)
        where TContext : DbContext
    {
        var contextName = typeof(TContext).Name;
        var db = services.GetRequiredService<TContext>();

        logger.LogInformation(
            "Applying database migrations for {DbContext} using provider {Provider}...",
            contextName,
            db.Database.ProviderName ?? "(null)");

        await db.Database.MigrateAsync();

        logger.LogInformation(
            "Database migrations applied successfully for {DbContext}.",
            contextName);
    }

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

        services.TryAddSingleton<ISmtpClientFactory, SmtpClientFactory>();


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

                // EF Core 10 can raise PendingModelChangesWarning at runtime
                // even when the snapshot and generated migrations are already
                // aligned on this codebase. Treat that known false positive as
                // non-fatal in every environment so production startup matches
                // the behavior already observed in development.
                IgnoreKnownFalsePositiveMigrationWarnings(options);
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
             options.UserInteraction.LoginUrl = "/signin";

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
                        IgnoreKnownFalsePositiveMigrationWarnings(b);
                    }

                    // NOTE: don't b.UseSeeding(...) here — EF Core's UseSeeding
                    // only runs when the database is empty, so on a live
                    // configuration store (clients/scopes already present)
                    // it never fires and missing scopes are never inserted.
                    // We call the seeder explicitly from MigrateDatabase after
                    // migrations, so it runs on every startup regardless of
                    // whether the database is fresh.
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
                        IgnoreKnownFalsePositiveMigrationWarnings(b);
                    }
                };

            });

        // Skip the production signing-cert requirement when running with
        // an in-memory database (test fixtures) or in the Development
        // environment. In those cases IdentityServer8 falls back to
        // AddDeveloperSigningCredential which mints an ephemeral key
        // at startup; signing real tokens against it would fail, but
        // the test fixtures only use the discovery/JWKS endpoints.
        var useDevSigning = builder.Environment.IsDevelopment()
            || UsesInMemoryProvider(connectionString);
        if (useDevSigning)
        {
            identityServerBuilder.AddDeveloperSigningCredential();
        }
        else
        {
            // Production: reuse the Let's Encrypt certificate that Kestrel
            // already loads for TLS so IdentityServer has a stable signing
            // key (and a JWKS endpoint). The cert is renewed by the ACME
            // hook and a service restart picks up the new key automatically.
            //
            // The path comes from Kestrel:Endpoints:Https:Certificate to
            // avoid maintaining a separate setting; fullchain.pem bundles
            // the leaf + chain, which X509Certificate2 needs for chain
            // validation by relying parties.
            var certPath = builder.Configuration["Kestrel:Endpoints:Https:Certificate:Path"];
            var keyPath = builder.Configuration["Kestrel:Endpoints:Https:Certificate:KeyPath"];
            if (string.IsNullOrWhiteSpace(certPath) || string.IsNullOrWhiteSpace(keyPath))
            {
                throw new InvalidOperationException(
                    "Production IdentityServer requires a signing certificate. " +
                    "Configure Kestrel:Endpoints:Https:Certificate:{Path,KeyPath}.");
            }
            // Load the leaf cert and extract its private key for signing.
            // The previous attempts (X509Certificate2.CreateFromPemFile,
            // the 3-arg ctor with X509KeyStorageFlags, and BC + CopyWithPrivateKey)
            // all loaded the cert successfully, but CreateJwkDocumentAsync
            // still raised NullReferenceException on the first GET /jwks
            // request: IdentityServer8's key material service reads the
            // private key off the X509Certificate2 at runtime, and on Linux
            // the key handle is not retained across that boundary.
            //
            // The reliable pattern is to pass a SigningCredentials that
            // wraps a SecurityKey built directly from the BC-parsed key
            // parameters. The SecurityKey is a managed object whose Key
            // property is a live AsymmetricAlgorithm, which survives every
            // read IdentityServer does (token signing, JWKS publish).
            var signingCredentials = LoadSigningCredentials(certPath, keyPath);
            identityServerBuilder.AddSigningCredential(signingCredentials);
        }

        // Note: IdentityServer8 does NOT expose the JWKS at /.well-known/jwks.
        // The default jwks_uri is /.well-known/openid-configuration/jwks,
        // which is what DiscoveryKeyEndpoint serves. Earlier revisions of
        // this file tried to override CustomEntries["jwks_uri"], but
        // IdentityServer8 reserves that key and rejects the override with
        // "Discovery custom entry jwks_uri cannot be added, because it
        // already exists." The default endpoint works once the signing
        // credential's private key is attached (see above).
        return identityServerBuilder;
    }

    /// <summary>
    /// Load the signing credentials (algorithm + private key) from the
    /// configured PEM files. Returns a <see cref="SigningCredentials"/>
    /// whose <c>Key</c> is a managed <see cref="SecurityKey"/> built
    /// directly from the BouncyCastle-parsed key parameters — which keeps
    /// the private key alive for every read IdentityServer8 does (token
    /// signing, JWKS publish), unlike <c>X509Certificate2.CopyWithPrivateKey</c>
    /// which loses the handle on Linux when IdentityServer8's key material
    /// service reads it back at runtime.
    /// </summary>
    private static SigningCredentials LoadSigningCredentials(string certPath, string keyPath)
    {
        // Pre-flight read so permission / missing-file errors surface with
        // the actual path instead of being wrapped as an opaque
        // InvalidOperationException by the cert / key parsers.
        try
        {
            return LoadSigningCredentialsInner(certPath, keyPath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"[yavsc] Failed to load signing credentials from {certPath} / {keyPath}:");
            Console.Error.WriteLine(ex.ToString());
            throw new InvalidOperationException(
                $"Failed to load signing credentials from {certPath} / {keyPath}. " +
                "See stderr for the underlying managed exception (likely a " +
                "PEM format mismatch between the cert and private key).",
                ex);
        }
    }

    private static SigningCredentials LoadSigningCredentialsInner(string certPath, string keyPath)
    {
        // Validate the cert is readable (used downstream for token
        // audience/subject validation; signing itself uses the key).

        // Derive a stable KeyId from the certificate's SHA-1
        // thumbprint (the default for X509Certificate2.GetCertHash()).
        // Without an explicit KeyId, IdentityServer emits JWTs without
        // a 'kid' header and the JWKS without per-key identifiers,
        // which breaks signature validation on resource servers (they
        // cannot match a token to a key in the JWKS, they fail with
        // IDX10500 "The signature key was not found"). Truncating the
        // 40-hex-char SHA-1 to 16 hex chars is enough to be globally
        // unique within a deployment and keeps the JWT header compact.
        // The thumbprint changes on cert renewal, which is the desired
        // behaviour: old tokens age out, resource servers refresh
        // their JWKS cache for the new kid.
        var kid = ComputeKid(certPath);

        string keyPem = File.ReadAllText(keyPath);

        // BouncyCastle's PemReader accepts every flavour of unencrypted
        // private key PEM that ACME clients produce (PKCS#1 with
        // BEGIN EC/RSA PRIVATE KEY, PKCS#8 with BEGIN PRIVATE KEY, both
        // EC and RSA), and returns the right AsymmetricKeyParameter
        // subtype without the SIGABRTs we saw when forcing the
        // System.Security.Cryptography path on the production EC Let's
        // Encrypt cert.
        using var sr = new StringReader(keyPem);
        var pemReader = new PemReader(sr);
        var keyObj = pemReader.ReadObject()
            ?? throw new InvalidOperationException(
                $"PEM reader returned null for {keyPath}");

        AsymmetricKeyParameter bcKey = keyObj switch
        {
            AsymmetricCipherKeyPair pair => pair.Private,
            AsymmetricKeyParameter param => param,
            _ => throw new InvalidOperationException(
                $"Unexpected PEM object type '{keyObj.GetType().FullName}' " +
                $"in {keyPath}; expected a private key."),
        };

        // Build the SecurityKey + SigningCredentials. RsaSecurityKey /
        // ECDsaSecurityKey wrap managed AsymmetricAlgorithm objects whose
        // Key is the live private key — IdentityServer8 can call Sign on
        // these repeatedly without losing the key handle.
        switch (bcKey)
        {
            case RsaPrivateCrtKeyParameters rsa:
                {
#pragma warning disable CA1416 // Valider la compatibilité de la plateforme
                    var rsaDotNet = DotNetUtilities.ToRSA(rsa);
#pragma warning restore CA1416 // Valider la compatibilité de la plateforme
                    var key = new RsaSecurityKey(rsaDotNet) { KeyId = kid };
                    return new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
                }
            case ECPrivateKeyParameters ec:
                {
                    var ecParams = new ECParameters
                    {
                        Curve = LoadEcCurve(ec.Parameters),
                        D = ec.D.ToByteArrayUnsigned(),
                    };
                    var ecdsa = ECDsa.Create();
                    ecdsa.ImportParameters(ecParams);
                    var key = new ECDsaSecurityKey(ecdsa) { KeyId = kid };
                    return new SigningCredentials(key, SecurityAlgorithms.EcdsaSha256);
                }
            default:
                throw new InvalidOperationException(
                    $"Unsupported private key algorithm '{bcKey.GetType().Name}' " +
                    $"in {keyPath}; expected RSA or EC.");
        }
    }

    /// <summary>
    /// Derive the <c>kid</c> used to identify the signing key in the
    /// JWT header and the JWKS. Takes the first 16 hex characters of
    /// the certificate's SHA-1 thumbprint. See the inline rationale in
    /// <see cref="LoadSigningCredentialsInner"/> for why this is
    /// needed (IdentityServer8 + IDX10500).
    /// </summary>
    /// <remarks>
    /// Internal so unit tests in <c>Yavsc.Org.Tests</c> can exercise
    /// the truncation/encoding without going through the full PEM /
    /// BouncyCastle pipeline. The input is a path rather than a
    /// pre-loaded <see cref="X509Certificate2"/> to match the
    /// production call site.
    /// </remarks>
    internal static string ComputeKid(string certPath)
    {
        // X509CertificateLoader is the .NET 9+ replacement for the
        // obsolete `new X509Certificate2(string)` ctor (SYSLIB0057).
        // Same on-disk format (PEM or DER), same thumbprint, just
        // doesn't trip the obsolete-API warning at build time.
        var certForKid = X509CertificateLoader.LoadCertificateFromFile(certPath);
        var certHash = certForKid.GetCertHash();
        // GetCertHash() returns a SHA-1 thumbprint (20 bytes, 40 hex
        // chars). Truncating to 16 hex chars keeps the JWT header
        // compact; Math.Min guards against an unexpected short hash.
        return Convert.ToHexString(certHash)[..Math.Min(16, certHash.Length * 2)];
    }

    /// <summary>
    /// Map a BouncyCastle <see cref="ECDomainParameters"/> to a
    /// <see cref="ECCurve"/> that <see cref="ECDsa.ImportParameters"/>
    /// understands. Handles the curves Let's Encrypt issues (P-256,
    /// P-384, P-521); other curves throw.
    /// </summary>
    private static ECCurve LoadEcCurve(ECDomainParameters bcCurve)
    {
        // bcCurve.N is the order of the generator; its bit length is the
        // canonical fingerprint for NIST curves (256, 384, 521 bits).
        var orderBits = bcCurve.N.BitLength;
        return orderBits switch
        {
            256 => ECCurve.NamedCurves.nistP256,
            384 => ECCurve.NamedCurves.nistP384,
            521 => ECCurve.NamedCurves.nistP521,
            _ => throw new InvalidOperationException(
                $"Unsupported EC curve with order bit length {orderBits}; " +
                "expected P-256, P-384 or P-521."),
        };
    }

    private static bool UsesInMemoryProvider(string connectionString)
    {
        return string.Equals(connectionString, InMemoryProviderName, StringComparison.OrdinalIgnoreCase);
    }

    private static Action<DbContext, bool> EnsureDefaultApplicationScopes()
    {
        return (context, _) =>
        {
            foreach (String scope in Org.Constants.BuildInApiScopes)
            {
                var existentScope = context.Set<IdentityServer8.EntityFramework.Entities.ApiScope>().FirstOrDefault(b => b.Name == scope);
                if (existentScope == null)
                {
                    context.Set<IdentityServer8.EntityFramework.Entities.ApiScope>().Add(new IdentityServer8.EntityFramework.Entities.ApiScope { Name = scope });
                    context.SaveChanges();
                }
            }
            var identityResources = context.Set<IdentityServer8.EntityFramework.Entities.IdentityResource>();
            var apiScopes = context.Set<IdentityServer8.EntityFramework.Entities.ApiScope>();

            // IdentityResources standards (OpenId + Profile only).
            // Application-defined API scopes from Constants.ApiResourcesScopes
            // are NOT identity resources — they belong to the ApiScopes table
            // and are seeded as such further down.
            if (!identityResources.Any(r => r.Name == "openid"))
            {
                var openid = new IdentityResources.OpenId().ToEntity();
                identityResources.Add(openid);
            }

            if (!identityResources.Any(r => r.Name == "profile"))
            {
                var profile = new IdentityResources.Profile().ToEntity();
                identityResources.Add(profile);
            }

            // Application-defined API scopes (admin, moderation, performer,
            // client, blogs, …). Seeded into ApiScopes, not IdentityResources:
            // these gate access to API resources (e.g. the Yavsc.Blogs
            // deployment requires the "blogs" scope) and must therefore be
            // discoverable through /connect/discovery's
            // scopes_supported of type resource, not identity.
            //
            // NOTE: prior versions inserted these into IdentityResources,
            // which made them visible to /connect/authorize but unfulfillable
            // (no API resource recognises an identity-scoped consent as
            // access to a downstream resource). Clients like PostIt that
            // request one of these scopes were rejected with "invalid_scope"
            // at the token endpoint. Keep this in ApiScopes.
            foreach (var scopeSpec in Org.Constants.ApiResourcesScopes)
            {
                if (!apiScopes.Any(s => s.Name == scopeSpec.ScopeName))
                {
                    apiScopes.Add(new IdentityServer8.EntityFramework.Entities.ApiScope
                    {
                        Name = scopeSpec.ScopeName,
                        DisplayName = scopeSpec.Description,
                        // The corresponding Postgres columns are NOT NULL
                        // with no DB default — EF Core ships the C# default
                        // (false) unless we set them explicitly. A scope
                        // inserted with Enabled=false is invisible to
                        // DefaultResourceValidator, which would silently
                        // reproduce the bug we're fixing here.
                        Enabled = true,
                        Required = false,
                        Emphasize = false,
                        ShowInDiscoveryDocument = true,
                    });
                }
            }

            // ApiResources — one per application scope, linked to its scope
            // via ApiResourceScopes. IdentityServer8's DefaultResourceValidator
            // rejects any scope that isn't backed by an ApiResource at
            // /connect/authorize time ("Scope X not found in store"), even
            // when the ApiScope row itself exists. Seeding the scope without
            // the resource is what caused the PostIt login to die in the
            // first place.
            //
            // The mapping is taken from Constants.ApiResourcesScopes
            // (ScopeName ↔ ResourceName). We dedupe on resource name so a
            // future spec that re-uses an existing resource doesn't insert
            // duplicates.
            var apiResources = context.Set<IdentityServer8.EntityFramework.Entities.ApiResource>();
            var apiResourceScopes = context.Set<IdentityServer8.EntityFramework.Entities.ApiResourceScope>();

            // Make sure every resource row referenced by the spec exists.
            foreach (var resourceGroup in Org.Constants.ApiResourcesScopes
                         .GroupBy(s => s.ResourceName))
            {
                var spec = resourceGroup.First();
                if (!apiResources.Any(r => r.Name == spec.ResourceName))
                {
                    apiResources.Add(new IdentityServer8.EntityFramework.Entities.ApiResource
                    {
                        Name = spec.ResourceName,
                        DisplayName = spec.ResourceDisplayName,
                        Enabled = true,
                        // Created is NOT NULL with no DB default. Without
                        // this EF will send DateTime.MinValue (0001-01-01)
                        // which Postgres rejects with
                        // "null value in column 'Created' violates
                        // not-null constraint" once the seeder runs.
                        Created = DateTime.UtcNow,
                        ShowInDiscoveryDocument = true,
                        NonEditable = false,
                    });
                }
            }
            context.SaveChanges();

            // Link each scope to its resource. We re-query both sets after
            // the SaveChanges above so the newly inserted resources have
            // their generated Ids.
            //
            // Note: Constants.ApiResourcesScopes is a static readonly array
            // (not IQueryable), so we have to materialise the names into a
            // local list before letting EF try to translate the Where into
            // SQL — otherwise EF throws "The LINQ expression … could not be
            // translated" at runtime.
            var wantedResourceNames = Org.Constants.ApiResourcesScopes
                .Select(s => s.ResourceName)
                .ToHashSet();

            var resourceByName = apiResources
                .Where(r => wantedResourceNames.Contains(r.Name))
                .ToDictionary(r => r.Name);

            foreach (var scopeSpec in Org.Constants.ApiResourcesScopes)
            {
                if (!resourceByName.TryGetValue(scopeSpec.ResourceName, out var resource))
                    continue;

                bool alreadyLinked = apiResourceScopes.Any(link =>
                    link.ApiResourceId == resource.Id && link.Scope == scopeSpec.ScopeName);

                if (alreadyLinked)
                    continue;

                apiResourceScopes.Add(new ApiResourceScope
                {
                    ApiResource = resource,
                    ApiResourceId = resource.Id,
                    Scope = scopeSpec.ScopeName,
                });
            }
            context.SaveChanges();
        };
    }

    private const string PostItClientId = "postit";

    private static readonly string[] PostItRedirectUris = new[]
    {
        // Loopback URI for desktop / browser-based PKCE flows.
        "postit://callback",
        "android://postit-signin",
        "https://blogs.pschneider.fr"
    };

    private static readonly string[] PostItGrantTypes = new[]
    {
        "authorization_code",
        "client_credentials",
    };

    private static readonly string[] PostItScopes = new[]
    {
        // Scopes the PostIt client is allowed to ask for. Must match
        // what postit-settings.json (and Constants.BuildInApiScopes on
        // the server) actually defines. Notably:
        // - "blogs" (plural) is the API scope that gates access to the
        //   Yavsc.Blogs deployment at https://blogs.pschneider.fr.
        // - "offline_access" is required for the YavscApiClient's
        //   silent refresh path to work; without it IdentityServer
        //   refuses to issue a refresh_token.
        "blogs",
        IdentityServer8.IdentityServerConstants.StandardScopes.OpenId,
        IdentityServer8.IdentityServerConstants.StandardScopes.Profile,
        IdentityServer8.IdentityServerConstants.StandardScopes.OfflineAccess,
    };

    private static Action<DbContext, bool> EnsureDefaultConfiguration(
        IConfiguration configuration
    )
    {
        return (context, _) =>
        {
            EnsureDefaultApplicationScopes()(context, _);

            var clients = context.Set<IdentityServer8.EntityFramework.Entities.Client>();
            var existingClient = clients.FirstOrDefault(c => c.ClientId == PostItClientId);

            if (existingClient is null)
            {
                SeedNewPostItClient(configuration, context);
                return;
            }

            // The PostIt client was already seeded in a previous run.
            // Make sure its AllowedScopes still match what the server
            // actually exposes — for instance, "blogs" only exists as an
            // ApiScope since we fixed the seed (see EnsureDefaultApplicationScopes
            // above). Without this pass, a client created before the fix
            // would still ask for a scope the server no longer recognises
            // and IdentityServer would answer "invalid_scope" at the token
            // endpoint. Idempotent: missing scopes are added, nothing is
            // removed (manual revocation stays manual).
            AlignPostItClientScopes(context, existingClient);
        };
    }

    /// <summary>
    /// Insert a brand new <c>postit</c> client configured as a public OIDC
    /// client using Authorization Code + PKCE. Used the first time the
    /// ConfigurationDb is seeded.
    /// </summary>
    private static void SeedNewPostItClient(IConfiguration configuration, DbContext context)
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

        context.Set<IdentityServer8.EntityFramework.Entities.Client>().Add(client);

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

        foreach (var redirectUri in BuildPostItRedirectUris(configuration))
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
    /// Idempotent reconciliation of the PostIt client's AllowedScopes
    /// against the scopes the server actually publishes. Adds any missing
    /// scope as a ClientScope row; never removes anything (revocation is a
    /// manual operation, not a seed concern). Called on every startup from
    /// <see cref="EnsureDefaultConfiguration"/> so a client created before
    /// a scope was introduced (or before the seed was corrected) catches up
    /// automatically.
    /// </summary>
    private static void AlignPostItClientScopes(
        DbContext context,
        IdentityServer8.EntityFramework.Entities.Client postitClient
    )
    {
        var clientScopes = context.Set<ClientScope>();
        var existingScopeNames = clientScopes
            .Where(s => s.Client == postitClient || s.ClientId == postitClient.Id)
            .Select(s => s.Scope)
            .ToHashSet();

        bool changed = false;
        foreach (var scope in PostItScopes)
        {
            if (existingScopeNames.Contains(scope))
                continue;

            clientScopes.Add(new IdentityServer8.EntityFramework.Entities.ClientScope
            {
                Client = postitClient,
                Scope = scope
            });
            changed = true;
        }

        if (changed)
        {
            context.SaveChanges();
        }
    }

    /// <summary>
    /// Compose the full set of redirect URIs for the PostIt client. The base
    /// URIs cover the standalone desktop/mobile flows; the value of
    /// <c>Site:ExternalUrl</c> is appended so PostIt can also be embedded in
    /// a Yavsc.Org web page (e.g. an iframe-launched launcher).
    /// </summary>
    private static IEnumerable<string> BuildPostItRedirectUris(IConfiguration configuration)
    {
        foreach (var uri in PostItRedirectUris)
            yield return uri;

        var externalUrl = configuration["Site:ExternalUrl"];
        if (!string.IsNullOrWhiteSpace(externalUrl))
            yield return externalUrl;
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


    public async static Task<WebApplication> ConfigurePipeline(this WebApplication app, string staticAssetsManifestPath = null)
    {
        ILoggerFactory loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<Program>();

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        JwtSecurityTokenHandler.DefaultMapInboundClaims = true;
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            await app.MigrateDatabaseAsync();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            logger.LogInformation("Running in production mode. Ensure the database is migrated.");
            await app.MigrateDatabaseAsync();
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
        app.MapDefaultControllerRoute();
        app.MapStaticAssets(staticAssetsManifestPath);
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

    private static async Task MigrateDatabaseAsync(this IApplicationBuilder app)
    {
        using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var logger = serviceScope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("Yavsc.Org.Migrations");

            try
            {
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    await ApplyMigrationsAsync<ApplicationDbContext>(scope.ServiceProvider, logger);
                }
                EnsureCriticalSchema(serviceScope.ServiceProvider, logger);
            }
            catch (Exception ex)
            {
                app.Properties["DegradedDBContext"] = ex.Message;
                logger.LogError(
                    ex,
                    "Database migration failed for {DbContext}. App started in degraded mode.",
                    nameof(ApplicationDbContext));

                // EF Core 10 may raise PendingModelChangesWarning as an exception.
                // Dump a concise model diff to make the mismatch actionable.
                if (ex is InvalidOperationException ioe
                    && ioe.Message.Contains("PendingModelChangesWarning", StringComparison.Ordinal))
                {
                    LogPendingModelChanges(serviceScope.ServiceProvider, logger);
                }
            }
        }

        // Run the IdentityServer configuration seeder explicitly, after
        // migrations. EF Core's UseSeeding callback only fires when the
        // database is empty — on a live ConfigurationDb that's been used
        // for months, the seeder never runs and missing scopes/resources
        // are never inserted. Calling EnsureDefaultConfiguration here makes
        // the seed idempotent across restarts.
        SeedConfigurationDatabase(app);
    }

    private static void LogPendingModelChanges(IServiceProvider services, ILogger logger)
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var migrationsAssembly = db.GetService<IMigrationsAssembly>();
            var snapshotModel = migrationsAssembly.ModelSnapshot?.Model;
            if (snapshotModel is null)
            {
                logger.LogWarning("Pending-model diagnostic: no ModelSnapshot found for ApplicationDbContext.");
                return;
            }

            logger.LogWarning(
                "Pending-model diagnostic: provider={Provider}",
                db.Database.ProviderName ?? "(null)");

            var runtimeDeclDate = db.Model
                .FindEntityType("Yavsc.Models.Identity.DeviceDeclaration")?
                .FindProperty("DeclarationDate")?
                .GetDefaultValueSql();

            var snapshotDeclDate = snapshotModel
                .FindEntityType("Yavsc.Models.Identity.DeviceDeclaration")?
                .FindProperty("DeclarationDate")?
                .GetDefaultValueSql();

            logger.LogWarning(
                "Pending-model diagnostic: DeviceDeclaration.DeclarationDate default SQL runtime='{RuntimeDefaultSql}', snapshot='{SnapshotDefaultSql}'.",
                runtimeDeclDate ?? "(null)",
                snapshotDeclDate ?? "(null)");

            bool runtimeHasMusicLoverSettings = db.Model.FindEntityType("Yavsc.Models.Musical.Profiles.MusicLoverSettings") is not null;
            bool snapshotHasMusicLoverSettings = snapshotModel.FindEntityType("Yavsc.Models.Musical.Profiles.MusicLoverSettings") is not null;
            logger.LogWarning(
                "Pending-model diagnostic: MusicLoverSettings runtime={RuntimeHas} snapshot={SnapshotHas}.",
                runtimeHasMusicLoverSettings,
                snapshotHasMusicLoverSettings);

            bool runtimeHasSignature = db.Model.FindEntityType("Yavsc.Models.Billing.Signature") is not null;
            bool snapshotHasSignature = snapshotModel.FindEntityType("Yavsc.Models.Billing.Signature") is not null;
            logger.LogWarning(
                "Pending-model diagnostic: Signature runtime={RuntimeHas} snapshot={SnapshotHas}.",
                runtimeHasSignature,
                snapshotHasSignature);

            bool runtimeHasModerated = db.Model
                .FindEntityType("Yavsc.Models.Workflow.Activity")?
                .FindProperty("Moderated") is not null;
            bool snapshotHasModerated = snapshotModel
                .FindEntityType("Yavsc.Models.Workflow.Activity")?
                .FindProperty("Moderated") is not null;
            logger.LogWarning(
                "Pending-model diagnostic: Activity.Moderated runtime={RuntimeHas} snapshot={SnapshotHas}.",
                runtimeHasModerated,
                snapshotHasModerated);
        }
        catch (Exception diagEx)
        {
            logger.LogError(diagEx, "Pending-model diagnostic failed.");
        }
    }

    private static void EnsureCriticalSchema(IServiceProvider services, ILogger logger)
    {
        try
        {
            var db = services.GetRequiredService<ApplicationDbContext>();

            // Hotfix guard: keep startup resilient if a migration was skipped,
            // while still allowing EF migrations to be the source of truth.
            db.Database.ExecuteSqlRaw(@"
ALTER TABLE ""Activities""
ADD COLUMN IF NOT EXISTS ""Moderated"" boolean NOT NULL DEFAULT FALSE;");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Critical schema check failed for Activities.Moderated.");
        }
    }

    private static void SeedConfigurationDatabase(IApplicationBuilder app)
    {
        try
        {
            using var scope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();

            var logger = app.ApplicationServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("Yavsc.Org.Seeding");

            var configurationDb = scope.ServiceProvider
                .GetRequiredService<IdentityServer8.EntityFramework.DbContexts.ConfigurationDbContext>();

            var configuration = scope.ServiceProvider
                .GetRequiredService<IConfiguration>();

            logger.LogInformation(
                "Running seed for {DbContext} using provider {Provider}...",
                nameof(IdentityServer8.EntityFramework.DbContexts.ConfigurationDbContext),
                configurationDb.Database.ProviderName ?? "(null)");

            EnsureDefaultConfiguration(configuration)(configurationDb, true);

            logger.LogInformation(
                "Seed completed for {DbContext}.",
                nameof(IdentityServer8.EntityFramework.DbContexts.ConfigurationDbContext));
        }
        catch (Exception ex)
        {
            // Seeding is best-effort: a missing scope row will just leave
            // the same login failure as before, no worse than today. Don't
            // crash the host over it. Log so the operator sees something.
            var logger = app.ApplicationServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("Yavsc.Org.Seeding");
            logger.LogError(
                ex,
                "Seed failed for {DbContext}.",
                nameof(IdentityServer8.EntityFramework.DbContexts.ConfigurationDbContext));
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
