
using System;
using System.Globalization;
using System.IdentityModel.Tokens;
using System.IO;
using System.Reflection;
using System.Security.Claims;
using System.Web;
using System.Web.Optimization;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Authentication.OAuth;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.DataProtection.Infrastructure;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
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
using Microsoft.Extensions.WebEncoders;
using Microsoft.Net.Http.Headers;
using Yavsc.Auth;
using Yavsc.Formatters;
using Yavsc.Models;
using Yavsc.Services;
using OAuth.AspNet.AuthServer;

namespace Yavsc
{

    public partial class Startup
    {
        public static string ConnectionString { get; private set; }
        private RsaSecurityKey key;

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

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new ProducesAttribute("text/x-tex"));
                options.Filters.Add(new ProducesAttribute("text/pdf"));
            });

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("fr")
                };


                // You must explicitly state which cultures your application supports.
                // These are the cultures the app supports for formatting numbers, dates, etc.
                options.SupportedCultures = supportedCultures;

                // These are the cultures the app supports for UI strings, i.e. we have localized resources for.
                options.SupportedUICultures = supportedCultures;

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
            var keyParamsFileInfo =
                new FileInfo(Configuration["DataProtection:RSAParamFile"]);
            var keyParams = (keyParamsFileInfo.Exists) ?
                RSAKeyUtils.GetKeyParameters(keyParamsFileInfo.Name) :
                RSAKeyUtils.GenerateKeyAndSave(keyParamsFileInfo.Name);
            key = new RsaSecurityKey(keyParams);

            services.Configure<SharedAuthenticationOptions>(options =>
            {
                options.SignInScheme = "ServerCookie";
            });

            services.Configure<TokenAuthOptions>(
                to =>
                {
                    to.Audience = Configuration["Site:Audience"];
                    to.Issuer = Configuration["Site:Authority"];
                    to.SigningCredentials =
                    new SigningCredentials(key, SecurityAlgorithms.RsaSha256Signature);
                }
            );


            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<SiteSettings>), typeof(OptionsManager<SiteSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<SmtpSettings>), typeof(OptionsManager<SmtpSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<GoogleAuthSettings>), typeof(OptionsManager<GoogleAuthSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<CompanyInfoSettings>), typeof(OptionsManager<CompanyInfoSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<OAuth2AppSettings>), typeof(OptionsManager<OAuth2AppSettings>)));

            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<TokenAuthOptions>), typeof(OptionsManager<TokenAuthOptions>)));


            services.AddTransient<Microsoft.Extensions.WebEncoders.UrlEncoder, UrlEncoder>();
            services.AddDataProtection();
            services.Add(ServiceDescriptor.Singleton(typeof(IApplicationDiscriminator),
                typeof(SystemWebApplicationDiscriminator)));

            services.ConfigureDataProtection(configure =>
            {
                configure.SetApplicationName(Configuration["Site:Title"]);
                configure.SetDefaultKeyLifetime(TimeSpan.FromDays(45));
                configure.PersistKeysToFileSystem(
                     new DirectoryInfo(Configuration["DataProtection:Keys:Dir"]));
            });

            services.AddAuthentication(
                op => op.SignInScheme = "ServerCookie"
            );
            // Add framework services.
            services.AddEntityFramework()
              .AddNpgsql()
              .AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(ConnectionString))
              ;

            services.AddIdentity<ApplicationUser, IdentityRole>(
                option =>
                {
                    option.User.AllowedUserNameCharacters += " ";
                    option.User.RequireUniqueEmail = true;
                    option.Cookies.ApplicationCookie.DataProtectionProvider =
                        new MonoDataProtectionProvider(Configuration["Site:Title"]);
                }
            ).AddEntityFrameworkStores<ApplicationDbContext>()
                 .AddTokenProvider<EmailTokenProvider<ApplicationUser>>(Constants.EMailFactor)
                 .AddTokenProvider<UserTokenProvider>(Constants.DefaultFactor)
                ;

          

            //  .AddTokenProvider<UserTokenProvider>(Constants.SMSFactor)
            //

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

            services.AddMvc(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                  .RequireAuthenticatedUser()
                    .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
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

            services.AddTransient<IApplicationStore,ApplicationDbContext>();

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
         ILoggerFactory loggerFactory)
        {
            Startup.UserFilesDirName = siteSettings.Value.UserFiles.DirName;
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                loggerFactory.MinimumLevel = LogLevel.Debug;
                app.UseDeveloperExceptionPage();
                app.UseStatusCodePagesWithReExecute("/Home/Status/{0}");
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

            var googleOptions = new YavscGoogleOptions
            {
                ClientId = Configuration["Authentication:Google:ClientId"],
                ClientSecret = Configuration["Authentication:Google:ClientSecret"],
                AccessType = "offline",
                SaveTokensAsClaims = true,
                UserInformationEndpoint = "https://www.googleapis.com/plus/v1/people/me"
            };
            var gvents = new OAuthEvents();

            googleOptions.Events = new OAuthEvents
            {
                OnCreatingTicket = async context =>
                {
                    using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                            .CreateScope())
                    {
                        var gcontext = context as GoogleOAuthCreatingTicketContext;
                        context.Identity.AddClaim(new Claim(YavscClaimTypes.GoogleUserId, gcontext.GoogleUserId));
                        var service =
                         serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                        await service.StoreTokenAsync(gcontext.GoogleUserId, context.TokenResponse);
                    }
                }
            };

            googleOptions.Scope.Add("https://www.googleapis.com/auth/calendar");

            app.UseIISPlatformHandler(options =>
            {
                options.AuthenticationDescriptions.Clear();
                options.AutomaticAuthentication = true;
            });
            
            ConfigureFileServerApp(app,siteSettings.Value,env);

            app.UseWebSockets();

            app.UseIdentity();


            /* app.UseOpenIdConnectServer(options =>
                {
                    options.Provider = new AuthorizationProvider(loggerFactory, 
                    new UserTokenProvider());

                    // Register the certificate used to sign the JWT tokens.
                    // options.SigningCredentials.AddCertificate(
                    //   assembly: typeof(Startup).GetTypeInfo().Assembly,
                    //   resource: "Mvc.Server.Certificate.pfx",
                    //   password: "Owin.Security.OpenIdConnect.Server"); 

                    // options.SigningCredentials.AddKey(key);
                    // Note: see AuthorizationController.cs for more
                    // information concerning ApplicationCanDisplayErrors.
                    options.ApplicationCanDisplayErrors = true;
                    options.AllowInsecureHttp = true;
                    options.AutomaticChallenge = true;
                  //  options.AutomaticAuthenticate=true;


                    options.AuthorizationEndpointPath = new PathString("/connect/authorize");
                    options.TokenEndpointPath = new PathString("/connect/authorize/accept");
                    options.UseSlidingExpiration = true;
                    options.AllowInsecureHttp = true;
                    options.AuthenticationScheme = "oidc-server"; // was = OpenIdConnectDefaults.AuthenticationScheme || "oidc";
                    options.LogoutEndpointPath = new PathString("/connect/logout");

                    // options.ValidationEndpointPath = new PathString("/connect/introspect");
                }); */

            app.UseCookieAuthentication(options =>
               {
                   options.AutomaticAuthenticate = true;
                   options.AutomaticChallenge = true;
                   options.AuthenticationScheme = "ServerCookie";
                   options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                   options.LoginPath = new PathString("/signin");
                   options.LogoutPath = new PathString("/signout");
               });

            app.UseMiddleware<Yavsc.Auth.GoogleMiddleware>(googleOptions);

            // Facebook
            app.UseFacebookAuthentication(options =>
                {
                    options.AppId = Configuration["Authentication:Facebook:AppId"];
                    options.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
                    options.Scope.Add("email");
                    options.UserInformationEndpoint = "https://graph.facebook.com/v2.5/me?fields=id,name,email,first_name,last_name";
                });
                
            app.UseOAuthAuthorizationServer(
            
                options =>
                {
                    options.AuthorizeEndpointPath = new PathString("/signin");
                    options.TokenEndpointPath = new PathString("/token");
                    options.ApplicationCanDisplayErrors = true;

#if DEBUG
                    options.AllowInsecureHttp = true;
#endif

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

                    options.AutomaticAuthenticate = false;
                }
            );

            app.UseRequestLocalization(localizationOptions.Value, (RequestCulture)new RequestCulture((string)"fr"));

            /* Generic OAuth (here GitHub): options.Notifications = new OAuthAuthenticationNotifications
                           {
                               OnGetUserInformationAsync = async context =>
                               {
                        // Get the GitHub user
                        HttpRequestMessage userRequest = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                                   userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                                   userRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                   HttpResponseMessage userResponse = await context.Backchannel.SendAsync(userRequest, context.HttpContext.RequestAborted);
                                   userResponse.EnsureSuccessStatusCode();
                                   var text = await userResponse.Content.ReadAsStringAsync();
                                   JObject user = JObject.Parse(text);

                                   var identity = new ClaimsIdentity(
                                       context.Options.AuthenticationType,
                                       ClaimsIdentity.DefaultNameClaimType,
                                       ClaimsIdentity.DefaultRoleClaimType);

                                   JToken value;
                                   var id = user.TryGetValue("id", out value) ? value.ToString() : null;
                                   if (!string.IsNullOrEmpty(id))
                                   {
                                       identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, id, ClaimValueTypes.String, context.Options.AuthenticationType));
                                   }
                                   var userName = user.TryGetValue("login", out value) ? value.ToString() : null;
                                   if (!string.IsNullOrEmpty(userName))
                                   {
                                       identity.AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, userName, ClaimValueTypes.String, context.Options.AuthenticationType));
                                   }
                                   var name = user.TryGetValue("name", out value) ? value.ToString() : null;
                                   if (!string.IsNullOrEmpty(name))
                                   {
                                       identity.AddClaim(new Claim("urn:github:name", name, ClaimValueTypes.String, context.Options.AuthenticationType));
                                   }
                                   var link = user.TryGetValue("url", out value) ? value.ToString() : null;
                                   if (!string.IsNullOrEmpty(link))
                                   {
                                       identity.AddClaim(new Claim("urn:github:url", link, ClaimValueTypes.String, context.Options.AuthenticationType));
                                   }

                                   context.Identity = identity;
                               }
                           }; */


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseSignalR();
        }
        private sealed class SystemWebApplicationDiscriminator : IApplicationDiscriminator
        {
            private readonly Lazy<string> _lazyDiscriminator = new Lazy<string>(GetAppDiscriminatorCore);

            public string Discriminator => _lazyDiscriminator.Value;

            private static string GetAppDiscriminatorCore()
            {
                return HttpRuntime.AppDomainAppId;
            }
        }
        // Entry point for the application.
        public static void Main(string[] args) => Microsoft.AspNet.Hosting.WebApplication.Run<Startup>(args);
    }

}
