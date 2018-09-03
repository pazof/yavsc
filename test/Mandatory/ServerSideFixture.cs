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
        SiteSettings _siteSetup;
        ILogger _logger;
        IApplication _app;
        EMailer _mailer;
        ILoggerFactory _loggerFactory;
        IEmailSender _mailSender;
        public static string ApiKey  => "53f4d5da-93a9-4584-82f9-b8fdf243b002" ;

        public SiteSettings SiteSetup
        {
            get
            {
                return _siteSetup;
            }

            set
            {
                _siteSetup = value;
            }
        }

        public IEmailSender MailSender
        {
            get
            {
                return _mailSender;
            }

            set
            {
                _mailSender = value;
            }
        }

        public IApplication App
        {
            get
            {
                return _app;
            }

            set
            {
                _app = value;
            }
        }

        public ILogger Logger
        {
            get
            {
                return _logger;
            }

            set
            {
                _logger = value;
            }
        }




        // 
        public ServerSideFixture()
        {
            InitTestHost();
            Logger = _loggerFactory.CreateLogger<ServerSideFixture> ();
            Logger.LogInformation("ServerSideFixture created.");
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

            App = hostengnine.Start();
            _mailer = App.Services.GetService(typeof(EMailer)) as EMailer;
            _loggerFactory = App.Services.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            var siteSetup = App.Services.GetService(typeof(IOptions<SiteSettings>)) as IOptions<SiteSettings>;
            SiteSetup = siteSetup.Value;
            MailSender = App.Services.GetService(typeof(IEmailSender)) as IEmailSender;
        }

        public void Dispose()
        {
            Logger.LogInformation("Disposing");
        }
    }
}


