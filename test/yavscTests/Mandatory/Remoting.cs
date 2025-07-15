// // YavscWorkInProgress.cs
// /*
// paul  21/06/2018 10:11 20182018 6 21
// */
using System.Net;
using isnd.tests;
using Xunit;
using Xunit.Abstractions;

namespace yavscTests
{
    [Collection("Yavsc Server")]
    [Trait("regression", "oui")]
    public class Remoting : BaseTestContext, IClassFixture<WebServerFixture>
    {
        public Remoting(WebServerFixture serverFixture, ITestOutputHelper output)
        : base(output, serverFixture)
        {

        }

        [Theory]
        [MemberData(nameof(GetLoginIntentData))]
        public async Task TestUserMayLogin
            (
             string userName,
             string password
            )
        {
            try
            {
                var serverUrl = _serverFixture.Addresses.FirstOrDefault(
                    u => u.StartsWith("https:")
                );

                String auth = _serverFixture.SiteSettings.Authority;

           

                var query = new Dictionary<string, string>
                {
                    ["Username"] = userName,
                    ["Password"] = password,
                    ["GrantType"] = "password"
                };

               throw new NotImplementedException();

            }
            catch (Exception ex)
            {
                var webex = ex as WebException;
                if (webex != null)
                {
                    if (webex.Status == WebExceptionStatus.ProtocolError)
                    {

                    }
                   
                } 
                throw;
            }
        }

        public static IEnumerable<object[]> GetLoginIntentData()
        {
            return new object[][] { new object[] { "testuser", "test" } };
        }
      
    }
}
