using System;
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

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var blog = BuildBlogClient(out var settings);
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainPageViewModel(blog, settings)
            };
        }
        else if (ApplicationLifetime is IActivityApplicationLifetime singleViewFactoryApplicationLifetime)
        {
            singleViewFactoryApplicationLifetime.MainViewFactory = () =>
            {
                var blog = BuildBlogClient(out var settings);
                return new MainPage { DataContext = new MainPageViewModel(blog, settings) };
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            var blog = BuildBlogClient(out var settings);
            singleViewPlatform.MainView = new MainPage
            {
                DataContext = new MainPageViewModel(blog, settings)
            };
        }

        base.OnFrameworkInitializationCompleted();
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

    /// <summary>
    /// Build the (Settings, BlogApiClient) pair used by all UI
    /// lifetimes. A single TokenStore is shared so a login performed
    /// by the LoginPage is observable to the MainPage (and vice-versa)
    /// without going through disk on every API call.
    /// </summary>
    private static BlogApiClient BuildBlogClient(out Settings settings)
    {
        settings = new Settings();
        try { settings.Load().GetAwaiter().GetResult(); } catch { /* fall back to embedded defaults */ }
        var tokenStore = new TokenStore(System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
            "PostIt", "tokens.json"));
        return new BlogApiClient(new YavscApiClient(settings, tokenStore));
    }
}
