using System;
using isnd.tests;
using Xunit;
using Xunit.Abstractions;

namespace yavscTests.Mandatory
{
    [Collection("Database")]
    [Trait("regression", "II")]
    [Trait("dev", "wip")]
    public class Database: IClassFixture<WebServerFixture>, IDisposable
    {
        readonly WebServerFixture _serverFixture;
        readonly ITestOutputHelper output;
        public Database(WebServerFixture serverFixture, ITestOutputHelper output)
        {
            this.output = output;
            _serverFixture = serverFixture;
            try {
                if (_serverFixture.DbCreated)
                 
            _serverFixture.DropTestDb();

            }
            catch (Exception)
            {
                output.WriteLine("db not dropped");
            }
            output.WriteLine($"Startup.Testing.ConnectionStrings.Default is {_serverFixture.TestingSetup.ConnectionStrings.Default}");
        }

        /// <summary>
        /// Assuming we're using an account that may create databases, 
        /// Install all our migrations in a fresh new database.
        /// </summary>
        [Fact]
        public void InstallFromScratchUsingPoweredNpgsqlUser()
        {
            Assert.True(_serverFixture.EnsureTestDb());
            Assert.True(_serverFixture.UpgradeDb()==0);
        }

        public void Dispose()
        {
            if (_serverFixture!=null) 
            {
                _serverFixture.Dispose();
            }

        }
    }
}
