// // YavscWorkInProgress.cs
// /*
// paul  21/06/2018 10:11 20182018 6 21
// */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Builder.Internal;
using Microsoft.Data.Entity;
using Microsoft.Dnx.Compilation.CSharp;
using Microsoft.Dnx.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;
using Yavsc;
using Yavsc.Authentication;
using Yavsc.Lib;
using Yavsc.Models;
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
           var r = new Uri(redirectUrl);
           var oauthor =new OAuthenticator( clientId,  clientSecret,  scope, 
           new Uri( authorizeUrl) , new Uri(redirectUrl) , new Uri(accessTokenUrl));
           var query = new Dictionary<string,string>();
           query[Parameters.Username]=login;
           query[Parameters.Password]=pass;
           query[Parameters.ClientId]=clientId;
           query[Parameters.ClientSecret]=clientSecret;
           query[Parameters.Scope]=scope;
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
