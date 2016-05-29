
using System;
using System.Globalization;
using System.IdentityModel.Tokens;
using System.IO;
using System.Reflection;
using System.Security.Claims;
using System.Web;
using System.Web.Optimization;
using AspNet.Security.OpenIdConnect.Extensions;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authentication.OAuth;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.DataProtection.Infrastructure;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Localization;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.AspNet.StaticFiles;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.WebEncoders;
using Microsoft.Net.Http.Headers;
using Yavsc.Auth;
using Yavsc.Extensions;
using Yavsc.Formatters;
using Yavsc.Models;
using Yavsc.Providers;
using Yavsc.Services;




namespace Yavsc
{


    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/bootjq").Include(
                "~/bower_components/bootstrap/dist/js",
            "~/bower_components/jquery/dist/js",
            "~/bower_components/jquery.validation/dist/js",
            "~/bower_components/jquery-validation-unobtrusive/dist/js",
            "~/bower_components/bootstrap-datepicker/dist/js"));
            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
            ));
            bundles.Add(new ScriptBundle("~/bundles/markdown").Include(
                "~/bower_components/dropzone/dist/min/dropzone-amd-module.min.js",
                "~/bower_components/dropzone/dist/min/dropzone.min.js"
            ));
            bundles.Add(new StyleBundle("~/Content/markdown").Include(
                "~/bower_components/dropzone/dist/min/basic.min.css",
                "~/bower_components/dropzone/dist/min/dropzone.min.css"
            ));
        }
    }

    public class Startup
    {
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

            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<SiteSettings>), typeof(OptionsManager<SiteSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<SmtpSettings>), typeof(OptionsManager<SmtpSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<GoogleAuthSettings>), typeof(OptionsManager<GoogleAuthSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<CompanyInfoSettings>), typeof(OptionsManager<CompanyInfoSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<OAuth2AppSettings>), typeof(OptionsManager<OAuth2AppSettings>)));

            services.AddTransient<Microsoft.Extensions.WebEncoders.UrlEncoder, UrlEncoder>();
            services.AddDataProtection();
            services.Add(ServiceDescriptor.Singleton(typeof(IApplicationDiscriminator),
                typeof(SystemWebApplicationDiscriminator)));

            services.ConfigureDataProtection(configure =>
            {
                configure.SetApplicationName("Yavsc");
                configure.SetDefaultKeyLifetime(TimeSpan.FromDays(45));
                configure.PersistKeysToFileSystem(
                     new DirectoryInfo(Configuration["DataProtection:Keys:Dir"]));
            });

            services.AddAuthentication(options =>
            {
                options.SignInScheme = "ServerCookie";
            }
            );
            // Add framework services.
            services.AddEntityFramework()
              .AddNpgsql()
              .AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(Configuration["Data:DefaultConnection:ConnectionString"]))
              ;

            services.AddIdentity<ApplicationUser, IdentityRole>(
                option =>
                {
                    option.User.AllowedUserNameCharacters += " ";
                    option.User.RequireUniqueEmail = true;
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
                options.AddPolicy("API", policy =>
                {
                    policy.RequireClaim(OpenIdConnectConstants.Claims.Scope, "api-resource-controller");
                });
                // options.AddPolicy("EmployeeId", policy => policy.RequireClaim("EmployeeId", "123", "456"));
                // options.AddPolicy("BuildingEntry", policy => policy.Requirements.Add(new OfficeEntryRequirement()));
            });

            services.AddSingleton<IAuthorizationHandler, HasBadgeHandler>();
            services.AddSingleton<IAuthorizationHandler, HasTemporaryPassHandler>();
            services.AddSingleton<IAuthorizationHandler, BlogEditHandler>();
            services.AddSingleton<IAuthorizationHandler, BlogViewHandler>();
            services.AddSingleton<IAuthorizationHandler, CommandEditHandler>();
            services.AddSingleton<IAuthorizationHandler, CommandViewHandler>();
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
        IOptions<SiteSettings> siteSettings, IOptions<RequestLocalizationOptions> localizationOptions,
        IOptions<OAuth2AppSettings> oauth2SettingsContainer,
         ILoggerFactory loggerFactory)
        {
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

            var udirinfo = new DirectoryInfo(Configuration["Site:UserFiles:RootDir"]);
            if (!udirinfo.Exists)
                throw new Exception($"Configuration value for Site:UserFiles:RootDir : {udirinfo.FullName}");


            var googleOptions = new GoogleOptions
            {
                ClientId = Configuration["Authentication:Google:ClientId"],
                ClientSecret = Configuration["Authentication:Google:ClientSecret"],
                AccessType = "offline",
                SaveTokensAsClaims = true,
                UserInformationEndpoint = "https://www.googleapis.com/plus/v1/people/me",
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

            app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());

            app.UseStaticFiles().UseWebSockets();

            app.UseRequestLocalization(localizationOptions.Value, (RequestCulture)new RequestCulture((string)"fr"));

            app.UseIdentity();

            app.UseWhen(context => context.Request.Path.StartsWithSegments(new PathString("/api")), branch =>
            {
                branch.UseJwtBearerAuthentication(options =>
                {
                    options.AutomaticAuthenticate = true;
                    options.AutomaticChallenge = true;
                    options.RequireHttpsMetadata = false;
                    options.Audience = siteSettings.Value.Audience;
                    options.Authority = siteSettings.Value.Authority;
                });
            });

            // Create a new branch where the registered middleware will be executed only for API calls.
            app.UseWhen(context => !context.Request.Path.StartsWithSegments(new PathString("/api")), branch =>
            {
                // Create a new branch where the registered middleware will be executed only for non API calls.
                branch.UseCookieAuthentication(options =>
                {
                    options.AutomaticAuthenticate = true;
                    options.AutomaticChallenge = true;
                    options.AuthenticationScheme = "ServerCookie";
                    options.CookieName = CookieAuthenticationDefaults.CookiePrefix + "ServerCookie";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                    options.LoginPath = new PathString("/signin");
                    options.LogoutPath = new PathString("/signout");
                });


                branch.UseMiddleware<GoogleMiddleware>(googleOptions);

                // Facebook
                branch.UseFacebookAuthentication(options =>
                    {
                        options.AppId = Configuration["Authentication:Facebook:AppId"];
                        options.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
                        options.Scope.Add("email");
                        options.UserInformationEndpoint = "https://graph.facebook.com/v2.5/me?fields=id,name,email,first_name,last_name";
                    });

            });
            app.UseOpenIdConnectServer(options =>
               {
                   options.Provider = new AuthorizationProvider(loggerFactory);

                   // Register the certificate used to sign the JWT tokens.
                   /* options.SigningCredentials.AddCertificate(
                       assembly: typeof(Startup).GetTypeInfo().Assembly,
                       resource: "Mvc.Server.Certificate.pfx",
                       password: "Owin.Security.OpenIdConnect.Server"); */

                   // options.SigningCredentials.AddKey(key);
                   // Note: see AuthorizationController.cs for more
                   // information concerning ApplicationCanDisplayErrors.
                   options.ApplicationCanDisplayErrors = true;
                   options.AllowInsecureHttp = true;
                   options.AutomaticChallenge = true;
                   options.AuthorizationEndpointPath = new PathString("/connect/authorize");
                   options.TokenEndpointPath = new PathString("/connect/authorize/accept");
                   options.UseSlidingExpiration = true;
                   options.AllowInsecureHttp = true;
                   options.AuthenticationScheme = "oidc"; // was = OpenIdConnectDefaults.AuthenticationScheme;
                   options.LogoutEndpointPath = new PathString("/connect/logout");
                   /* options.ValidationEndpointPath = new PathString("/connect/introspect"); */
               });

            app.UseFileServer(new FileServerOptions()
            {
                FileProvider = new PhysicalFileProvider(
                     udirinfo.FullName),
                RequestPath = new PathString(Constants.UserFilesRequestPath),
                EnableDirectoryBrowsing = false
            });


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
                /*   routes.MapRoute(
                       name: "wf",
                       template: "do/{action=Index}/{id?}",
                       defaults: new { controller = "WorkFlow" }); */
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
