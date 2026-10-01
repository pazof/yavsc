using System;
using System.Collections.Generic;
using Mono.Options;
using PostIt;
using PostIt.ViewModels;

namespace PostIt.CLIOptions;

public static class CLIOptionHandler
{
    public static void HandleArgs(string[] args)
    {
        bool showHelp = false;
        string configFile = null;
        bool liveMode = false;

        var options = new OptionSet {
            { "c|configuration=", "Configuration file to load", (string v) => configFile = v },
            { "l|live", "Enable live mode", v => liveMode = true },
            { "v|version", "Show the application version", v => Console.WriteLine("PostIt version "
               + typeof(App).Assembly.GetName().Version?.ToString()) },
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
            App.UseConfigFileWhenLoading(configFile);
        }
        if (liveMode)
        {
            App.EnableLiveMode();
        }
    }
}
