using System;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Yavsc.Lib;
using Yavsc.Services;
using Yavsc;
using Xunit;

namespace test
{
    [Trait("regres", "no")]
    public class ServerSideFixture : IDisposable { 
        public SiteSettings _siteSetup;
        public ILogger _logger;
        public IApplication _app;
        public EMailer _mailer;
        public ILoggerFactory _loggerFactory;
        public IEmailSender _mailSender;
        public static string ApiKey  => "53f4d5da-93a9-4584-82f9-b8fdf243b002" ;
        // 
        public ServerSideFixture()
        {
            InitTestHost();
            _logger = _loggerFactory.CreateLogger<ServerSideFixture> ();
            _logger.LogInformation("ServerSideFixture");
        }

        [Fact]
        void InitTestHost()
        {
            var host = new WebHostBuilder();

            var hostengnine = host
            .UseEnvironment("Development")
            .UseServer("test")
            .UseStartup<test.Startup>()
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
}

