using System;
using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Yavsc.Lib;
using Yavsc.Services;
using Yavsc;
using Yavsc.Models;
using Xunit;
using Npgsql;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata.Conventions;
using yavscTests.Settings;
using System.Threading.Tasks;
using System.IO;

namespace yavscTests
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

        public  string ApiKey { get; private set; }

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
        public TestingSetup TestingSetup { get; private set; }

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



        internal int UpgradeDb()
        {
            return Microsoft.Data.Entity.Commands.Program.Main(
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
        public WebHostBuilder Host { get; private set; }
        private readonly IHostingEngine hostengnine;


        void AssertNotNull(object obj, string msg)
        {
            if (obj == null)
                throw new Exception(msg);
        }
        
        // 
        public ServerSideFixture()
        {
             Host = new WebHostBuilder();
            AssertNotNull(Host, nameof(Host));

            hostengnine = Host
            .UseEnvironment("Testing")
            .UseServer("yavscTests")
            .UseStartup<Startup>()
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

            var testingSetup = App.Services.GetService(typeof(IOptions<TestingSetup>)) as IOptions<TestingSetup>;
            AssertNotNull(testingSetup, nameof(testingSetup));

            DbContext = App.Services.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;

            SiteSetup = siteSetup.Value;
            AssertNotNull(SiteSetup, nameof(SiteSetup));

            TestingSetup = testingSetup.Value;
            AssertNotNull(TestingSetup, nameof(TestingSetup));

            Logger = _loggerFactory.CreateLogger<ServerSideFixture>();
            AssertNotNull(Logger, nameof(Logger));

            var builder = new DbConnectionStringBuilder
            {
                ConnectionString = Startup.TestingSetup.ConnectionStrings.Default
            };
            ConventionSet conventions = new ConventionSet();

            modelBuilder = new ModelBuilder(conventions);
            ApplicationDbContext context = new ApplicationDbContext();
            


            TestingDatabase = (string)builder["Database"];
            AssertNotNull(TestingDatabase, nameof(TestingDatabase));
            
            Logger.LogInformation("ServerSideFixture created.");
        }


        private readonly ModelBuilder modelBuilder;

        public string TestingDatabase { get; private set; }

        public void CheckDbExistence()
        {
            using (
            NpgsqlConnection cx = new NpgsqlConnection(Startup.TestingSetup.ConnectionStrings.Default))
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
                using (NpgsqlConnection cx = 
                new NpgsqlConnection(Startup.TestingSetup.ConnectionStrings.DatabaseCtor))
                {
                _logger.LogInformation($"create database for TestingDatabase : {TestingDatabase}");

                    cx.Open();
                    var command = cx.CreateCommand();
                    using (NpgsqlConnection ownercx =  new NpgsqlConnection(Startup.TestingSetup.ConnectionStrings.Default))
                    command.CommandText = $"create database \"{TestingDatabase}\" OWNER \"{ownercx.UserName}\";";
                    
                    _logger.LogInformation(command.CommandText);
                    command.ExecuteNonQuery();
                    cx.Close();
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
        public bool EnsureWeb()
        {
            if (WebApp!=null) return true;

            Task.Run(() => {
                var di = new DirectoryInfo(Startup.TestingSetup.YavscWebPath);
                Assert.True(di.Exists);
                Environment.CurrentDirectory = di.FullName;
                WebHostBuilder = new WebHostBuilder();
                webhostengnine = WebHostBuilder
                .UseEnvironment("Development")
                .UseServer("yavscTests")
                .UseStartup<Yavsc.Startup>()
                .Build();
                WebApp = webhostengnine.Start();
            }).Wait();
            return true;
        }

        public void Dispose()
        {
            if (DbCreated) DropTestDb();
            if (WebApp!=null) WebApp.Dispose();
            if (Logger!=null) Logger.LogInformation("Disposing");
        }

        public bool DbCreated { get { 
            try {
            CheckDbExistence();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);
            }
            return dbCreated; } }

        public WebHostBuilder WebHostBuilder { get; private set; }

        private IHostingEngine webhostengnine;

        public IApplication WebApp { get; private set; }
    }
}


