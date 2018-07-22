using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
// using Microsoft.AspNet.Authorization;
// using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Hosting;
using cli.Services;

namespace cli
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder();

            var hostengnine = host
            .UseEnvironment("Development")
            .UseServer("cli")
            .UseStartup<Startup>()
            .Build();

            var app = hostengnine.Start();
            var mailer = app.Services.GetService<EMailer>();
            var loggerFactory = app.Services.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<Program>();
            mailer.SendMonthlyEmail(1,"UserOrientedTemplate");
            logger.LogInformation("Finished");
        }
    }
}
