
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Yavsc.Models;

namespace Yavsc.Org.Tests.Mandatory
{[Collection("Database")]
    [Trait("regression", "II")]
    [Trait("dev", "wip")]
    public class Database: IClassFixture<WebServerFixture>, IDisposable
    {
        readonly ITestOutputHelper output;
        readonly WebServerFixture _serverFixture;

        public Database(ITestOutputHelper output, WebServerFixture _serverFixture)
        {
            this.output = output;
            this._serverFixture = _serverFixture;
        }

        /// <summary>
        /// Assuming we're using an account that may create databases,
        /// Install all our migrations in a fresh new database.
        /// </summary>

        public void Dispose()
        {
            if (_serverFixture!=null)
            {
                _serverFixture.Dispose();
            }

        }

        [Fact]
        public void TestDatabaseMigration()
        {
            // Test logic goes here
            _serverFixture.ResetAndMigrateDatabase();
        }
    }

}
