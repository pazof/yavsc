namespace Yavsc.Org.Tests.Mandatory
{[Collection("Database")]
    [Trait("regression", "II")]
    [Trait("dev", "wip")]
    public class Database : IClassFixture<WebServerFixture>
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

        [Fact]
        public void TestDatabaseMigration()
        {
            // Test logic goes here
            _serverFixture.ResetAndMigrateDatabase();
        }
    }

}
