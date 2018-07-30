
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using NJsonSchema;

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
            var command = new SendMailCommandProvider();
            command.Integrates(cliapp);
            var gencmd = new GenerateJsonSchema();
            gencmd.Integrates(cliapp);

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
