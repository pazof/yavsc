using System;
using System.Data.Common;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Yavsc.Lib;
using Yavsc.Services;
using Yavsc;
using Yavsc.Models;
using Xunit;
using Npgsql;

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

        public ApplicationDbContext DbContext { get; private set;  } 
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

        internal void UpgradeDb()
        {
            Microsoft.Data.Entity.Commands.Program.Main(
                new string [] { "database", "update" });
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
        public bool DbCreated { get; private set; }


        // 
        public ServerSideFixture()
        {
            InitTestHost();
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
           
            Logger = _loggerFactory.CreateLogger<ServerSideFixture> ();
            var siteSetup = App.Services.GetService(typeof(IOptions<SiteSettings>)) as IOptions<SiteSettings>;
            SiteSetup = siteSetup.Value;
            MailSender = App.Services.GetService(typeof(IEmailSender)) as IEmailSender;

            var builder = new DbConnectionStringBuilder();
            builder.ConnectionString = Startup.DbSettings.Testing;
            Logger.LogInformation("testing database:" +builder["Database"]);
            TestingDatabase = (string) builder["Database"];

            CheckDbExistence();
            if (!DbCreated)
                CreateTestDb();
            DbContext = App.Services.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;

        }

        public string TestingDatabase { get; private set; }

        public void CheckDbExistence()
        {
            using (
            NpgsqlConnection cx = new NpgsqlConnection(Startup.DbSettings.DatabaseCtor))
            {
                cx.Open();
                var command = cx.CreateCommand();
                command.CommandText = $"SELECT 1 FROM pg_database WHERE datname='{TestingDatabase}';";
                DbCreated = (command.ExecuteScalar()!=null);

                _logger.LogInformation($"DbCreated:{DbCreated}");
                cx.Close();
            }
        }
        public void CreateTestDb()
        {
            if (!DbCreated)
            using (
            NpgsqlConnection cx = new NpgsqlConnection(Startup.DbSettings.DatabaseCtor))
            {
                cx.Open();
                var command = cx.CreateCommand();
                command.CommandText = $"create database \"{TestingDatabase}\";";
                command.ExecuteNonQuery();

                _logger.LogInformation($"database created.");
                cx.Close();
            }
            DbCreated=true;
        }
        public void DropTestDb()
        {
            if (DbCreated)
            using (
            NpgsqlConnection cx = new NpgsqlConnection(Startup.DbSettings.DatabaseCtor))
            {
                cx.Open();
                var command = cx.CreateCommand();
                command.CommandText = $"drop database \"{TestingDatabase}\"";
                command.ExecuteNonQuery();
                _logger.LogInformation($"database dropped");
                cx.Close();
            }
            DbCreated=false;
        } 
          
        public void Dispose()
        {
            Logger.LogInformation("Disposing");
        }
    }
}


