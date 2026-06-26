using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
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
        if (TryHandOffCustomSchemeUrl()) return;

        var settings = new Settings();
        settings.Load();

        var tokenStore = new TokenStore(System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
            "PostIt", "tokens.json"));

        var api = new YavscApiClient(settings, tokenStore);
        var client = new BlogApiClient(api);

        // Configure DI
        var services = new ServiceCollection();

        // Vues
        services.AddTransient<MainPage>();
        services.AddTransient<LoginPage>();
        services.AddTransient<SettingsPage>();
        services.AddTransient<HomePage>();

        // ViewModels
        services.AddSingleton(settings);
        services.AddSingleton(api);
        services.AddSingleton(client);
        services.AddTransient<MainPageViewModel>();
        services.AddTransient<SettingsPageViewModel>();
        services.AddTransient<LoginPageViewModel>();
        services.AddTransient<HomePageViewModel>();
        var provider = services.BuildServiceProvider();

        // Injecter le ViewLocator avec le provider
        DataTemplates.Clear();
        DataTemplates.Add(new ViewLocator(provider));

        // Page de départ
        var homeVm = provider.GetRequiredService<HomePageViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow { DataContext = homeVm };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            singleView.MainView = new MainWindow { DataContext = homeVm };
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
