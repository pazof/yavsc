using System;
using Xunit;
using Xunit.Abstractions;

namespace test.Mandatory
{
    [Collection("Database")]
    [Trait("regression", "non")]
    [Trait("dev", "wip")]
    public class Database: IClassFixture<ServerSideFixture>, IDisposable
    {
        readonly ServerSideFixture _serverFixture;
        readonly ITestOutputHelper output;
        public Database(ServerSideFixture serverFixture, ITestOutputHelper output)
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
            output.WriteLine($"Startup.Testing.ConnectionStrings.DatabaseCtor is {Startup.Testing.ConnectionStrings.DatabaseCtor}");
        }

        /// <summary>
        /// Assuming we're using an account that may create databases, 
        /// Install all our migrations in a fresh new database.
        /// </summary>
        [Fact]
        public void InstallFromScratchUsingPoweredNpgsqlUser()
        {
            _serverFixture.EnsureTestDb();
            _serverFixture.UpgradeDb();
        }

        public void Dispose()
        {
            _serverFixture.DropTestDb();

        }
    }
}
