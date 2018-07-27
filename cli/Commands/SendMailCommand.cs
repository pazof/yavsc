
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using cli.Services;

namespace cli {
    
public class SendMailCommandProvider {
        public static CommandLineApplication Handle(CommandLineApplication rootApp)
        {
         CommandArgument _sendMailCommandArg = null;
         CommandOption _sendHelpOption = null;
         CommandLineApplication sendMailCommandApp 
            = rootApp.Command("send",
                (target) =>
                {
                    target.FullName = "Send email";
                    target.Description = "Sends emails using given template";
                    _sendHelpOption = target.HelpOption("-? | -h | --help");
                    _sendMailCommandArg = target.Argument(
                    "class",
                    "class name of mailling to execute (actually, only 'monthly') .",
                    multipleValues: true);
                }, false);

            sendMailCommandApp.OnExecute(() =>
            {
                if (_sendMailCommandArg.Value == "monthly")
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
                }
                return 0;
            });
            return sendMailCommandApp;
        }
    }
}