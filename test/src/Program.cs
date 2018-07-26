using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions;
using Yavsc.Lib;
using Yavsc.Services;
using Yavsc;

namespace test
{
    public class Program
    {
        public Program()
        {
            var host = new WebHostBuilder();

            var hostengnine = host
            .UseEnvironment("Development")
            .UseServer("test")
            .UseStartup<test.Startup>()
            .Build();
            
            var app = hostengnine.Start();

            CommandArgument opName = new CommandArgument()
            {
                Name = "command",
                Description = "command to invoke ('monthlyTasks')",
                MultipleValues = false
            };

            if (opName.Value == "monthlyTasks") {
                CommandOption opMailId = new CommandOption("m", OptionTypes.SingleValue )
                {
                LongName = "mail-id",
                Description = "UserOrientedTemplate template id to use ('1')",
                };

                var sender = app.Services.GetService(typeof(IEmailSender)) as IEmailSender;
                var mailer = app.Services.GetService(typeof(EMailer)) as EMailer;
                var loggerFactory = app.Services.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
                ILogger logger = loggerFactory.CreateLogger<Program>() ;
                
                mailer.SendMonthlyEmail(opMailId.Value,"UserOrientedTemplate");
                logger.LogInformation("Finished");
            }
        }
    }
}
