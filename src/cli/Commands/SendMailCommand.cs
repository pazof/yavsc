
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using cli.Services;
using cli.Model;

namespace cli {
    
public class SendMailCommandProvider : ICommander {
        public CommandLineApplication Integrate(CommandLineApplication rootApp)
        {
         CommandArgument sendMailCommandArg = null;
         CommandOption sendHelpOption = null;
         CommandLineApplication sendMailCommandApp 
            = rootApp.Command("send",
                (target) =>
                {
                    target.FullName = "Send email";
                    target.Description = "Sends emails using given template";
                    sendHelpOption = target.HelpOption("-? | -h | --help");
                    sendMailCommandArg = target.Argument(
                    "class",
                    "class name of mailling to execute (actually, only 'monthly') .",
                    multipleValues: true);
                }, false);

            sendMailCommandApp.OnExecute(() =>
            {
                if (sendMailCommandArg.Value == "monthly")
                {
                    var host = new WebHostBuilder();
                    var hostengnine = host.UseEnvironment("Development")
                        .UseServer("cli")
                        .UseStartup<Startup>()
                        .Build();
                    var app = hostengnine.Start();
                    var mailer = app.Services.GetService<EMailer>();
                    var loggerFactory = app.Services.GetService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger<cli.Program>();
                    logger.LogInformation("Starting emailling");
                    mailer.SendMonthlyEmail(1, "UserOrientedTemplate");
                    logger.LogInformation("Finished emailling");
                }
                else
                {
                    sendMailCommandApp.ShowHelp();
                    return 1;
                }
                return 0;
            });
            return sendMailCommandApp;
        }
    }
}