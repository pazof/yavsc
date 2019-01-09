using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Razor;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.WebEncoders;
using Microsoft.Dnx.Compilation;
using Yavsc;
using Yavsc.Models;
using Yavsc.Services;
using cli.Services;
using cli.Settings;
using Microsoft.Extensions.CodeGeneration;
using Microsoft.Dnx.Runtime.Compilation;
using Microsoft.Dnx.Runtime;
using Microsoft.Dnx.Compilation.Caching;
using Microsoft.Dnx.Host;
using System.IO;
using System.Runtime.Versioning;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.CodeGeneration.EntityFramework;
using Microsoft.Extensions.CodeGeneration.Templating.Compilation;
using System.Linq;

namespace cli
{
    public class Startup
    {
        public string ConnectionString
        {
            get; set;
        }

        public static ConnectionSettings Settings { get; private set; }
        public static IConfiguration Configuration { get; set; }

        public static string HostingFullName { get; private set; }

        public static IServiceCollection Services { get; private set; }



        Microsoft.Extensions.Logging.ILogger logger;
        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            var devtag = env.IsDevelopment() ? "D" : "";
            var prodtag = env.IsProduction() ? "P" : "";
            var stagetag = env.IsStaging() ? "S" : "";

            HostingFullName = $"{appEnv.RuntimeFramework.FullName} [{env.EnvironmentName}:{prodtag}{devtag}{stagetag}]";
            // Set up configuration sources.

            var builder = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            Configuration = builder.Build();
            ConnectionString = Configuration["ConnectionStrings:Default"];
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            var cxSettings = Configuration.GetSection("Connection");
            services.Configure<ConnectionSettings>(cxSettings);
            var smtpSettingsconf = Configuration.GetSection("Smtp");
            Microsoft.Extensions.CodeGenerators.Mvc.View.ViewGeneratorTemplateModel v;

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

            services.AddAuthentication();

            services.AddSingleton(typeof(IApplicationEnvironment), svs => PlatformServices.Default.Application);
            services.AddSingleton(typeof(IRuntimeEnvironment), svs => PlatformServices.Default.Runtime);
            services.AddSingleton(typeof(IAssemblyLoadContextAccessor), svs => PlatformServices.Default.AssemblyLoadContextAccessor);
            services.AddSingleton(typeof(IAssemblyLoaderContainer), svs => PlatformServices.Default.AssemblyLoaderContainer);
            services.AddSingleton(typeof(ILibraryManager), svs => PlatformServices.Default.LibraryManager);

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
                    logger.LogInformation($"Found {projects?.Count} projects");
                    return new CompilerOptionsProvider(projects);
                });
            services.AddMvc();
            services.AddTransient(typeof(IPackageInstaller),typeof(PackageInstaller));
            services.AddTransient(typeof(Microsoft.Extensions.CodeGeneration.ILogger),typeof(Microsoft.Extensions.CodeGeneration.ConsoleLogger));

            Services = services;

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
        IOptions<SiteSettings> siteSettings, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            logger = loggerFactory.CreateLogger<Startup>();
            logger.LogInformation(env.EnvironmentName);
            var authConf = Configuration.GetSection("Authentication").GetSection("Yavsc");
            var clientId = authConf.GetSection("ClientId").Value;
            var clientSecret = authConf.GetSection("ClientSecret").Value;

        }

    }
}
