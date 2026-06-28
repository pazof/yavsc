using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PostIt.Services;
using PostIt.ViewModels;
using PostIt.Views;

namespace PostIt;

public partial class App : Application
{
    /// <summary>
    /// DI container the platform entry points hand to ViewModels so
    /// they can resolve the canonical <see cref="Settings"/> singleton
    /// (and any other shared service) instead of falling back to a
    /// freshly-constructed <c>new Settings()</c>. The earlier fallback
    /// path is what created two Settings instances on
    /// <c>postit://callback</c> re-launches and crashed Avalonia's
    /// binding sink with a cross-thread exception inside
    /// <c>DataValidationErrors.SetErrors</c>.
    /// </summary>
    public IServiceProvider? Services { get; private set; }

    public App()
    {
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Belt-and-braces 2nd-instance guard. The primary check now
        // lives in PostIt.Desktop.Program.Main and exits before
        // Avalonia boots — preventing a flash of the MainWindow on
        // every postit://callback launch. This block is kept for any
        // entry point that bypasses Program.Main (PostIt.Browser,
        // PostIt.Android's process lifecycle, ad-hoc tests that build
        // App directly) and as defence-in-depth in case the Desktop
        // build is ever reconfigured to skip the early check.
        if (TryHandOffCustomSchemeUrl()) return;

        var settings = new Settings();
        settings.Load();

        var tokenStore = new TokenStore(System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
            "PostIt", "tokens.json"));

        var api = new YavscApiClient(settings, tokenStore);
        var client = new BlogApiClient(api);

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

        // Persistent session banner: one instance for the lifetime of
        // the app so the same VM survives page navigation.
        var sessionStatus = new SessionStatusViewModel { Api = api };
        sessionStatus.Refresh();
        services.AddSingleton(sessionStatus);
        services.AddTransient<SessionStatusBanner>();

        var provider = services.BuildServiceProvider();

        // Bind the canonical Settings to the static accessor so any
        // code path that can't easily take a constructor parameter
        // (designer surfaces, Avalonia data templates, the
        // LoginPage.axaml.cs fallback) still gets the same instance
        // the rest of the app is using. Idempotent: re-binding from
        // a second App boot (tests) is a no-op.
        Settings.BindToServiceProvider(provider);

        Services = provider;

        DataTemplates.Clear();
        DataTemplates.Add(new ViewLocator(provider));

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var homePage = provider.GetRequiredService<HomePage>();
            homePage.DataContext = provider.GetRequiredService<HomePageViewModel>();

            var window = new MainWindow();
            window.SessionBanner.DataContext = sessionStatus;

            // Build the navigation stack from scratch: HomePage is the
            // root in both cases. App.BootAsync will push MainPage on
            // top if the silent refresh succeeds.
            window.DataContext = homePage.DataContext;
            desktop.MainWindow = window;
            _ = window.NavRoot.PushAsync(homePage);

            // When the user logs out, route back to HomePage. We
            // ReplaceAsync the current top so we don't grow the stack
            // on every logout — otherwise repeated login/logout would
            // eventually balloon the back history.
            sessionStatus.LogoutCompleted += () =>
            {
                var w = (MainWindow)((IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!).MainWindow!;
                var nav = w.NavRoot;
                var hp = provider.GetRequiredService<HomePage>();
                hp.DataContext = provider.GetRequiredService<HomePageViewModel>();
                _ = nav.PopToRootAsync();
            };

            window.Opened += async (_, _) => await BootAsync(provider, api, window);
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            singleView.MainView = new MainWindow
            {
                DataContext = provider.GetRequiredService<HomePageViewModel>()
            };
        }
    }

    /// <summary>
    /// Run once after the main window is shown: try to refresh the
    /// cached OIDC tokens silently; on success, push MainPage on top
    /// of HomePage so the user lands on the blog editor already
    /// authenticated. On failure (refresh token rejected, no bundle
    /// on disk), leave them on HomePage and the Login button is the
    /// next step.
    /// </summary>
    private static async Task BootAsync(
        IServiceProvider provider,
        YavscApiClient api,
        MainWindow window)
    {
        var refreshed = await api.TrySilentLoginAsync().ConfigureAwait(true);
        var sessionStatus = provider.GetRequiredService<SessionStatusViewModel>();
        sessionStatus.Refresh();
        if (!refreshed) return;

        var mainVm = provider.GetRequiredService<MainPageViewModel>();
        var mainPage = provider.GetRequiredService<MainPage>();
        mainPage.DataContext = mainVm;
        await window.NavRoot.PushAsync(mainPage);
    }

    private bool TryHandOffCustomSchemeUrl()
    {
        var url = SchemeUrlDetector.FindCallbackUrl(Environment.GetCommandLineArgs());
        if (url is null) return false;

        // Best-effort: try to send the URL to the running
        // instance via the named pipe. If the pipe isn't
        // answering, just exit — there's no 1st instance
        // to forward to (e.g. user double-clicked the link
        // after closing PostIt). Falling through with a
        // normal startup would be confusing.
        try
        {
            SingleInstance.TryHandOffAsync(url).GetAwaiter().GetResult();
        }
        catch
        {
            // Pipe errors are non-fatal for the 2nd-instance
            // hand-off.
        }

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
