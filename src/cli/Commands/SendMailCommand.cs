using cli.Model;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Yavsc.Services;

namespace cli
{
    public class SendMailCommandProvider : ICommander
    {
        MailSender emailer;
        private ILogger<SendMailCommandProvider> logger;
        public SendMailCommandProvider(MailSender emailer, ILoggerFactory loggerFactory)
        {
            this.emailer = emailer;
            this.logger = loggerFactory.CreateLogger<SendMailCommandProvider>();
        }
        public CommandLineApplication Integrate(CommandLineApplication rootApp)
        {
            CommandOption sendHelpOption = null;
            CommandLineApplication sendMailCommandApp
               = rootApp.Command("send-email",
                   (target) =>
                   {
                       target.FullName = "Send email";
                       target.Description = "Sends emails using given template from code";
                       sendHelpOption = target.HelpOption("-? | -h | --help");
                   }, false);

            sendMailCommandApp.OnExecute(() =>
            {
                bool showhelp = sendHelpOption.HasValue();

                if (!showhelp)
                {
                    logger.LogInformation("Starting emailling");
                    emailer.SendEmailFromCriteria("allow-monthly");
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
