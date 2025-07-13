// // YavscWorkInProgress.cs
// /*
// paul  21/06/2018 10:11 20182018 6 21
// */
using System.Net;
using isnd.tests;
using Xunit;
using Xunit.Abstractions;
using Yavsc.Authentication;

namespace yavscTests
{
    [Collection("Yavsc Work In Progress")]
    [Trait("regression", "oui")]
    public class Remoting : BaseTestContext, IClassFixture<WebServerFixture>
    {
        public Remoting(WebServerFixture serverFixture, ITestOutputHelper output)
        : base(output, serverFixture)
        {

        }

        [Theory]
        [MemberData(nameof(GetLoginIntentData), parameters: 1)]
        public async Task TestUserMayLogin
            (
            string clientId,
             string clientSecret,
             string scope,
             string authorizeUrl,
             string redirectUrl,
             string accessTokenUrl
            )
        {
            try
            {
                var oauthor = new OAuthenticator(clientId, clientSecret, scope,
                new Uri(authorizeUrl), new Uri(redirectUrl), new Uri(accessTokenUrl));
                var query = new Dictionary<string, string>
                {
                    ["Username"] = _serverFixture.TestingSetup.ValidCreds.UserName,
                    ["Password"] = _serverFixture.TestingSetup.ValidCreds.Password,
                    ["GrantType"] = "Password"
                };

                var result = await oauthor.RequestAccessTokenAsync(query);
                Console.WriteLine(">> Got an output");
                Console.WriteLine( "AccessToken " + result["AccessToken"]);
                Console.WriteLine("TokenType " + result["TokenType"]);
                Console.WriteLine("ExpiresIn " + result["ExpiresIn"]);
                Console.WriteLine("RefreshToken : " + result["RefreshToken"]);

            }
            catch (Exception ex)
            {
                var webex = ex as WebException;
                if (webex != null && webex.Status == (WebExceptionStatus)400)
                {
                    if (_serverFixture.TestingSetup.ValidCreds.UserName == "lame-user")
                    {
                        Console.WriteLine("Bad pass joe!");
                        return;
                    }
                }
                throw;
            }
        }

        public static IEnumerable<object[]> GetLoginIntentData(int count)
        {
            return new object[][] {new object[]{ "", "", "", "", "", "" } };
        }
      
    }
}
