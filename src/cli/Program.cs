
using System;
using cli.Commands;
using Microsoft.Extensions.CommandLineUtils;

namespace cli
{
    public partial class Program
    {

        public static void Main(string[] args)
        {
            CommandOption rootCommandHelpOption = null;

            CommandLineApplication cliapp = new CommandLineApplication(false);
            cliapp.Name = "cli";
            cliapp.FullName = "Yavsc command line interface";
            cliapp.Description = "Dnx console app for yavsc server side";
            cliapp.ShortVersionGetter = () => "v1.0";
            cliapp.LongVersionGetter = () => "version 1.0 (stable)";
            rootCommandHelpOption = cliapp.HelpOption("-? | -h | --help");

            (new SendMailCommandProvider()).Integrate(cliapp);
            (new GenerateJsonSchema()).Integrate(cliapp);
            (new AuthCommander()).Integrate(cliapp);
            (new CiBuildCommand()).Integrate(cliapp);
            (new GenerationCommander()).Integrate(cliapp);

            if (args.Length == 0)
            {
                cliapp.ShowHint();
                Environment.Exit(1);
            }

            cliapp.Execute(args);

            if (cliapp.RemainingArguments.Count > 0)
            {
                cliapp.ShowHint();
                Environment.Exit(2);
            }

        }
    }
}
