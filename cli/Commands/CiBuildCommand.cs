
using cli.Model;
using Microsoft.Extensions.CommandLineUtils;

namespace cli
{

    public class CiBuildCommand : ICommander
    {
        public CommandLineApplication Integrate(CommandLineApplication rootApp)
        {
            CommandLineApplication ciBuildApp = rootApp.Command("build",
                (target) =>
                {
                    target.FullName = "Build this project.";
                    target.Description = "Build this project, as specified in .paul-ci.json";
                    target.HelpOption("-? | -h | --help");
                }, false);
            ciBuildApp.OnExecute(()=>
            {


                return 0;
            });
            return ciBuildApp;
        }
    }
}