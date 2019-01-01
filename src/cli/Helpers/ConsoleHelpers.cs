using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using cli.Model;
using Microsoft.Extensions.CommandLineUtils;
using Yavsc.Authentication;
using static OAuth.AspNet.AuthServer.Constants;

namespace cli.Helpers
{
    public static class ConsoleHelpers
    {
        public static CommandLineApplication Integrate(this CommandLineApplication rootApp, ICommander commander)
        {
            return commander.Integrate(rootApp);
        }

        static OAuthenticator OAuthorInstance { get; set; }
        public static OAuthenticator InitAuthor(
            this ConnectionSettings settings,
            string clientId,
            string clientSecret,
            string scope,
            string authorizeUrl,
            string redirectUrl,
            string accessTokenUrl)
        {
            return OAuthorInstance = new OAuthenticator(settings.ClientId, 
            settings.ClientSecret, 
            settings.Scope,
            new Uri(settings.AuthorizeUrl), new Uri(settings.RedirectUrl), new Uri(settings.AccessTokenUrl));
        }

        public static async Task<IDictionary<string, string>> GetAuthFromPass(
            string login,
            string pass)
        {
            var query = new Dictionary<string, string>();
            query[Parameters.Username] = login;
            query[Parameters.Password] = pass;
            query[Parameters.GrantType] = GrantTypes.Password;
            return await OAuthorInstance.RequestAccessTokenAsync(query);
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

    }

}