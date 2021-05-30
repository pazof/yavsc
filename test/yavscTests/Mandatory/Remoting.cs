// // YavscWorkInProgress.cs
// /*
// paul  21/06/2018 10:11 20182018 6 21
// */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.PlatformAbstractions;
using Xunit;
using Xunit.Abstractions;
using Yavsc.Authentication;
using static OAuth.AspNet.AuthServer.Constants;

namespace test
{
    [Collection("Yavsc Work In Progress")]
    [Trait("regression", "oui")]
    public class Remoting : BaseTestContext, IClassFixture<ServerSideFixture>
    {
        readonly RegiserAPI r;
        public Remoting(ServerSideFixture serverFixture, ITestOutputHelper output)
        : base(output, serverFixture)
        {
            
                r = new RegiserAPI(serverFixture, output);
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
                r.EnsureWeb();

                var oauthor = new OAuthenticator(clientId, clientSecret, scope,
                new Uri(authorizeUrl), new Uri(redirectUrl), new Uri(accessTokenUrl));
                var query = new Dictionary<string, string>
                {
                    [Parameters.Username] = Startup.Testing.ValidCreds[0].UserName,
                    [Parameters.Password] = Startup.Testing.ValidCreds[0].Password,
                    [Parameters.GrantType] = GrantTypes.Password
                };

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
        
        public static IEnumerable<object[]> GetLoginIntentData(int numTests)
        {

            var allData = new List<object[]>();

            for (int iTest=0; iTest < numTests && iTest < Startup.Testing.ValidCreds.Length; iTest++)
            {

                var login = Startup.Testing.ValidCreds[iTest].UserName;
                var pass =  Startup.Testing.ValidCreds[iTest].Password;

                allData.Add(new object[] { ServerSideFixture.ApiKey, "blouh", "profile",
                "http://localhost:5000/authorize", "http://localhost:5000/oauth/success",
                    "http://localhost:5000/token",login, pass});
            }
            var valid = allData.Count;
            for (int iTest=0; iTest + valid < numTests  && iTest < Startup.Testing.InvalidCreds.Length; iTest++)
            {
                var login = Startup.Testing.InvalidCreds[iTest].UserName;
                var pass =  Startup.Testing.InvalidCreds[iTest].Password;

                allData.Add(new object[] { ServerSideFixture.ApiKey, "blouh", "profile",
                "http://localhost:5000/authorize", "http://localhost:5000/oauth/success",
                    "http://localhost:5000/token",login, 0 });
            }
            return allData.Take(numTests);

        }


    }
}
