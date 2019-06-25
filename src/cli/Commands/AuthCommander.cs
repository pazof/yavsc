
using System;
using System.Collections.Generic;
using System.Text;
using cli.Model;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Yavsc.Authentication;

namespace cli.Commands
{
    public class AuthCommander : ICommander
    {
        private CommandArgument _login;
        private CommandOption _apiKey;
        private CommandOption _secret;
        private CommandOption _scope;
        private CommandOption _save;

        ILogger _logger;

        public AuthCommander(ILoggerFactory loggerFactory)
         {
             _logger = loggerFactory.CreateLogger<AuthCommander>();
         }

        public CommandLineApplication Integrate(CommandLineApplication rootApp)
        {
            CommandLineApplication authApp = rootApp.Command("auth",
                (target) =>
                {
                    target.FullName = "Authentication methods";
                    target.Description = "Login, save credentials and get authorized.";
                    target.HelpOption("-? | -h | --help");
                    var loginCommand = target.Command("login", app => {
                        _login = app.Argument("login", "login to use", true);
                        _apiKey = app.Option("-a | --api", "API key to use against authorization server", CommandOptionType.SingleValue);
                        _secret = app.Option( "-e | --secret", "Secret phrase associated to API key", CommandOptionType.SingleValue);
                        _scope = app.Option( "-c | --scope", "invoked scope asking for a security token", CommandOptionType.SingleValue);
                        _save = app.Option( "-s | --save", "Save authentication token to given file", CommandOptionType.SingleValue);
                        app.HelpOption("-? | -h | --help");
                    }  );
                    loginCommand.OnExecute(async ()=>
            {
                var authUrl = Startup.ConnectionSettings.AuthorizeUrl;
                var redirect = Startup.ConnectionSettings.RedirectUrl;
                var tokenUrl = Startup.ConnectionSettings.AccessTokenUrl;

                 var oauthor = new OAuthenticator(_apiKey.HasValue() ? _apiKey.Value() : Startup.ConnectionSettings.ClientId,
                  _secret.HasValue() ? _secret.Value() : Startup.ConnectionSettings.ClientSecret, 
                  _scope.HasValue() ? _scope.Value() : Startup.ConnectionSettings.Scope,
                new Uri(authUrl), new Uri(redirect), new Uri(tokenUrl));
                var query = new Dictionary<string, string>();
                query["username"] = _login.Value;
                query["password"] = GetPassword(_login.Value);
                query["grant_type"] = "password";
                try {
                    var result = await oauthor.RequestAccessTokenAsync(query);
                    Startup.UserConnectionSettings.AccessToken = result["access_token"];
                    Startup.UserConnectionSettings.ExpiresIn = result["expires_in"];
                    Startup.UserConnectionSettings.RefreshToken = result["refresh_token"];
                    Startup.UserConnectionSettings.TokenType = result["token_type"];
                    Startup.UserConnectionSettings.UserName = _login.Value;
                    Startup.SaveCredentials(_save.HasValue() ? _save.Value() :  Startup.UserConnectionsettingsFileName);
                   
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }

                return 0;
            });
                }, false);

            return authApp;
        }

        public static string GetPassword(string userName)
        {
            var oldBack = Console.BackgroundColor;
            var oldFore = Console.ForegroundColor;
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{userName}'s password:");
            Console.BackgroundColor = oldBack;
            Console.ForegroundColor = oldFore;

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
            Console.WriteLine();
            return pwd.ToString();
        }
        
    }
}