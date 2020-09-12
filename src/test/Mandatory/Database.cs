using System;
using Xunit;
using Xunit.Abstractions;

namespace test.Mandatory
{
    [Collection("Database")]
    [Trait("regres", "no")]
    [Trait("dev", "wip")]
    public class Database: IClassFixture<ServerSideFixture>, IDisposable
    {
        readonly ServerSideFixture _serverFixture;
        readonly ITestOutputHelper output;
        public Database(ServerSideFixture serverFixture, ITestOutputHelper output)
        {
            this.output = output;
            _serverFixture = serverFixture;
            if (_serverFixture.DbCreated)
                _serverFixture.DropTestDb();
            
            output.WriteLine($"Startup.DbSettings.Testing is {Startup.DbSettings.Testing}");
        }

        /// <summary>
        /// Assuming we're using an account that may create databases, 
        /// Install all our migrations in a fresh new database.
        /// </summary>
        [Fact]
        public void InstallFromScratchUsingPoweredNpgsqlUser()
        {
            _serverFixture.CreateTestDb();
            _serverFixture.UpgradeDb();
        }

        public void Dispose()
        {
            if (_serverFixture.DbCreated)
                _serverFixture.DropTestDb();

        }
    }
}
