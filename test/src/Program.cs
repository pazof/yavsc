using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Logging;
using Yavsc.Lib;
using Yavsc.Services;

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
            var sender = app.Services.GetService(typeof(IEmailSender)) as IEmailSender;
            var mailer = app.Services.GetService(typeof(EMailer)) as EMailer;
            var loggerFactory = app.Services.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            ILogger logger = loggerFactory.CreateLogger<Program>() ;
            mailer.SendMonthlyEmail(1,"UserOrientedTemplate");
            logger.LogInformation("Finished");
        }
    }
}
