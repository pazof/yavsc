using Xunit;
using Xunit.Abstractions;

namespace test.Mandatory
{
    [Collection("Database")]
    [Trait("regres", "no")]
    [Trait("dev", "wip")]
    public class Database: IClassFixture<ServerSideFixture>
    {
        ServerSideFixture _serverFixture;
        ITestOutputHelper output;
        public Database(ServerSideFixture serverFixture, ITestOutputHelper output)
        {
            this.output = output;
            _serverFixture = serverFixture;
            
            output.WriteLine($"Startup.TestDbSettings.Database was {Startup.TestDbSettings.Database}");
        }

        /// <summary>
        /// Assuming we're using an account that may create databases, 
        /// Install all our migrations in a fresh new database.
        /// </summary>
        [Fact]
        public void InstallFromScratchUsingPoweredNpgsqlUser()
        {
            if (_serverFixture.DbCreated)
                _serverFixture.DropTestDb();
            
            _serverFixture.CreateTestDb();
            _serverFixture.UpgradeDb();
        }
    }
}
