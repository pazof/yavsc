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
             string userName,
             string password
            )
        {
            try
            {
                String auth = _serverFixture.SiteSettings.Authority;

                var oauthor = new OAuthenticator(clientId, clientSecret, scope,
                new Uri(authorizeUrl), new Uri(redirectUrl), new Uri(accessTokenUrl));
                var query = new Dictionary<string, string>
                {
                    ["Username"] = userName,
                    ["Password"] = password,
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
                    if (_serverFixture?.TestingSetup?.ValidCreds.UserName == "lame-user")
                    {
                        Console.WriteLine("Bad pass joe!");
                        return;
                    }
                }
                throw;
            }
        }

        public static IEnumerable<object[]> GetLoginIntentData(int countFakes = 0)
        {
            if (countFakes == 0)
                return new object[][] { new object[] { "testuser", "test" } };

            var fakUsers = new List<String[]>();
            for (int i = 0; i < countFakes; i++)
            {
                fakUsers.Add(new String[] { "fakeTester" + i, "pass" + i });
            }
            return fakUsers;
        }
      
    }
}
