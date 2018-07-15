using System;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Xunit;
using Xunit.Abstractions;
using Yavsc.Abstract.Manage;
using Yavsc.Lib;
using Yavsc.Services;

namespace Yavsc.test
{


    public class ServerSideFixture : IDisposable { 
        public SiteSettings _siteSetup;
        public ILogger _logger;
        public IApplication _app;
        public EMailer _mailer;
        public ILoggerFactory _loggerFactory;
        public IEmailSender _mailSender;

        public ServerSideFixture()
        {
            InitServices();
            _logger = _loggerFactory.CreateLogger<ServerSideFixture> ();
            _logger.LogInformation("ServerSideFixture");
        }
        void InitServices()
        {
            var host = new WebHostBuilder();

            var hostengnine = host
            .UseEnvironment("Development")
            .UseServer("test")
            .UseStartup<Startup>()
            .Build();

            _app = hostengnine.Start();
            _mailer = _app.Services.GetService(typeof(EMailer)) as EMailer;
            _loggerFactory = _app.Services.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            var siteSetup = _app.Services.GetService(typeof(IOptions<SiteSettings>)) as IOptions<SiteSettings>;
            _siteSetup = siteSetup.Value;
            _mailSender = _app.Services.GetService(typeof(IEmailSender)) as IEmailSender;
        }

        public void Dispose()
        {
            _logger.LogInformation("Disposing");
        }
    }


    [Collection("EMaillingTeststCollection")]
    public class EMaillingTests : IClassFixture<ServerSideFixture>

    {
        ServerSideFixture _serverFixture;
        ITestOutputHelper output;
        public EMaillingTests(ServerSideFixture serverFixture, ITestOutputHelper output)
        {
            this.output = output;
            _serverFixture = serverFixture;
        }

        void InitServices()
        {
            var host = new WebHostBuilder();

            var hostengnine = host
            .UseEnvironment("Development")
            .UseServer("test")
            .UseStartup<Startup>()
            .Build();

            var app = hostengnine.Start();
            var sender = app.Services.GetService(typeof(IEmailSender)) as IEmailSender;
            var mailer = app.Services.GetService(typeof(EMailer)) as EMailer;
            var loggerFactory = app.Services.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            ILogger logger = loggerFactory.CreateLogger<Program>() ;
            mailer.SendMonthlyEmail(1,"UserOrientedTemplate");
            logger.LogInformation("Finished");
        }

        [Fact]

        public void SendEMailSynchrone()
        {
            output.WriteLine("SendEMailSynchrone ...");
            EmailSentViewModel mailSentInfo = _serverFixture._mailSender.SendEmailAsync
                                (_serverFixture._siteSetup.Owner.Name, _serverFixture._siteSetup.Owner.EMail, $"monthly email", "test boby monthly email").Result;
                if (mailSentInfo==null) 
                    _serverFixture._logger.LogError("No info on sending");
                else if (!mailSentInfo.Sent)
                    _serverFixture._logger.LogError($"{mailSentInfo.ErrorMessage}");
                else 
                    _serverFixture._logger.LogInformation($"mailId:{mailSentInfo.MessageId} \nto:{_serverFixture._siteSetup.Owner.Name}");
            Assert.NotNull(mailSentInfo);
            output.WriteLine($">>done with {mailSentInfo.EMail} {mailSentInfo.Sent} {mailSentInfo.MessageId} {mailSentInfo.ErrorMessage}");
        }
    }
}