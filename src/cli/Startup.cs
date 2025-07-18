
using Yavsc;
using cli.Services;
using cli.Settings;
using System.Runtime.Versioning;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;

namespace cli
{
    public class Startup
    {
        public string ConnectionString
        {
            get; set;
        }

        public static ConnectionSettings ConnectionSettings { get; set; }

        public static UserConnectionSettings UserConnectionSettings { get; set; }
        


        public static string HostingFullName { get; private set; }


        public static string EnvironmentName { get; private set; }
        public static Microsoft.Extensions.Logging.ILogger Logger { get; private set; }
        public Startup()
        {
            var devtag = env.IsDevelopment() ? "D" : "";
            var prodtag = env.IsProduction() ? "P" : "";
            var stagetag = env.IsStaging() ? "S" : "";
            EnvironmentName = env.EnvironmentName;

            HostingFullName = $"{appEnv.RuntimeFramework.FullName} [{env.EnvironmentName}:{prodtag}{devtag}{stagetag}]";
           
            // Set up configuration sources.
            UserConnectionsettingsFileName = $"connectionsettings.{env.EnvironmentName}.json";
            var builder = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile(UserConnectionsettingsFileName, optional: true);

            Configuration = builder.Build();
            ConnectionString = Configuration["ConnectionStrings:Default"];
        }

        public static void SaveCredentials(string fileName, bool condensed=false) {
            var cf = new FileInfo(fileName);
            using (var writer = cf.OpenWrite())
            {
                using (var textWriter = new StreamWriter(writer))
                {
                    var data = new { UserConnection = UserConnectionSettings, Connection = ConnectionSettings };
                    var json = JsonConvert.SerializeObject(data, condensed ? Formatting.None : Formatting.Indented);
                    textWriter.Write(json);
                    textWriter.Close();
                }
                writer.Close();
            }
        }

        public static string UserConnectionsettingsFileName { get ; private set;}

        const string userCxKey = "UserConnection";

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            var cxSettings = Configuration.GetSection("Connection");
            services.Configure<ConnectionSettings>(cxSettings);
            var cxUserSettings = Configuration.GetSection(userCxKey);
            services.Configure<UserConnectionSettings>(cxUserSettings);

            var smtpSettingsconf = Configuration.GetSection("Smtp");
            // TODO give it a look : Microsoft.Extensions.CodeGenerators.Mvc.View.ViewGeneratorTemplateModel v;

            services.Configure<SmtpSettings>(smtpSettingsconf);
            services.Configure<GenMvcSettings>(Configuration.GetSection("gen_mvc"));
            services.AddInstance(typeof(ILoggerFactory), new LoggerFactory());
            services.AddTransient(typeof(IEmailSender), typeof(MailSender));
            services.AddTransient(typeof(RazorEngineHost), 
             svs => { 
                 var settings = svs.GetService<GenMvcSettings>();

                 return new YaRazorEngineHost {
                DefaultBaseClass = "Microsoft.Extensions.CodeGenerators.Mvc.View.ViewGeneratorTemplateModel",
                DefaultClassName = settings.ControllerName,
                DefaultNamespace = settings.NameSpace }; }
            );
            // Well ... I'll perhaps have, one day, enough trust to use it ...
            services.AddTransient(typeof(MvcGenerator), typeof(MvcGenerator));
            services.AddEntityFramework().AddNpgsql().AddDbContext<ApplicationDbContext>(
                  db => db.UseNpgsql(ConnectionString)
              );

