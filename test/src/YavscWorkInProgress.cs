// // YavscWorkInProgress.cs
// /*
// paul  21/06/2018 10:11 20182018 6 21
// */
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Yavsc.Authentication;
using static OAuth.AspNet.AuthServer.Constants;

namespace test
{
    [Collection("Yavsc Work In Progress")]
    [Trait("noregres", "no")]
    public class YavscWorkInProgress : BaseTestContext, IClassFixture<ServerSideFixture>
    {

        ServerSideFixture _serverFixture;
        ITestOutputHelper output;
        public YavscWorkInProgress(ServerSideFixture serverFixture, ITestOutputHelper output)
        {
            this.output = output;
            _serverFixture = serverFixture;
        }
        
        

        [Theory]
        [InlineData("d9be5e97-c19d-42e4-b444-0e65863b19e1","blouh","profile",
         "http://localhost:5000/authorize",
         "http://localhost:5000/oauth/success",
         "http://localhost:5000/token",
         "joe",
         "badpass"
        )]
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
           try {
           var oauthor =new OAuthenticator( clientId,  clientSecret,  scope, 
           new Uri( authorizeUrl) , new Uri(redirectUrl) , new Uri(accessTokenUrl));
           var query = new Dictionary<string,string>();
           query[Parameters.Username]=login;
           query[Parameters.Password]=pass;
           query[Parameters.GrantType]=GrantTypes.Password;
           var result = await oauthor.RequestAccessTokenAsync(query);
           Console.WriteLine(">> Got an output");
           Console.WriteLine(Parameters.AccessToken+": "+result[Parameters.AccessToken]);
           Console.WriteLine(Parameters.TokenType+": "+result[Parameters.TokenType]);
           Console.WriteLine(Parameters.ExpiresIn+": "+result[Parameters.ExpiresIn]);
           Console.WriteLine(Parameters.RefreshToken+": "+result[Parameters.RefreshToken]);

           }
           catch (Exception ex)
           {
               var webex = ex as WebException;
               if (webex!=null && webex.Status == (WebExceptionStatus)400)
               {
                   if (login == "joe") {
                        Console.WriteLine("Bad pass joe!");
                        return;
                   }
               }
               throw;
           }
        }
    }
}
