// // YavscWorkInProgress.cs
// /*
// paul  21/06/2018 10:11 20182018 6 21
// */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Yavsc.Authentication;
using static OAuth.AspNet.AuthServer.Constants;

namespace test
{
    [Collection("Yavsc Work In Progress")]
    [Trait("regres", "yes")]
    public class YavscWorkInProgress : BaseTestContext, IClassFixture<ServerSideFixture>
    {
        public YavscWorkInProgress(ServerSideFixture serverFixture, ITestOutputHelper output)
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
             string accessTokenUrl,
             string login,
             string pass
            )
        {
            try
            {
                var oauthor = new OAuthenticator(clientId, clientSecret, scope,
                new Uri(authorizeUrl), new Uri(redirectUrl), new Uri(accessTokenUrl));
                var query = new Dictionary<string, string>();
                query[Parameters.Username] = login;
                query[Parameters.Password] = pass;
                query[Parameters.GrantType] = GrantTypes.Password;
                var result = await oauthor.RequestAccessTokenAsync(query);
                Console.WriteLine(">> Got an output");
                Console.WriteLine(Parameters.AccessToken + ": " + result[Parameters.AccessToken]);
                Console.WriteLine(Parameters.TokenType + ": " + result[Parameters.TokenType]);
                Console.WriteLine(Parameters.ExpiresIn + ": " + result[Parameters.ExpiresIn]);
                Console.WriteLine(Parameters.RefreshToken + ": " + result[Parameters.RefreshToken]);

            }
            catch (Exception ex)
            {
                var webex = ex as WebException;
                if (webex != null && webex.Status == (WebExceptionStatus)400)
                {
                    if (login == "joe")
                    {
                        Console.WriteLine("Bad pass joe!");
                        return;
                    }
                }
                throw;
            }
        }
        public struct LoginIntentData
        {
            public string clientId;
            public string clientSecret;
            public string scope;
            public string authorizeUrl;
            public string redirectUrl;
            public string accessTokenUrl;
            public string login;
            public string pass;
        }
        public static IEnumerable<object[]> GetLoginIntentData(int numTests)
        {

            var allData = new List<object[]>
            {
                new object[] {"d9be5e97-c19d-42e4-b444-0e65863b19e1", "blouh", "profile",
                "http://localhost:5000/authorize", "http://localhost:5000/oauth/success",
                    "http://localhost:5000/token","joe", "badpass"
                },
                new object[] { -4, -6, -10 },
                new object[] { -2, 2, 0 },
                new object[] { int.MinValue, -1, int.MaxValue },
            };

            return allData.Take(numTests);

        }


    }
}
