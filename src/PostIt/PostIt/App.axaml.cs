using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using PostIt.Services;
using PostIt.ViewModels;
using PostIt.Views;
using PostIt.Helpers;

namespace PostIt;

public partial class App : Application
{
    private int _bootStarted;

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
    public IServiceProvider? ServiceProvider { get; private set; }

    public MainView? View { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (TryHandOffCustomSchemeUrl()) return;

        this.ServiceProvider = new ServiceCollection().BuildServices();
        var settings = ServiceProvider.GetRequiredService<Settings>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = ServiceProvider.GetRequiredService<MainWindow>();
            desktop.MainWindow = window;
            View = window.MainView;
            this.ConfigureRootView(window.MainView);

            ApplyDarkMode(settings);
        }
        else if (ApplicationLifetime is IActivityApplicationLifetime singleViewFactoryApplicationLifetime)
        {
            singleViewFactoryApplicationLifetime.MainViewFactory =
                () =>
                {
                    View = ServiceProvider.GetRequiredService<MainView>();
                    this.ConfigureRootView(View);
                    ApplyDarkMode(settings);
                    return View;
                };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = View = ServiceProvider.GetRequiredService<MainView>();
            ConfigureRootView(View);
            ApplyDarkMode(settings);
        }

        base.OnFrameworkInitializationCompleted();
    }

private void ConfigureRootView(MainView rootView)
{
    // Déclencher le Boot une seule fois lors du chargement du contrôle à l'écran.
    rootView.AttachedToVisualTree += async (_, _) => await BootOnceAsync();

    var sessionStatus = ServiceProvider!.GetRequiredService<SessionStatusViewModel>();
    sessionStatus.LogoutCompleted += () =>
    {
        // Remplacer Window.NavRoot par rootView.NavRoot
        rootView.NavRoot.PopToRootAsync();
    };

    sessionStatus.LoginSucceeded += async () =>
    {
        await PushMainPageAsync();
    };

    rootView.SessionBanner.DataContext = sessionStatus;
}

    private async Task BootOnceAsync()
    {
        if (Interlocked.Exchange(ref _bootStarted, 1) == 1)
        {
            return;
        }

        var api = ServiceProvider!.GetRequiredService<YavscApiClient>();
        await BootAsync(this.ServiceProvider!, api);
    }

    /// <summary>
    /// Test-only hook: bind a concrete <see cref="MainView"/> so
    /// command-driven navigation paths (<see cref="PushPage"/>) can
    /// push onto a real <see cref="NavigationPage"/> in headless
    /// fixtures that do not run the full desktop lifetime bootstrap.
    /// </summary>
    internal void AttachMainWindow(MainView mainView)
    {
        View = mainView ?? throw new ArgumentNullException(nameof(mainView));
    }

    private static void ApplyDarkMode(Settings settings)
    {
        Application.Current!.RequestedThemeVariant =
                            settings.DarkMode ? ThemeVariant.Dark : ThemeVariant.Light;
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
        YavscApiClient api)
    {
        var refreshed = await api.TrySilentLoginAsync().ConfigureAwait(true);
        var sessionStatus = provider.GetRequiredService<SessionStatusViewModel>();
        sessionStatus.Refresh();
        var homePage = provider.GetRequiredService<HomePageViewModel>();
        var app = (App)Current!;
        await app.PushPageAsync(homePage);
        if (!refreshed) return;

        await PushMainPageAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// Resolve a fresh <c>MainPageViewModel</c> from DI and push its
    /// mapped page (via <see cref="ViewLocator"/>) on top
    /// of the current navigation stack. Used both by <see cref="BootAsync"/>
    /// (silent refresh at boot) and by <c>SessionStatusViewModel.LoginSucceeded</c>
    /// (interactive login from the banner). Pulled out as a helper so
    /// the two callers can't drift apart.
    /// </summary>
    public static async Task PushMainPageAsync()
    {
        var app = (App)Current!;
        var mainVm = app.ServiceProvider!.GetRequiredService<MainViewModel>();
        await mainVm.InitializeAsync();
        await app.PushPageAsync(mainVm);
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

    internal async Task GoBackAsync()
    {
        await View!.NavRoot.PopAsync();
    }
}
