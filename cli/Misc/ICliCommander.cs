using Microsoft.Extensions.CommandLineUtils;

public interface ICliCommand
{
    CommandLineApplication Integrates(CommandLineApplication rootApp);
}