            services.AddTransient((s) => new RazorTemplateEngine(s.GetService<RazorEngineHost>()));
            services.AddTransient(typeof(IModelTypesLocator), typeof(ModelTypesLocator));
            services.AddTransient(typeof(ILibraryExporter), typeof(RuntimeLibraryExporter));
            services.AddLogging();
            services.AddTransient<EMailer>();
            services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });
            services.Configure<SharedAuthenticationOptions>(options =>
            {
                options.SignInScheme = "Bearer";
            });

            services.AddTransient<Microsoft.Extensions.WebEncoders.UrlEncoder, UrlEncoder>();

            services.AddAuthentication("Bearer");

            services.AddSingleton(typeof(IApplicationEnvironment), svs => PlatformServices.Default.Application);
            services.AddSingleton(typeof(IRuntimeEnvironment), svs => PlatformServices.Default.Runtime);
            services.AddSingleton(typeof(IAssemblyLoadContextAccessor), svs => PlatformServices.Default.AssemblyLoadContextAccessor);
            services.AddSingleton(typeof(IAssemblyLoaderContainer), svs => PlatformServices.Default.AssemblyLoaderContainer);
            services.AddSingleton(typeof(ILibraryManager), svs => PlatformServices.Default.LibraryManager);
            services.AddSingleton<UserManager<ApplicationDbContext>>();

            services.AddSingleton(typeof(BootstrapperContext),
                svs => new BootstrapperContext
                {
                    RuntimeDirectory = Path.GetDirectoryName(typeof(BootstrapperContext).Assembly.Location),
                    ApplicationBase = Configuration["gen_mvc:AppBase"],
                    // NOTE(anurse): Mono is always "dnx451" (for now).
                    TargetFramework = new FrameworkName("DNX", new Version(4, 5, 1)),
                    RuntimeType = "Mono"
                }
            );

            services.AddTransient(typeof(CompilationEngine),
              svs => {
                  var logger =  svs.GetService<ILoggerFactory>().CreateLogger<Startup>();
                  var project = svs.GetService<Project>();
                  var env = new ApplicationEnvironment(project,
                    PlatformServices.Default.Application.RuntimeFramework,
                    PlatformServices.Default.Application.Configuration,
                    PlatformServices.Default.Application
                  );
                  logger.LogInformation("app name: "+env.ApplicationName);

                  return new CompilationEngine(
              new CompilationEngineContext(PlatformServices.Default.Application,
                PlatformServices.Default.Runtime,
                PlatformServices.Default.AssemblyLoadContextAccessor.Default, new CompilationCache()));
              });

            services.AddTransient(typeof(IFrameworkReferenceResolver),
                    svs => new FrameworkReferenceResolver());

            services.AddTransient(
                typeof(Project), svs =>
               {
                   Project project = null;
                   var diag = new List<DiagnosticMessage>();
                   var settings = svs.GetService<IOptions<GenMvcSettings>>();
                   if (Project.TryGetProject(settings.Value.AppBase, out project, diag))
                   {
                       return project;
                   }
                   return null;
               }
            );

            services.AddTransient(
                typeof(ILibraryExporter),
                svs =>
                {
                    var settings = svs.GetService<IOptions<GenMvcSettings>>();
                    var compilationEngine = svs.GetService<CompilationEngine>();
                    var bootstrappercontext = svs.GetService<BootstrapperContext>();
                    var project = svs.GetService<Project>();
                    if (settings == null)
                        throw new Exception("settings are missing to generate some server code (GenMvcSettings) ");

                    return
                    new RuntimeLibraryExporter(
                        () => compilationEngine.CreateProjectExporter
                        (project, bootstrappercontext.TargetFramework, settings.Value.ConfigurationName));

                }

            );
            services.AddTransient(typeof(IEntityFrameworkService), typeof(EntityFrameworkServices));
            services.AddTransient(typeof(IDbContextEditorServices), typeof(DbContextEditorServices));
            services.AddTransient(typeof(IFilesLocator), typeof(FilesLocator));
            services.AddTransient(typeof(
                Microsoft.Extensions.CodeGeneration.Templating.ITemplating), typeof(
                Microsoft.Extensions.CodeGeneration.Templating.RazorTemplating));
            services.AddTransient(
                typeof(Microsoft.Extensions.CodeGeneration.Templating.Compilation.ICompilationService),
                typeof(Microsoft.Extensions.CodeGeneration.Templating.Compilation.RoslynCompilationService));

services.AddTransient(
                typeof(
Microsoft.Extensions.CodeGeneration.ICodeGeneratorActionsService),
                typeof(Microsoft.Extensions.CodeGeneration.CodeGeneratorActionsService));


            services.AddTransient(typeof(Microsoft.Dnx.Compilation.ICompilerOptionsProvider),
                svs =>
                {
                    var bsContext = svs.GetService<BootstrapperContext>();
                    var compileEngine = svs.GetService<ICompilationEngine>();

                    var applicationHostContext = new ApplicationHostContext
                    {
                        ProjectDirectory = bsContext.ApplicationBase,
                        RuntimeIdentifiers = new string[] { "dnx451" },
                        TargetFramework = bsContext.TargetFramework
                    };

                    var libraries = ApplicationHostContext.GetRuntimeLibraries(applicationHostContext, throwOnInvalidLockFile: true);
                    var projects = libraries.Where(p => p.Type == LibraryTypes.Project)
                                            .ToDictionary(p => p.Identity.Name, p => (ProjectDescription)p);
                    Logger.LogInformation($"Found {projects?.Count} projects");
                    return new CompilerOptionsProvider(projects);
                });
            services.AddMvc();
            services.AddTransient(typeof(IPackageInstaller),typeof(PackageInstaller));
            services.AddTransient(typeof(Microsoft.Extensions.CodeGeneration.ILogger),typeof(Microsoft.Extensions.CodeGeneration.ConsoleLogger));

            services.AddIdentity<ApplicationUser, IdentityRole>(
                    option =>
                    {
                        option.User.RequireUniqueEmail = true;
                    }
                ).AddEntityFrameworkStores<ApplicationDbContext>();
            Services = services;
        }

        public void Configure(
        IOptions<ConnectionSettings> cxSettings,
        IOptions<UserConnectionSettings> useCxSettings,
        ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            Logger = loggerFactory.CreateLogger<Startup>();
            var authConf = Configuration.GetSection("Authentication").GetSection("Yavsc");
            var clientId = authConf.GetSection("ClientId").Value;
            var clientSecret = authConf.GetSection("ClientSecret").Value;
            ConnectionSettings = cxSettings?.Value ?? new ConnectionSettings();
            UserConnectionSettings = useCxSettings?.Value ?? new UserConnectionSettings();
            Logger.LogInformation($"Configuration ended, with hosting Full Name: {HostingFullName}");
        }
    }
}
