using System;
using System.Collections.Generic;
using Mono.Options;
using PostIt;
using PostIt.ViewModels;

public static class CLIOptionHandler
{
    public static void HandleArgs(string[] args)
    {
        bool showHelp = false;
        string configFile = null;

        var options = new OptionSet {
            { "c|configuration=", "Configuration file to load", (string v) => configFile = v },
            { "h|help", "Show this message and exit", v => showHelp = true }
        };

        List<string> extra = options.Parse(args);

        if (showHelp)
        {
            Console.WriteLine("Usage: app [OPTIONS]+");
            options.WriteOptionDescriptions(Console.Out);
            return;
        }
        if (configFile != null)
        {
            var app = App.Current as App;
            app.UseConfigFileWhenLoading(configFile);
        }
    }
}
