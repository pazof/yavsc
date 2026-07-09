using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
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
        // SettingsPage is a singleton: there must be one and only one
        // instance of the settings UI for the lifetime of the app.
        // This guarantees that (a) the bindings always reflect the
        // current in-memory Settings state, (b) the page already has
        // its DataContext wired up at composition-root time (see
        // below), and (c) the OpenSettingsRequested handler is a
        // pure push with a no-op-if-already-on-top guard, never a
        // re-resolution from DI. Transient would let the user
        // accumulate stale SettingsPage instances on the navigation
        // stack, each bound to a fresh SettingsViewModel and missing
        // any in-flight edits.
        services.AddSingleton<SettingsPage>();
        services.AddTransient<HomePage>();
        services.AddTransient<SignaturePage>();

        // ViewModels
        services.AddSingleton(settings);
        services.AddSingleton(api);
        services.AddSingleton(client);
        services.AddTransient<MainPageViewModel>();
        services.AddTransient<HomePageViewModel>();
        services.AddTransient<SignaturePageViewModel>();

        // Persistent session banner: one instance for the lifetime of
        // the app so the same VM survives page navigation.
        var sessionStatus = new SessionStatusViewModel { Api = api };
        sessionStatus.Refresh();
        services.AddSingleton(sessionStatus);
        services.AddTransient<SessionStatusBanner>();

        var provider = services.BuildServiceProvider();

        // Bind the canonical Settings to the static accessor so any
        // code path that can't easily take a constructor parameter
        // (designer surfaces, Avalonia data templates) still gets
        // the same instance the rest of the app is using. Idempotent:
        // re-binding from a second App boot (tests) is a no-op.
        Settings.BindToServiceProvider(provider);

        Services = provider;

        DataTemplates.Clear();
        DataTemplates.Add(new ViewLocator(provider));

        // Wire the Settings singleton onto the SettingsPage singleton
        // once, at composition time. The page is registered as a
        // singleton (see above) precisely so this binding is stable
        // for the lifetime of the app: every push to / pop from the
        // navigation stack finds the same ContentPage with the same
        // DataContext, and the TwoWay bindings inside the page keep
        // mutating the same in-memory Settings instance that the rest
        // of the app reads (OidcClientOptions construction, etc.).
        provider.GetRequiredService<SettingsPage>().DataContext = settings;

        // Settings.DarkMode was previously a dead field: it round-
        // tripped through the settings file and the SettingsPage
        // CheckBox, but no consumer ever read it. Wire it here to
        // Application.RequestedThemeVariant so the toggle takes
        // effect immediately, and seed the initial theme from the
        // value Load() just populated (so a dark-mode user lands on
        // a dark window on first launch, not on a default-light
        // window that flips after the user touches the toggle).
        ApplyDarkMode(settings);
        settings.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Settings.DarkMode))
            {
                ApplyDarkMode(settings);
            }
        };

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

            // When the user signs in interactively (Login button on
            // the session banner), push MainPage on top of HomePage.
            sessionStatus.LoginSucceeded += () =>
            {
                var w = (MainWindow)((IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!).MainWindow!;
                _ = PushMainPageAsync(provider, w);
            };

            // When the user clicks the "Paramètres" button on the
            // session banner, push the SettingsPage singleton on top
            // of the current navigation stack. The DataContext is
            // already wired at composition time (see the
            // provider.GetRequiredService<SettingsPage>().DataContext
            // assignment above), so this handler is a pure
            // navigation concern.
            //
            // Anti-empilement guard: if the SettingsPage is already
            // at the top of the stack, do nothing. NavigationPage's
            // PushAsync does not deduplicate; calling it twice with
            // the same instance would push it a second time and the
            // user would have to tap Back twice to leave. Reference
            // comparison is correct here because SettingsPage is a
            // singleton — there is exactly one instance to compare
            // against.
            sessionStatus.OpenSettingsRequested += () =>
            {
                var w = (MainWindow)((IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!).MainWindow!;
                var settingsPage = provider.GetRequiredService<SettingsPage>();
                var stack = w.NavRoot.NavigationStack;
                if (stack.Count > 0 && ReferenceEquals(stack[stack.Count - 1], settingsPage))
                {
                    return;
                }
                _ = w.NavRoot.PushAsync(settingsPage);
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
        YavscApiClient api,
        MainWindow window)
    {
        var refreshed = await api.TrySilentLoginAsync().ConfigureAwait(true);
        var sessionStatus = provider.GetRequiredService<SessionStatusViewModel>();
        sessionStatus.Refresh();
        if (!refreshed) return;

        await PushMainPageAsync(provider, window).ConfigureAwait(true);
    }

    /// <summary>
    /// Resolve a fresh <c>MainPage</c> + VM from DI and push it on top
    /// of the current navigation stack. Used both by <see cref="BootAsync"/>
    /// (silent refresh at boot) and by <c>SessionStatusViewModel.LoginSucceeded</c>
    /// (interactive login from the banner). Pulled out as a helper so
    /// the two callers can't drift apart.
    /// </summary>
    private static async Task PushMainPageAsync(IServiceProvider provider, MainWindow window)
    {
        var mainVm = provider.GetRequiredService<MainPageViewModel>();
        var mainPage = provider.GetRequiredService<MainPage>();
        mainPage.DataContext = mainVm;
        await window.NavRoot.PushAsync(mainPage).ConfigureAwait(true);
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
