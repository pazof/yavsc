using Microsoft.Extensions.CommandLineUtils;

namespace cli.Model
{
    public interface ICommander
    {
         CommandLineApplication Integrate(CommandLineApplication rootApp);
    }
}