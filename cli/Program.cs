using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Identity;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Yavsc.Models;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Builder.Internal;
using Microsoft.Extensions.PlatformAbstractions;

namespace cli
{

public class Program
{
    private readonly EventLog _log =
           new EventLog("Application") { Source = "Application" };

    public Program()
    {
    }

    public static void Main(string[] args)
    {
        var prog = new Program();

        // Program.exe <-g|--greeting|-$ <greeting>> [name <fullname>]
        // [-?|-h|--help] [-u|--uppercase]
        CommandLineApplication commandLineApplication =
          new CommandLineApplication(throwOnUnexpectedArg: false);
        CommandArgument names = null;
        commandLineApplication.Command("name",
          (target) =>
            names = target.Argument(
              "fullname",
              "Enter the full name of the person to be greeted.",
              multipleValues: true));
        CommandOption greeting = commandLineApplication.Option(
          "-$|-g |--greeting <greeting>",
          "The greeting to display. The greeting supports"
          + " a format string where {fullname} will be "
          + "substituted with the full name.",
          CommandOptionType.SingleValue);
          CommandOption envNameOption = commandLineApplication.Option(
          "-e |--environment <environment>","The environment to run against.",CommandOptionType.SingleValue);
        CommandOption uppercase = commandLineApplication.Option(
          "-u | --uppercase", "Display the greeting in uppercase.",
          CommandOptionType.NoValue);
        commandLineApplication.HelpOption("-? | -h | --help");
        commandLineApplication.OnExecute(() =>
        {
            string greetingString = "Hello!";
            string environmentName = "Production";
            if (greeting.HasValue())
            {

                string greetingStringTemplate = greeting.Value();

                var fullname = string.Join(" ", commandLineApplication.RemainingArguments);
                greetingString = greetingStringTemplate.Replace("{fullname}", fullname);
            } 
            if (envNameOption.HasValue()){
                environmentName = envNameOption.Value();
            }
            Greet(greetingString, environmentName);
            return 0;
        });
        commandLineApplication.Execute(args);
    }
    
    private static void Greet(
      string greeting, string environmentName)
    {
        Console.WriteLine(greeting);
        IServiceCollection services = new ServiceCollection();
        // Startup.cs finally :)

		//EntryPoint.Main(new string[] {});

        IHostingEnvironment hosting = new HostingEnvironment{ EnvironmentName = environmentName };

			var basePath = AppDomain.CurrentDomain.BaseDirectory;
			// FIXME null ref var appName = AppDomain.CurrentDomain.ApplicationIdentity.FullName;

			// var rtdcontext = new System.Runtime.DesignerServices.WindowsRuntimeDesignerContext (new string { "." }, "nonname");

			// hosting.Initialize("approot", config);

			// ApplicationHostContext apphostcontext = new ApplicationHostContext ();
			IServiceProvider serviceProvider = services.BuildServiceProvider();
			IApplicationBuilder iappbuilder = new ApplicationBuilder(serviceProvider);
			iappbuilder.ApplicationServices = serviceProvider;


			Startup startup = new Startup(hosting, PlatformServices.Default.Application);

        //configure console logging
        serviceProvider
            .GetService<ILoggerFactory>()
            .AddConsole(LogLevel.Debug);
		
        var logger = serviceProvider.GetService<ILoggerFactory>()
            .CreateLogger<Program>();

        logger.LogDebug("Logger is working!");
        // Get Service and call method
        var userManager = serviceProvider.GetService<UserManager<ApplicationUser> >();
        var emailSender = serviceProvider.GetService<IEmailSender>();

        foreach (var user in userManager?.Users)
        {
            Task.Run(async () => 
            await emailSender.SendEmailAsync(Startup.SiteSetup, Startup.SmtpSettup, Startup.SiteSetup.Owner.Name, Startup.SiteSetup.Owner.EMail,
                     $"[{Startup.SiteSetup.Title}]  Rappel de votre inscription ({user.UserName})", $"{user.Id}/{user.UserName}/{user.Email}"));

        }


    }
}

}
