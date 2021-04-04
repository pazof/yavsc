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
using test.Settings;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata.Conventions;

namespace test
{
    [Trait("regression", "II")]
    public class ServerSideFixture : IDisposable
    {
        SiteSettings _siteSetup;
        ILogger _logger;
        IApplication _app;
        readonly EMailer _mailer;
        readonly ILoggerFactory _loggerFactory;
        IEmailSender _mailSender;

        public static string ApiKey => "53f4d5da-93a9-4584-82f9-b8fdf243b002";

        public ApplicationDbContext DbContext { get; private set; }
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

        /// <summary>
        /// initialized by Init
        /// </summary>
        /// <value></value>
        public static object TestingSetup { get; private set; }

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
                new string[] { "database", "update" });
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
        bool dbCreated;
        private readonly WebHostBuilder host;
        private readonly IHostingEngine hostengnine;


        void AssertNotNull(object obj, string msg)
        {
            if (obj == null)
                throw new Exception(msg);
        }
        
        // 
        public ServerSideFixture()
        {
             host = new WebHostBuilder();
            AssertNotNull(host, nameof(host));

            hostengnine = host
            .UseEnvironment("Development")
            .UseServer("test")  
            .UseStartup<test.Startup>()
            .Build();
            
            AssertNotNull(hostengnine, nameof(hostengnine));

            App = hostengnine.Start();
            
            AssertNotNull(App, nameof(App));

            //  hostengnine.ApplicationServices

            _mailer = App.Services.GetService(typeof(EMailer)) as EMailer;
            AssertNotNull(_mailer, nameof(_mailer));
            MailSender = App.Services.GetService(typeof(IEmailSender)) as IEmailSender;
            AssertNotNull(MailSender, nameof(MailSender));

            _loggerFactory = App.Services.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            AssertNotNull(_loggerFactory, nameof(_loggerFactory));
            var siteSetup = App.Services.GetService(typeof(IOptions<SiteSettings>)) as IOptions<SiteSettings>;
            AssertNotNull(siteSetup, nameof(siteSetup));
            var testingSetup = App.Services.GetService(typeof(IOptions<Testing>)) as IOptions<Testing>;
            DbContext = App.Services.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;

            SiteSetup = siteSetup.Value;
            TestingSetup = testingSetup.Value;


            Logger = _loggerFactory.CreateLogger<ServerSideFixture>();

            var builder = new DbConnectionStringBuilder
            {
                ConnectionString = Startup.Testing.ConnectionStrings.Default
            };
            ConventionSet conventions = new ConventionSet();

            modelBuilder = new ModelBuilder(conventions);
            ApplicationDbContext context = new ApplicationDbContext();
            


            TestingDatabase = (string)builder["Database"];
            
            Logger.LogInformation("ServerSideFixture created.");
        }


        private readonly ModelBuilder modelBuilder;

        public string TestingDatabase { get; private set; }

        public void CheckDbExistence()
        {
            using (
            NpgsqlConnection cx = new NpgsqlConnection(Startup.Testing.ConnectionStrings.DatabaseCtor))
            {
                cx.Open();
                _logger.LogInformation($"check db for TestingDatabase:{TestingDatabase}");
                var command = cx.CreateCommand();
                command.CommandText = $"SELECT 1 FROM pg_database WHERE datname='{TestingDatabase}';";
                dbCreated = (command.ExecuteScalar()!=null);
                _logger.LogInformation($"DbCreated:{dbCreated}");
                cx.Close();
            }
        }

        public bool EnsureTestDb()
        {
            if (!DbCreated) 
            {
                 using (NpgsqlConnection cx = new NpgsqlConnection(Startup.Testing.ConnectionStrings.DatabaseCtor))
                {
                _logger.LogInformation($"create database for TestingDatabase : {TestingDatabase}");

                    cx.Open();
                    var command = cx.CreateCommand();
                    using (NpgsqlConnection ownercx =  new NpgsqlConnection(Startup.Testing.ConnectionStrings.Default))
                    command.CommandText = $"create database \"{TestingDatabase}\" OWNER \"{ownercx.UserName}\";";
                    _logger.LogInformation(command.CommandText);
                    command.ExecuteNonQuery();
                }
                dbCreated = true;

            }
            return dbCreated;
        }

        public void DropTestDb()
        {
            if (dbCreated)
                DbContext.Database.EnsureDeleted();
            dbCreated = false;
        }

        public void Dispose()
        {
            Logger.LogInformation("Disposing");
        }

        public bool DbCreated { get { 
            CheckDbExistence();
            return dbCreated; } }
    }
}


