using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor;
using Microsoft.Extensions.PlatformAbstractions;
using Yavsc;
using Yavsc.Models;
using Yavsc.Services;
using Microsoft.Data.Entity;
using Microsoft.Extensions.WebEncoders;
using yavscTests.Settings;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using Yavsc.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OAuth.AspNet.Tokens;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.OAuth;
using Yavsc.Helpers.Auth;
using Google.Apis.Util.Store;
using System.Security.Claims;
using Google.Apis.Auth.OAuth2.Responses;
using Constants = Yavsc.Constants;
using OAuth.AspNet.AuthServer;
using Yavsc.Models.Auth;
using Microsoft.AspNetCore.Identity;
using System.Collections.Concurrent;
using System.Security.Principal;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Yavsc.AuthorizationHandlers;
using Yavsc.Formatters;
using Microsoft.Net.Http.Headers;
using static Yavsc.Startup;
using Microsoft.AspNetCore.DataProtection.Infrastructure;
using System.IO;
using Microsoft.AspNetCore.Identity.EntityFramework;
using Yavsc.Auth;
using Yavsc.Lib;

namespace yavscTests
{
    public class Startup
    {

        public static IConfiguration Configuration { get; set; }

        public static string HostingFullName { get; private set; }

        public ApplicationDbContext DbContext { get; private set; }

        public static TestingSetup TestingSetup { get; private set; }

        public static IConfigurationRoot GoogleWebClientConfiguration { get; set; }

        public static CookieAuthenticationOptions ExternalCookieAppOptions { get; private set; }
        public MonoDataProtectionProvider ProtectionProvider { get; private set; }
        public static OAuth.AspNet.AuthServer.OAuthAuthorizationServerOptions OAuthServerAppOptions { get; private set; }
        public Yavsc.Auth.YavscGoogleOptions YavscGoogleAppOptions { get; private set; }
        private static ILogger logger;

        public static string ApiKey { get; private set; }


        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            var devtag = env.IsDevelopment() ? "D" : "";
            var prodtag = env.IsProduction() ? "P" : "";
            var stagetag = env.IsStaging() ? "S" : "";

            HostingFullName = $"{appEnv.RuntimeFramework.FullName} [{env.EnvironmentName}:{prodtag}{devtag}{stagetag}]";
            // Set up configuration sources.

            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }
            Configuration = builder.Build();

            var googleClientFile = Configuration["Authentication:Google:GoogleWebClientJson"];
            var googleServiceAccountJsonFile = Configuration["Authentication:Google:GoogleServiceAccountJson"];
            if (googleClientFile != null)
                GoogleWebClientConfiguration = new ConfigurationBuilder().AddJsonFile(googleClientFile).Build();

        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            var siteSettingsconf = Configuration.GetSection("Site");
            services.Configure<SiteSettings>(siteSettingsconf);
            var smtpSettingsconf = Configuration.GetSection("Smtp");
            services.Configure<SmtpSettings>(smtpSettingsconf);
            var dbSettingsconf = Configuration.GetSection("ConnectionStrings");
            services.Configure<DbConnectionSettings>(dbSettingsconf);
            var testingconf = Configuration.GetSection("Testing");
            services.Configure<TestingSetup>(testingconf);

            services.AddInstance(typeof(ILoggerFactory), new LoggerFactory());
            services.AddTransient(typeof(IEmailSender), typeof(MailSender));
            services.AddEntityFramework().AddNpgsql().AddDbContext<ApplicationDbContext>();
            services.AddTransient((s) => new RazorTemplateEngine(s.GetService<RazorEngineHost>()));
            services.AddLogging();
            services.AddTransient<ServerSideFixture>();
            services.AddTransient<MailSender>();
            services.AddTransient<EMailer>();
            
