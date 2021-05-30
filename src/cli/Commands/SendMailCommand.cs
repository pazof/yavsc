
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using cli.Services;
using cli.Model;
using System.Linq;
using Yavsc.Models;
using System.Collections.Generic;
using System;

namespace cli
{
    public class SendMailCommandProvider : ICommander
    {
        readonly Dictionary<string, Func<ApplicationUser, bool>> Criterias =
            new Dictionary<string, Func<ApplicationUser, bool>>
            {
                {"allow-monthly", u => u.AllowMonthlyEmail },
                { "email-not-confirmed", u => !u.EmailConfirmed }
            };
        public CommandLineApplication Integrate(CommandLineApplication rootApp)
        {

            CommandArgument codeCommandArg = null;
            CommandArgument critCommandArg = null;
            CommandOption sendHelpOption = null;
            CommandLineApplication sendMailCommandApp
               = rootApp.Command("send-monthly",
                   (target) =>
                   {
                       target.FullName = "Send email";
                       target.Description = "Sends monthly emails using given template from code";
                       sendHelpOption = target.HelpOption("-? | -h | --help");
                       codeCommandArg = target.Argument(
                       "code",
                       "template code of mailling to execute.");
                       critCommandArg = target.Argument(
                           "criteria",
                           "user selection criteria : 'allow-monthly' or 'email-not-confirmed'");
                   }, false);

            sendMailCommandApp.OnExecute(() =>
            {
                int code;
                bool showhelp = !int.TryParse(codeCommandArg.Value, out code)
                  || Criterias.ContainsKey(critCommandArg.Value);

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
                    mailer.SendEmailFromCriteria(code, Criterias[critCommandArg.Value]);
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
