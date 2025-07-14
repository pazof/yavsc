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
            
            output.WriteLine($"Testing connection string is {_serverFixture?.TestingSetup?.ConnectionStrings.DefaultConnection}");
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
    }
}
