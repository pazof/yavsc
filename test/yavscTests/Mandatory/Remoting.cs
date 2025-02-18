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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.PlatformAbstractions;
using Xunit;
using Xunit.Abstractions;
using Yavsc.Authentication;
using static OAuth.AspNet.AuthServer.Constants;

namespace yavscTests
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
             string accessTokenUrl
            )
        {
            try
            {
                r.EnsureWeb();

                var oauthor = new OAuthenticator(clientId, clientSecret, scope,
                new Uri(authorizeUrl), new Uri(redirectUrl), new Uri(accessTokenUrl));
                var query = new Dictionary<string, string>
                {
                    [Parameters.Username] = Startup.TestingSetup.ValidCreds.UserName,
                    [Parameters.Password] = Startup.TestingSetup.ValidCreds.Password,
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
                    if (Startup.TestingSetup.ValidCreds.UserName == "lame-user")
                    {
                        Console.WriteLine("Bad pass joe!");
                        return;
                    }
                }
                throw;
            }
        }

    }
}
