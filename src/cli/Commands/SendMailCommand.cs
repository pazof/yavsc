
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using cli.Services;
using cli.Model;
using Yavsc.Server.Settings;

namespace cli
{
    public class SendMailCommandProvider : ICommander
    {
        public CommandLineApplication Integrate(CommandLineApplication rootApp)
        {
            CommandArgument critCommandArg = null;
            CommandOption sendHelpOption = null;
            CommandLineApplication sendMailCommandApp
               = rootApp.Command("send-monthly",
                   (target) =>
                   {
                       target.FullName = "Send email";
                       target.Description = "Sends monthly emails using given template from code";
                       sendHelpOption = target.HelpOption("-? | -h | --help");
                       critCommandArg = target.Argument(
                           "criteria",
                           "user selection criteria : 'allow-monthly' or 'email-not-confirmed'");
                   }, false);

            sendMailCommandApp.OnExecute(() =>
            {
                bool showhelp = !UserPolicies.Criterias.ContainsKey(critCommandArg.Value)
                || sendHelpOption.HasValue();

                if (!showhelp)
                {
                    var host = new WebHostBuilder();

                    var hostengnine = host.UseEnvironment(Program.HostingEnvironment.EnvironmentName)
                        .UseServer("cli")
                        .UseStartup<Startup>()
                        .Build();
                    var app = hostengnine.Start();
                    var mailer = app.Services.GetService<EMailer>();
                    var loggerFactory = app.Services.GetService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger<Program>();
                    logger.LogInformation("Starting emailling");
                    mailer.SendEmailFromCriteria(critCommandArg.Value);
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