            services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });

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
                options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
            });

            services.AddDataProtection();
            services.ConfigureDataProtection(configure =>
            {
                configure.SetApplicationName(Configuration["Site:Title"]);
                configure.SetDefaultKeyLifetime(TimeSpan.FromDays(45));
                configure.PersistKeysToFileSystem(
                     new DirectoryInfo(Configuration["DataProtection:Keys:Dir"]));
            });

            services.Add(ServiceDescriptor.Singleton(typeof(IApplicationDiscriminator),
                typeof(SystemWebApplicationDiscriminator)));


            services.AddSingleton<IAuthorizationHandler, HasBadgeHandler>();
            services.AddSingleton<IAuthorizationHandler, HasTemporaryPassHandler>();
            services.AddSingleton<IAuthorizationHandler, BlogEditHandler>();
            services.AddSingleton<IAuthorizationHandler, BlogViewHandler>();
            services.AddSingleton<IAuthorizationHandler, BillEditHandler>();
            services.AddSingleton<IAuthorizationHandler, BillViewHandler>();
            services.AddSingleton<IAuthorizationHandler, PostUserFileHandler>();
            services.AddSingleton<IAuthorizationHandler, ViewFileHandler>();
            services.AddSingleton<IAuthorizationHandler, SendMessageHandler>();
            services.AddSingleton<IConnexionManager, HubConnectionManager>();
            services.AddSingleton<ILiveProcessor, LiveProcessor>();
            services.AddSingleton<IFileSystemAuthManager, FileSystemAuthManager>();
            services.AddSingleton<UserManager<ApplicationDbContext>>();
            services.AddIdentity<ApplicationUser, IdentityRole>(
                 option =>
                 {
                     IdentityAppOptions = option;
                     option.User.RequireUniqueEmail = true;
                     option.Cookies.ApplicationCookie.LoginPath = "/signin";
                 }
             ).AddEntityFrameworkStores<ApplicationDbContext>()
             .AddTokenProvider<EmailTokenProvider<ApplicationUser>>(Constants.DefaultFactor)
           // .AddTokenProvider<UserTokenProvider>(Constants.DefaultFactor)
           //  .AddTokenProvider<UserTokenProvider>(Constants.SMSFactor)
           .AddTokenProvider<UserTokenProvider>(Constants.EMailFactor)
         //  .AddTokenProvider<UserTokenProvider>(Constants.AppFactor)
         // 
         ;

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
            services.AddTransient<IEmailSender, MailSender>();
            services.AddTransient<IYavscMessageSender, YavscMessageSender>();
            services.AddTransient<IBillingService, BillingService>();
            services.AddTransient<IDataStore, FileDataStore>((sp) => new FileDataStore("googledatastore", false));
            services.AddTransient<ICalendarManager, CalendarManager>();
            services.AddTransient<Microsoft.Extensions.WebEncoders.UrlEncoder, UrlEncoder>();

        }


        #region OAuth Methods


        private Client GetApplication(string clientId)
        {
            if (DbContext == null)
            {
                logger.LogError("no db!");
                return null;
            }
            Client app = DbContext.Applications.FirstOrDefault(x => x.Id == clientId);
            if (app == null) logger.LogError($"no app for <{clientId}>");
            return app;
        }

        private Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context == null) throw new InvalidOperationException("context == null");
            var app = GetApplication(context.ClientId);
            if (app == null) return Task.FromResult(0);
            Startup.logger.LogInformation($"ValidateClientRedirectUri: Validated ({app.RedirectUri})");
            context.Validated(app.RedirectUri);
            return Task.FromResult(0);
        }

        private Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string clientId, clientSecret;

            if (context.TryGetBasicCredentials(out clientId, out clientSecret) ||
                context.TryGetFormCredentials(out clientId, out clientSecret))
            {
                logger.LogInformation($"ValidateClientAuthentication: Got id: ({clientId} secret: {clientSecret})");
                var client = GetApplication(clientId);
                if (client == null)
                {
                    context.SetError("invalid_clientId", "Client secret is invalid.");
                    return Task.FromResult<object>(null);
                }
                else
                if (client.Type == ApplicationTypes.NativeConfidential)
                {
                    logger.LogInformation($"NativeConfidential key");
                    if (string.IsNullOrWhiteSpace(clientSecret))
                    {
                        logger.LogInformation($"invalid_clientId: Client secret should be sent.");
                        context.SetError("invalid_clientId", "Client secret should be sent.");
                        return Task.FromResult<object>(null);
                    }
                    else
                    {
                        //  if (client.Secret != Helper.GetHash(clientSecret))
                        // TODO store a hash in db, not the pass
                        if (client.Secret != clientSecret)
                        {
                            context.SetError("invalid_clientId", "Client secret is invalid.");
                            logger.LogInformation($"invalid_clientId: Client secret is invalid.");
                            return Task.FromResult<object>(null);
                        }
                    }
                }

                if (!client.Active)
                {
                    context.SetError("invalid_clientId", "Client is inactive.");
                    logger.LogInformation($"invalid_clientId: Client is inactive.");
                    return Task.FromResult<object>(null);
                }

                if (client != null && client.Secret == clientSecret)
                {
                    logger.LogInformation($"\\o/ ValidateClientAuthentication: Validated ({clientId})");
                    context.Validated();
                }
                else logger.LogInformation($":'( ValidateClientAuthentication: KO ({clientId})");
            }
            else logger.LogWarning($"ValidateClientAuthentication: neither Basic nor Form credential were found");
            return Task.FromResult(0);
        }


        private async Task<Task> GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            logger.LogWarning($"GrantResourceOwnerCredentials task ... {context.UserName}");

            ApplicationUser user = null;
            user = DbContext.Users.Include(u => u.Membership).First(u => u.UserName == context.UserName);


            if (await _usermanager.CheckPasswordAsync(user, context.Password))
            {

                var claims = new List<Claim>(
                        context.Scope.Select(x => new Claim("urn:oauth:scope", x))
                )
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email)
                };
                claims.AddRange((await _usermanager.GetRolesAsync(user)).Select(
                    r => new Claim(ClaimTypes.Role, r)
                    ));
                claims.AddRange(user.Membership.Select(
                    m => new Claim(YavscClaimTypes.CircleMembership, m.CircleId.ToString())
                    ));
                ClaimsPrincipal principal = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new GenericIdentity(context.UserName, OAuthDefaults.AuthenticationType),
                        claims)
                        );
                context.HttpContext.User = principal;
                context.Validated(principal);
            }
