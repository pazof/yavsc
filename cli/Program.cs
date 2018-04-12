



using System;
using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;
using Microsoft.AspNet.Builder.Internal;
using Yavsc.Services;
using Google.Apis.Util.Store;
using cli_2;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNet.Hosting;
using Yavsc.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Extensions.OptionsModel;
using Microsoft.AspNet.Hosting.Builder;

using Yavsc;
using Microsoft.AspNet.Hosting.Internal;

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
			// FIXME null ref var appName = AppDomain.CurrentDomain.ApplicationIdentity.FullName;

			// var rtdcontext = new System.Runtime.DesignerServices.WindowsRuntimeDesignerContext (new string { "." }, "nonname");
            

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
				.AddConsole(LogLevel.Debug);

		    startup = new Startup (hosting, appEnv);
            startup.ConfigureServices(services);


        ApplicationBuilderFactory applicationBuilderFactory
         = new ApplicationBuilderFactory(serviceProvider);
    
        var builder = applicationBuilderFactory.CreateBuilder(null);


            IOptions<SiteSettings> siteSettings = serviceProvider.GetService(typeof(IOptions<SiteSettings>)) as IOptions<SiteSettings>;
            IOptions<SmtpSettings> smtpSettings = serviceProvider.GetService(typeof(IOptions<SmtpSettings>)) as IOptions<SmtpSettings>;

            startup.Configure(builder,siteSettings,smtpSettings);

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

