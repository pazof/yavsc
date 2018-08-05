
using cli.Model;
using Microsoft.Extensions.CommandLineUtils;

namespace cli.Commands
{
    public class AuthCommander : ICommander
    {
        public CommandLineApplication Integrate(CommandLineApplication rootApp)
        {
            
            CommandLineApplication authApp = rootApp.Command("auth",
                (target) =>
                {
                    target.FullName = "Authentication methods";
                    target.Description = "Login, save credentials and get authorized.";
                    target.HelpOption("-? | -h | --help");
                    var loginCommand = target.Command("login", app => {
                        var loginarg = app.Argument("login", "login to use", true);
                        app.Option( "-s | --save", "Save authentication token to file", CommandOptionType.NoValue);
                        app.HelpOption("-? | -h | --help");
                    }  );
                }, false);
            authApp.OnExecute(()=>
            {
                return 0;
            });
            return authApp;
        }
    }
}