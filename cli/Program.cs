



using System;
using System.Runtime.Versioning;
using Google.Apis.Util.Store;
using Microsoft.AspNet.Builder.Internal;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Hosting.Builder;
using Microsoft.AspNet.Hosting.Internal;
using Microsoft.AspNet.Server;

using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.PlatformAbstractions;
using Yavsc;
using Yavsc.Models;

using Yavsc.Services;
using cli_2;

public class Program 
{
    private IServiceProvider serviceProvider;
    private static Startup startup;

    public Program()
    {

        ConfigureServices(new ServiceCollection());

    } 

    private void ConfigureServices(IServiceCollection services, string environmentName="Development")
    {

        IHostingEnvironment hosting = new HostingEnvironment{ EnvironmentName = environmentName };
        
        services.Add(new ServiceDescriptor(typeof(IHostingEnvironment), hosting ));
        

            services.AddLogging();

            services.AddOptions();


            // Add application services.
            services.AddTransient<IEmailSender, MessageSender>();
            services.AddTransient<IGoogleCloudMessageSender, MessageSender>();
            services.AddTransient<IBillingService, BillingService>();
            services.AddTransient<IDataStore, FileDataStore>( (sp) => new FileDataStore("googledatastore",false) );
            services.AddTransient<ICalendarManager, CalendarManager>();
             
            // TODO for SMS: services.AddTransient<ISmsSender, AuthMessageSender>();

            services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });
            
            services.AddIdentity<ApplicationUser,IdentityRole>();
			var basePath = AppDomain.CurrentDomain.BaseDirectory;
			// FIXME null ref  var appName = AppDomain.CurrentDomain.ApplicationIdentity.FullName;

			// var rtdcontext = new WindowsRuntimeDesignerContext (new string [] { "../Yavsc" }, "Yavsc");
            
            
			 serviceProvider = services.BuildServiceProvider();

            var projectRoot = "/home/paul/workspace/yavsc/Yavsc";

			hosting.Initialize (projectRoot, null);

			var targetFramework = new FrameworkName ("dnx",new Version(4,5,1));
			//  
			// 
            ApplicationEnvironment appEnv = new ApplicationEnvironment(targetFramework, projectRoot);
            
			//configure console logging
            // needs a logger factory ...
			var loggerFactory = serviceProvider
				.GetService<ILoggerFactory>()
				.AddConsole(LogLevel.Verbose);
  startup = new Startup (hosting, appEnv);
         


        ApplicationBuilderFactory applicationBuilderFactory
         = new ApplicationBuilderFactory(serviceProvider);
    
        var builder = applicationBuilderFactory.CreateBuilder(null);
        
        startup.ConfigureServices(services);

        builder.ApplicationServices = serviceProvider;
           IOptions<SiteSettings> siteSettings = serviceProvider.GetService(typeof(IOptions<SiteSettings>)) as IOptions<SiteSettings>;
            IOptions<SmtpSettings> smtpSettings = serviceProvider.GetService(typeof(IOptions<SmtpSettings>)) as IOptions<SmtpSettings>;
          
           startup.Configure(builder, loggerFactory,  siteSettings, smtpSettings);
        
    }


    public  static void Main(string[] args)
    {
        Console.WriteLine($"Hello world!" );
        foreach (var str in args) {
            Console.WriteLine($"*> {str}");
        }
        
        startup.Main(args);
    } 
}

