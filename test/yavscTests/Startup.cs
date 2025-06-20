using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor;
using Yavsc;
using Yavsc.Models;
using Yavsc.Services;
using Microsoft.Extensions.WebEncoders;
using yavscTests.Settings;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using Yavsc.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.OAuth;
using Yavsc.Helpers.Auth;
using Google.Apis.Util.Store;
using System.Security.Claims;
using Google.Apis.Auth.OAuth2.Responses;
using Constants = Yavsc.Constants;
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
using Microsoft.Net.Http.Headers;
using static Yavsc.Startup;
using Microsoft.AspNetCore.DataProtection.Infrastructure;
using System.IO;
using Yavsc.Lib;
using Yavsc.Settings;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Authorization;

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
               private static ILogger logger;

        public static string ApiKey { get; private set; }


        public Startup(IHostingEnvironment env)
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

             services.AddTransient(typeof(IEmailSender), typeof(MailSender));
            services.AddEntityFramework().AddNpgsql().AddDbContext<ApplicationDbContext>();
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

            services.AddAuthorizationBuilder()
                .AddPolicy("AdministratorOnly", policy =>
                {
                    policy.RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", Constants.AdminGroupName);
                })
                .AddPolicy("FrontOffice", policy => policy.RequireRole(Constants.FrontOfficeGroupName))
                .AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes("yavsc")
                    .RequireAuthenticatedUser().Build())
                .AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());

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
                     option.User.RequireUniqueEmail = true;
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
            services.AddTransient<ISecureDataFormat<AuthenticationTicket>, TicketDataFormat>();

            // Add application services.
            services.AddTransient<IEmailSender<ApplicationUser>, MailSender>();
            services.AddTransient<IYavscMessageSender, YavscMessageSender>();
            services.AddTransient<IBillingService, BillingService>();
            services.AddTransient<IDataStore, FileDataStore>((sp) => new FileDataStore("googledatastore", false));
            services.AddTransient<ICalendarManager, CalendarManager>();
      
        }



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

          

            ConfigureFileServerApp(app, SiteSetup, env, authorizationService);

            app.UseRequestLocalization(localizationOptions.Value, (RequestCulture)new RequestCulture((string)"en-US"));

            ConfigureWorkflow();
           

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

        }

    }
}
