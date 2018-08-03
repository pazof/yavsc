// // YavscWorkInProgress.cs
// /*
// paul  21/06/2018 10:11 20182018 6 21
// */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
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
        public static string GetPassword()
        {
            var pwd = new StringBuilder();
            while (true)
            {
                var len = pwd.ToString().Length;
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (pwd.Length > 0)
                    {
                        pwd.Remove(len - 1, 1);
                        Console.Write("\b \b");
                    }
                }
                else
                {
                    pwd.Append(i.KeyChar);
                    Console.Write("*");
                }
            }
            return pwd.ToString();
        }
        public static IEnumerable<object[]> GetLoginIntentData(int numTests)
        {

            var allData = new List<object[]>();
            Console.WriteLine($"Please, enter {numTests}:");

            for (int iTest=0; iTest<numTests; iTest++)
            {
                Console.Write("Please, enter a login:");
                var login = Console.ReadLine();
                Console.Write("Please, enter a pass:");
                var pass = GetPassword();

                allData.Add(new object[] { ServerSideFixture.ApiKey, "blouh", "profile",
                "http://localhost:5000/authorize", "http://localhost:5000/oauth/success",
                    "http://localhost:5000/token",login, pass });
            }
            return allData.Take(numTests);

        }


    }
}
