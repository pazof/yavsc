using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
// using Microsoft.AspNet.Authorization;
// using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Hosting;
using cli.Services;
using Microsoft.Extensions.CommandLineUtils;
using System;

namespace cli
{
    public class Program
    {
        public static void Main(string[] args)
        {

            CommandArgument sendMailCommandArg=null;
            CommandLineApplication sendMailCommand=null;
            CommandOption sendHelpOption=null;
            CommandOption rootCommandHelpOption = null;
            
            CommandLineApplication cliapp = new CommandLineApplication(false);
            cliapp.Name  = "cli";
            cliapp.FullName = "Yavsc command line interface";
            cliapp.Description  = "Dnx console for yavsc server side";
            cliapp.ShortVersionGetter = () => "v1.0";
            cliapp.LongVersionGetter =  () => "version 1.0 (stable)";
            rootCommandHelpOption = cliapp.HelpOption("-? | -h | --help");
            
            sendMailCommand = cliapp.Command("send",
                (target) => {
                    target.FullName="Send email";
                    target.Description="Sends emails using given template";
                    sendHelpOption = target.HelpOption("-? | -h | --help");
                    sendMailCommandArg = target.Argument(
                    "class",
                    "class name of mailling to execute (actually, only 'monthly') .",
                    multipleValues: true);
                }, false);

            sendMailCommand.OnExecute(() =>
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
                    var logger = loggerFactory.CreateLogger<Program>();
                    logger.LogInformation("Starting emailling");
                    mailer.SendMonthlyEmail(1, "UserOrientedTemplate");
                    logger.LogInformation("Finished emailling");
                }
                else 
                {
                    sendMailCommand.ShowHelp();
                }
                return 0;
            });

            cliapp.Execute(args);

            if (args.Length==0 || cliapp.RemainingArguments.Count>0)
                cliapp.ShowHint();
        }
    }
}
