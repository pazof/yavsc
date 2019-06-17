
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Web.Optimization;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Localization;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Yavsc
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.WebSockets;
    using System.Security.Claims;
    using System.Threading;
    using Formatters;
    using Google.Apis.Util.Store;
    using Microsoft.AspNet.Http;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.SignalR;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using Models;
    using PayPal.Manager;
    using Services;
    using Yavsc.Abstract.FileSystem;
    using Yavsc.AuthorizationHandlers;
    using Yavsc.Controllers;
    using Yavsc.Helpers;
    using Yavsc.ViewModels.Streaming;
    using static System.Environment;

    public partial class Startup
    {
        public static string AvatarsDirName { private set; get; }
        public static string GitDirName { private set; get; }
        public static string Authority { get; private set; }
        public static string Temp { get; set; }
        public static SiteSettings SiteSetup { get; private set; }
        public static GoogleServiceAccount GServiceAccount { get; private set; }

        public static string HostingFullName { get; set; }

        public static PayPalSettings PayPalSettings { get; private set; }
        private static ILogger logger;

        // leave the final slash

        PathString liveCastingPath = "/live/cast";

        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            var devtag = env.IsDevelopment() ? "D" : "";
            var prodtag = env.IsProduction() ? "P" : "";
            var stagetag = env.IsStaging() ? "S" : "";

            HostingFullName = $"{appEnv.RuntimeFramework.FullName} [{env.EnvironmentName}:{prodtag}{devtag}{stagetag}]";
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

            var auth = Configuration["Site:Authority"];
            var cxstr = Configuration["ConnectionStrings:Default"];
            ConnectionString = cxstr;

            AppDomain.CurrentDomain.SetData(Constants.YavscConnectionStringEnvName, ConnectionString);

            var googleClientFile = Configuration["Authentication:Google:GoogleWebClientJson"];
            var googleServiceAccountJsonFile = Configuration["Authentication:Google:GoogleServiceAccountJson"];
            if (googleClientFile != null)
                GoogleWebClientConfiguration = new ConfigurationBuilder().AddJsonFile(googleClientFile).Build();
            if (googleServiceAccountJsonFile != null)
            {
                var safile = new FileInfo(googleServiceAccountJsonFile);
                GServiceAccount = JsonConvert.DeserializeObject<GoogleServiceAccount>(safile.OpenText().ReadToEnd());
            }
        }

        public static string ConnectionString { get; set; }
        public static GoogleAuthSettings GoogleSettings { get; set; }
        public IConfigurationRoot Configuration { get; set; }
        public static IConfigurationRoot GoogleWebClientConfiguration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            // Database connection

            services.AddOptions();
            var siteSettings = Configuration.GetSection("Site");
            services.Configure<SiteSettings>(siteSettings);
            var smtpSettings = Configuration.GetSection("Smtp");
            services.Configure<SmtpSettings>(smtpSettings);
            var googleSettings = Configuration.GetSection("Authentication").GetSection("Google");
            services.Configure<GoogleAuthSettings>(googleSettings);
            var cinfoSettings = Configuration.GetSection("Authentication").GetSection("Societeinfo");
            services.Configure<CompanyInfoSettings>(cinfoSettings);
            var oauthFacebookSettings = Configuration.GetSection("Authentication").GetSection("Facebook");
            services.Configure<FacebookOAuth2AppSettings>(oauthFacebookSettings);
            var paypalSettings = Configuration.GetSection("Authentication").GetSection("PayPal");
            services.Configure<PayPalSettings>(paypalSettings);

            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<SiteSettings>), typeof(OptionsManager<SiteSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<SmtpSettings>), typeof(OptionsManager<SmtpSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<GoogleAuthSettings>), typeof(OptionsManager<GoogleAuthSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<CompanyInfoSettings>), typeof(OptionsManager<CompanyInfoSettings>)));
            services.Add(ServiceDescriptor.Singleton(typeof(IOptions<RequestLocalizationOptions>), typeof(OptionsManager<RequestLocalizationOptions>)));
            
           
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en"),
                    new CultureInfo("fr"),
                    new CultureInfo("pt")
                };

                var supportedUICultures = new[]
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
                //  return new ProviderCultureResult("fr");
                //}));

                options.RequestCultureProviders = new List<IRequestCultureProvider>
                    {
                        new QueryStringRequestCultureProvider { Options = options },
                        new CookieRequestCultureProvider { Options = options, CookieName="ASPNET_CULTURE" },
                        new AcceptLanguageHeaderRequestCultureProvider { Options = options }
                    };
            });

            // DataProtection
            ConfigureProtectionServices(services);


            // Add framework services.
            services.AddEntityFramework()
              .AddNpgsql()
              .AddDbContext<ApplicationDbContext>(
                  db => db.UseNpgsql(ConnectionString)
              );

            ConfigureOAuthServices(services);

            services.AddCors(

            options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.WithOrigins("*");
                });
            }

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
                options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
            });

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

            // TODO for SMS: services.AddTransient<ISmsSender, AuthMessageSender>();

            services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });
            CheckServices(services);
        }
        static ApplicationDbContext _dbContext;
        public static IServiceProvider Services { get; private set; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, IHostingEnvironment env,
            ApplicationDbContext dbContext, IOptions<SiteSettings> siteSettings,
        IOptions<RequestLocalizationOptions> localizationOptions,
        IOptions<OAuth2AppSettings> oauth2SettingsContainer,
        IAuthorizationService authorizationService,
        IOptions<PayPalSettings> payPalSettings,
        IOptions<GoogleAuthSettings> googleSettings,
        IStringLocalizer<Yavsc.YavscLocalisation> localizer,
        UserManager<ApplicationUser> usermanager,
         ILoggerFactory loggerFactory)
        {
            Services = app.ApplicationServices;

            _dbContext = dbContext;
            _usermanager = usermanager;
            GoogleSettings = googleSettings.Value;
            ResourcesHelpers.GlobalLocalizer = localizer;
            SiteSetup = siteSettings.Value;
            Authority = siteSettings.Value.Authority;
            var blogsDir = siteSettings.Value.Blog;
            if (blogsDir == null) throw new Exception("blogsDir is not set.");
            var billsDir = siteSettings.Value.Bills;
            if (billsDir == null) throw new Exception("billsDir is not set.");

            AbstractFileSystemHelpers.UserFilesDirName = new DirectoryInfo(blogsDir).FullName;
            AbstractFileSystemHelpers.UserBillsDirName = new DirectoryInfo(billsDir).FullName;
            Temp = siteSettings.Value.TempDir;
            PayPalSettings = payPalSettings.Value;

            // TODO implement an installation & upgrade procedure
            // Create required directories
            foreach (string dir in new string[] { AbstractFileSystemHelpers.UserFilesDirName, AbstractFileSystemHelpers.UserBillsDirName, SiteSetup.TempDir })
            {
                if (dir == null) throw new Exception(nameof(dir));

                DirectoryInfo di = new DirectoryInfo(dir);
                if (!di.Exists) di.Create();
            }

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            logger = loggerFactory.CreateLogger<Startup>();
            app.UseStatusCodePagesWithReExecute("/Home/Status/{0}");

            if (env.IsDevelopment())
            {
                var logenvvar = Environment.GetEnvironmentVariable("ASPNET_LOG_LEVEL");
                if (logenvvar != null)
                    switch (logenvvar)
                    {
                        case "info":
                            loggerFactory.MinimumLevel = LogLevel.Information;
                            break;
                        case "warn":
                            loggerFactory.MinimumLevel = LogLevel.Warning;
                            break;
                        case "err":
                            loggerFactory.MinimumLevel = LogLevel.Error;
                            break;
                        case "debug":
                        default:
                            loggerFactory.MinimumLevel = LogLevel.Debug;
                            break;
                    }


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
                        // TODO (or not) Hit the developper
                    }
                    else throw ex;
                }
            }
            // before fixing the security protocol, let beleive our lib it's done with it.
            var cxmgr = PayPal.Manager.ConnectionManager.Instance;
            // then, fix it.
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)0xC00; // Tls12, required by PayPal

            app.UseIISPlatformHandler(options =>
            {
                options.AuthenticationDescriptions.Clear();
                options.AutomaticAuthentication = false;
            });
            app.UseSession();

            ConfigureOAuthApp(app, SiteSetup, logger);
            ConfigureFileServerApp(app, SiteSetup, env, authorizationService);
            app.UseRequestLocalization(localizationOptions.Value, (RequestCulture)new RequestCulture((string)"en-US"));

            ConfigureWorkflow(app, SiteSetup, logger);
            // Empty this odd chat user list from db
            foreach (var p in dbContext.ChatConnection)
            {
              dbContext.Entry(p).State = EntityState.Deleted;
            }
            dbContext.SaveChanges();

            ConfigureWebSocketsApp(app, SiteSetup, env);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            logger.LogInformation("LocalApplicationData: " + Environment.GetFolderPath(SpecialFolder.LocalApplicationData, SpecialFolderOption.DoNotVerify));
            app.Use(async (context, next) =>
                {
                    var liveCasting = context.Request.Path.StartsWithSegments(liveCastingPath);
                    if (liveCasting)
                    {

                        // ensure this request is for a websocket
                        if (context.WebSockets.IsWebSocketRequest)
                        {
                            if (!context.User.Identity.IsAuthenticated)
                                context.Response.StatusCode = 403;
                            else
                            {
                                // get the flow id from request path
                                var castid = long.Parse(context.Request.Path.Value.Substring(liveCastingPath.Value.Length + 1));


                                var uname = context.User.GetUserName();
                                // ensure uniqueness of casting stream from this user


                                var uid = context.User.GetUserId();
                                // get some setup from user
                                var flow = _dbContext.LiveFlow.Include(f => f.Owner).SingleOrDefault(f => (f.OwnerId == uid && f.Id == castid));
                                if (flow == null)
                                {
                                    context.Response.StatusCode = 400;
                                }
                                else
                                {
                                    LiveCastMeta meta = null;
                                    if (LiveApiController.Casters.ContainsKey(uname))
                                    {
                                        meta = LiveApiController.Casters[uname];
                                        if (meta.Socket.State != WebSocketState.Closed)
                                        {
                                            // FIXME loosed connexion should be detected & disposed else where
                                            meta.Socket.Dispose();
                                            meta.Socket = await context.WebSockets.AcceptWebSocketAsync();
                                        }
                                        else
                                        {
                                            meta.Socket.Dispose();
                                            // Accept the socket
                                            meta.Socket = await context.WebSockets.AcceptWebSocketAsync();
                                        }
                                    }
                                    else
                                    {
                                        // Accept the socket
                                        meta = new LiveCastMeta { Socket = await context.WebSockets.AcceptWebSocketAsync() };
                                    }
                                    logger.LogInformation("Accepted web socket");
                                    // Dispatch the flow
                                    try
                                    {



                                        if (meta.Socket != null && meta.Socket.State == WebSocketState.Open)
                                        {
                                            LiveApiController.Casters[uname] = meta;
                                            // TODO: Handle the socket here.
                                            // Find receivers: others in the chat room
                                            // send them the flow

                                            var sBuffer = new ArraySegment<byte>(new byte[1024]);
                                            logger.LogInformation("Receiving bytes...");

                                            WebSocketReceiveResult received = await meta.Socket.ReceiveAsync(sBuffer, CancellationToken.None);
                                            logger.LogInformation("Received bytes!!!!");

                                            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();

                                            hubContext.Clients.All.addPublicStream(new
                                            {
                                                id = flow.Id,
                                                sender = flow.Owner.UserName,
                                                title = flow.Title,
                                                url = flow.GetFileUrl(),
                                                mediaType = flow.MediaType
                                            }, $"{flow.Owner.UserName} is starting a stream!");

                                            // FIXME do we really need to close those one in invalid state ?
                                            Stack<string> ToClose = new Stack<string>();

                                            try
                                            {
                                                while (received.MessageType != WebSocketMessageType.Close)
                                                {
                                                    logger.LogInformation($"Echoing {received.Count} bytes received in a {received.MessageType} message; Fin={received.EndOfMessage}");
                                                    // Echo anything we receive
                                                    // and send to all listner found
                                                    foreach (var cliItem in meta.Listeners)
                                                    {
                                                        var listenningSocket = cliItem.Value;
                                                        if (listenningSocket.State == WebSocketState.Open)
                                                            await listenningSocket.SendAsync(
                                                            sBuffer, received.MessageType, received.EndOfMessage, CancellationToken.None);
                                                        else
                                                        if (listenningSocket.State == WebSocketState.CloseReceived || listenningSocket.State == WebSocketState.CloseSent)
                                                        {
                                                            ToClose.Push(cliItem.Key);
                                                        }
                                                    }
                                                    received = await meta.Socket.ReceiveAsync(sBuffer, CancellationToken.None);

                                                    string no;
                                                    do
                                                    {
                                                        no = ToClose.Pop();
                                                        WebSocket listenningSocket;
                                                        if (meta.Listeners.TryRemove(no, out listenningSocket))
                                                            await listenningSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "State != WebSocketState.Open", CancellationToken.None);

                                                    } while (no != null);
                                                }
                                                await meta.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "eof", CancellationToken.None);
                                                LiveApiController.Casters[uname] = null;
                                            }
                                            catch (Exception ex)
                                            {
                                                logger.LogError($"Exception occured : {ex.Message}");
                                                logger.LogError(ex.StackTrace);
                                                meta.Socket.Dispose();
                                                LiveApiController.Casters[uname] = null;
                                            }
                                        }
                                        else
                                        { // not meta.Socket != null && meta.Socket.State == WebSocketState.Open
                                            if (meta.Socket != null)
                                            {
                                                logger.LogError($"meta.Socket.State not Open: {meta.Socket.State.ToString()} ");
                                                meta.Socket.Dispose();
                                            }
                                            else
                                                logger.LogError("socket object is null");
                                        }
                                    }
                                    catch (IOException ex)
                                    {
                                        if (ex.Message == "Unexpected end of stream")
                                        {
                                            logger.LogError($"Unexpected end of stream");
                                        }
                                        else
                                        {
                                            logger.LogError($"Really unexpected end of stream");
                                        }
                                        await meta.Socket?.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, ex.Message, CancellationToken.None);
                                        meta.Socket?.Dispose();
                                        LiveApiController.Casters[uname] = null;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        await next();
                    }

                });
            CheckApp(app, SiteSetup, env, loggerFactory);
        }

        // Entry point for the application.
        public static void Main(string[] args) => Microsoft.AspNet.Hosting.WebApplication.Run<Startup>(args);
    }

}
//