#if USERMANAGER
#endif
            return Task.FromResult(0);
        }
        private Task GrantClientCredetails(OAuthGrantClientCredentialsContext context)
        {
            var id = new GenericIdentity(context.ClientId, OAuthDefaults.AuthenticationType);
            var claims = context.Scope.Select(x => new Claim("urn:oauth:scope", x));
            var cid = new ClaimsIdentity(id, claims);
            ClaimsPrincipal principal = new ClaimsPrincipal(cid);

            context.Validated(principal);

            return Task.FromResult(0);
        }
        private readonly ConcurrentDictionary<string, string> _authenticationCodes = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

        private void CreateAuthenticationCode(AuthenticationTokenCreateContext context)
        {
            logger.LogInformation("CreateAuthenticationCode");
            context.SetToken(Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n"));
            _authenticationCodes[context.Token] = context.SerializeTicket();
        }

        private void ReceiveAuthenticationCode(AuthenticationTokenReceiveContext context)
        {
            string value;
            if (_authenticationCodes.TryRemove(context.Token, out value))
            {
                context.DeserializeTicket(value);
                logger.LogInformation("ReceiveAuthenticationCode: Success");
            }
        }

        private void CreateRefreshToken(AuthenticationTokenCreateContext context)
        {
            var uid = context.Ticket.Principal.GetUserId();
            logger.LogInformation($"CreateRefreshToken for {uid}");
            foreach (var c in context.Ticket.Principal.Claims)
                logger.LogInformation($"| User claim: {c.Type} {c.Value}");

            context.SetToken(context.SerializeTicket());
        }

        private void ReceiveRefreshToken(AuthenticationTokenReceiveContext context)
        {
            var uid = context.Ticket.Principal.GetUserId();
            logger.LogInformation($"ReceiveRefreshToken for {uid}");
            foreach (var c in context.Ticket.Principal.Claims)
                logger.LogInformation($"| User claim: {c.Type} {c.Value}");
            context.DeserializeTicket(context.Token);
        }

        #endregion


        UserManager<ApplicationUser> _usermanager;

        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ApplicationDbContext dbContext,
            IOptions<TestingSetup> testingSettings,
            UserManager<ApplicationUser> usermanager,
            ILoggerFactory loggerFactory
            )
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            logger = loggerFactory.CreateLogger<Startup>();
            logger.LogInformation(env.EnvironmentName);
            this.DbContext = dbContext;
            TestingSetup = testingSettings.Value;

                    _usermanager = usermanager;

            if (TestingSetup.ConnectionStrings == null)
                logger.LogInformation($" Testing.ConnectionStrings is null : ");
            else
            {
                AppDomain.CurrentDomain.SetData("YAVSC_DB_CONNECTION", TestingSetup.ConnectionStrings.Default);
            }

            var authConf = Configuration.GetSection("Authentication").GetSection("Yavsc");
            var clientId = authConf.GetSection("ClientId").Value;
            var clientSecret = authConf.GetSection("ClientSecret").Value;




            app.UseDeveloperExceptionPage();
            app.UseRuntimeInfoPage();
            var epo = new ErrorPageOptions
            {
                SourceCodeLineCount = 20
            };
            app.UseDeveloperExceptionPage(epo);
            app.UseDatabaseErrorPage(
                x =>
                {
                    x.EnableAll();
                    x.ShowExceptionDetails = true;
                }
            );

            app.UseWelcomePage("/welcome");

            // before fixing the security protocol, let beleive our lib it's done with it.
            //var cxmgr = PayPal.Manager.ConnectionManager.Instance;
            // then, fix it.
            // ServicePointManager.SecurityProtocol = (SecurityProtocolType)0xC00; // Tls12, required by PayPal

            app.UseIISPlatformHandler(options =>
            {
                options.AuthenticationDescriptions.Clear();
                options.AutomaticAuthentication = false;
            });
            app.UseSession();


            app.UseIdentity();

            ProtectionProvider = new MonoDataProtectionProvider(Configuration["Site:Title"]); ;

            app.UseWhen(context => context.Request.Path.StartsWithSegments("/api")
             || context.Request.Path.StartsWithSegments("/live"),
                branchLiveOrApi =>
                {
                    branchLiveOrApi.UseJwtBearerAuthentication(

                        options =>
                        {
                            options.AuthenticationScheme = JwtBearerDefaults.AuthenticationScheme;
                            options.AutomaticAuthenticate = true;
                            options.SecurityTokenValidators.Clear();
                            var tickeDataProtector = new TicketDataFormatTokenValidator(
                                ProtectionProvider
                            );
                            options.SecurityTokenValidators.Add(tickeDataProtector);
                            options.Events = new JwtBearerEvents
                            {
                                OnReceivingToken = context =>
                               {
                                   return Task.Run(() =>
                                   {
                                       var signalRTokenHeader = context.Request.Query["signalRTokenHeader"];

                                       if (!string.IsNullOrEmpty(signalRTokenHeader) &&
                                           (context.HttpContext.WebSockets.IsWebSocketRequest || context.Request.Headers["Accept"] == "text/event-stream"))
                                       {
                                           context.Token = context.Request.Query["signalRTokenHeader"];
                                       }
                                   });
                               }
                            };
                        });
                });

            app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api") && !context.Request.Path.StartsWithSegments("/live"),
                branch =>
                {
                    // External authentication shared cookie:
                    branch.UseCookieAuthentication(options =>
                    {
                        ExternalCookieAppOptions = options;
                        options.AuthenticationScheme = Yavsc.Constants.ExternalAuthenticationSheme;
                        options.AutomaticAuthenticate = true;
                        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                        options.LoginPath = new PathString(Yavsc.Constants.LoginPath.Substring(1));
                        options.AccessDeniedPath = new PathString(Yavsc.Constants.LoginPath.Substring(1));
                    });

                    YavscGoogleAppOptions = new Yavsc.Auth.YavscGoogleOptions
                    {
                        ClientId = GoogleWebClientConfiguration["web:client_id"],
                        ClientSecret = GoogleWebClientConfiguration["web:client_secret"],
                        AccessType = "offline",
                        Scope = {
                                "profile",
                                "https://www.googleapis.com/auth/admin.directory.resource.calendar",
                                "https://www.googleapis.com/auth/calendar",
                                "https://www.googleapis.com/auth/calendar.events"
                            },
                        SaveTokensAsClaims = true,
                        UserInformationEndpoint = "https://www.googleapis.com/plus/v1/people/me",
                        Events = new OAuthEvents
                        {
                            OnCreatingTicket = async context =>
                            {
                                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                                    .CreateScope())
                                {
                                    var gcontext = context as Yavsc.Auth.GoogleOAuthCreatingTicketContext;
                                    context.Identity.AddClaim(new Claim(YavscClaimTypes.GoogleUserId, gcontext.GoogleUserId));
                                    var DbContext = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();

                                    var store = serviceScope.ServiceProvider.GetService<IDataStore>();
                                    await store.StoreAsync(gcontext.GoogleUserId, new TokenResponse
                                    {
                                        AccessToken = gcontext.TokenResponse.AccessToken,
                                        RefreshToken = gcontext.TokenResponse.RefreshToken,
                                        TokenType = gcontext.TokenResponse.TokenType,
                                        ExpiresInSeconds = int.Parse(gcontext.TokenResponse.ExpiresIn),
                                        IssuedUtc = DateTime.Now
                                    });
                                    await dbContext.StoreTokenAsync(gcontext.GoogleUserId,
                                        gcontext.TokenResponse.Response,
                                        gcontext.TokenResponse.AccessToken,
                                        gcontext.TokenResponse.TokenType,
                                        gcontext.TokenResponse.RefreshToken,
                                        gcontext.TokenResponse.ExpiresIn);

                                }
                            }
                        }
                    };

                    branch.UseMiddleware<Yavsc.Auth.GoogleMiddleware>(YavscGoogleAppOptions);
                    /* FIXME 403
                                      
                                    branch.UseTwitterAuthentication(options=>
                                    {
                                            TwitterAppOptions = options;
                                            options.ConsumerKey = Configuration["Authentication:Twitter:ClientId"];
                                            options.ConsumerSecret = Configuration["Authentication:Twitter:ClientSecret"];
                                    }); */

                    branch.UseOAuthAuthorizationServer(

                        options =>
                        {
                            OAuthServerAppOptions = options;
                            options.AuthorizeEndpointPath = new PathString(Constants.AuthorizePath.Substring(1));
                            options.TokenEndpointPath = new PathString(Constants.TokenPath.Substring(1));
                            options.ApplicationCanDisplayErrors = true;
                            options.AllowInsecureHttp = true;
                            options.AuthenticationScheme = OAuthDefaults.AuthenticationType;
                            options.TokenDataProtector = ProtectionProvider.CreateProtector("Bearer protection");

                            options.Provider = new OAuthAuthorizationServerProvider
                            {
                                OnValidateClientRedirectUri = ValidateClientRedirectUri,
                                OnValidateClientAuthentication = ValidateClientAuthentication,
                                OnGrantResourceOwnerCredentials = GrantResourceOwnerCredentials,
                                OnGrantClientCredentials = GrantClientCredetails
                            };

                            options.AuthorizationCodeProvider = new AuthenticationTokenProvider
                            {
                                OnCreate = CreateAuthenticationCode,
                                OnReceive = ReceiveAuthenticationCode,
                            };

                            options.RefreshTokenProvider = new AuthenticationTokenProvider
                            {
                                OnCreate = CreateRefreshToken,
                                OnReceive = ReceiveRefreshToken,
                            };

                            options.AutomaticAuthenticate = true;
                            options.AutomaticChallenge = true;
                        }
                    );
                });

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "google-secret.json");

            /* TODO

            ConfigureFileServerApp(app, SiteSetup, env, authorizationService);

            app.UseRequestLocalization(localizationOptions.Value, (RequestCulture)new RequestCulture((string)"en-US"));

            ConfigureWorkflow();
            */

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

        }

    }
}
