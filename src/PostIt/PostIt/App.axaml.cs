using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PostIt.Services;
using PostIt.ViewModels;
using PostIt.Views;

namespace PostIt;

public partial class App : Application
{
    public App()
    {
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Single-instance hand-off: if we were launched with a
        // custom-scheme URL on the command line, we are a 2nd
        // instance whose job is to forward the OAuth2 callback
        // URL to the running PostIt process and exit. The first
        // instance is parked inside CustomSchemeBrowser.InvokeAsync
        // waiting on the named pipe for exactly this message.
        if (TryHandOffCustomSchemeUrl())
        {
            return;
        }
        var settings = new Settings();
        // Synchronous: Settings.Load is intentionally non-async so we
        // don't deadlock the Avalonia UI thread. .Wait() on an async
        // method would block here forever on the await inside the
        // file read.
        settings.Load();

        var tokenStore = new TokenStore(System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
            "PostIt", "tokens.json"));
        var client = new BlogApiClient(new YavscApiClient(settings, tokenStore));
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainPageViewModel(client, settings)
            };
        }
        else if (ApplicationLifetime is IActivityApplicationLifetime singleViewFactoryApplicationLifetime)
        {
            singleViewFactoryApplicationLifetime.MainViewFactory = () =>
            {
                return new MainPage { DataContext = new MainPageViewModel(client, settings) };
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainPage
            {
                DataContext = new MainPageViewModel(client, settings)
            };
        }

    }

    private bool TryHandOffCustomSchemeUrl()
    {
        var args = Environment.GetCommandLineArgs();
        var scheme = Platform.CustomScheme;
        foreach (var arg in args)
        {
            if (arg.StartsWith(scheme + "://", StringComparison.OrdinalIgnoreCase))
            {
                // Best-effort: try to send the URL to the running
                // instance via the named pipe. If the pipe isn't
                // answering, just exit — there's no 1st instance
                // to forward to (e.g. user double-clicked the link
                // after closing PostIt). Falling through with a
                // normal startup would be confusing.
                SingleInstance.TryHandOffAsync(arg).GetAwaiter().GetResult();

                if (ApplicationLifetime is IControlledApplicationLifetime lifetime)
                {
                    lifetime.Shutdown(0);
                }
                else
                {
                    Environment.Exit(0);
                }
                return true;
            }
        }
        return false;
    }

}
