using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using PostIt.Services;
using Yavsc.Api.Client;
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
    public IServiceProvider? ServiceProvider { get; private set; }

    public App()
    {
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
#if DEBUG
        this.AttachDeveloperTools();
#endif
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

        this.ServiceProvider = BuildServices(new ServiceCollection());
        AttachServiceProvider(ServiceProvider);
        var settings = ServiceProvider.GetRequiredService<Settings>();

        DataTemplates.Clear();
        DataTemplates.Add(new ViewLocator(ServiceProvider));

        // Wire the Settings singleton onto the SettingsPage singleton
        // once, at composition time. The page is registered as a
        // singleton (see above) precisely so this binding is stable
        // for the lifetime of the app: every push to / pop from the
        // navigation stack finds the same ContentPage with the same
        // DataContext, and the TwoWay bindings inside the page keep
        // mutating the same in-memory Settings instance that the rest
        // of the app reads (OidcClientOptions construction, etc.).
        ServiceProvider.GetRequiredService<SettingsPage>().DataContext = settings;

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
            desktop.MainWindow = CreateMainWindow();
        }
        else if (ApplicationLifetime is IActivityApplicationLifetime singleViewFactoryApplicationLifetime)
        {
            singleViewFactoryApplicationLifetime.MainViewFactory = () => CreateMainWindow();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = CreateMainWindow();
        }
        base.OnFrameworkInitializationCompleted();
    }

    MainWindow window;
    private MainWindow CreateMainWindow()
    {
        window = new MainWindow();
        var api = ServiceProvider!.GetRequiredService<YavscApiClient>();
        window.Opened += async (_, _) => await BootAsync(this.ServiceProvider!, api);
          var sessionStatus = ServiceProvider!.GetRequiredService<SessionStatusViewModel>();
        sessionStatus.LogoutCompleted += () =>
        {
            window.NavRoot.PopToRootAsync();
        };

        sessionStatus.LoginSucceeded += () =>
        {
            PushMainPageAsync();
        };

        var homeVm = ServiceProvider!.GetRequiredService<HomePageViewModel>();

        this.PushPageAsync(homeVm).Wait();
        window.SessionBanner.DataContext = sessionStatus;
        return window;
    }

    /// <summary>
    /// Build the DI container the app uses. Pulled out of
    /// <see cref="OnFrameworkInitializationCompleted"/> so headless
    /// tests can construct the same container at <c>TestApp</c> boot
    /// without going through the full Avalonia desktop lifetime
    /// (which never runs in a unit test). The container returned is
    /// the exact one production uses — no test-only fakes, no
    /// trimmed service list — so a test that exercises a VM, page,
    /// or service resolves through the same wiring the real app
    /// does, and a green test is a green contract for prod.
    /// </summary>
    internal static IServiceProvider BuildServices(ServiceCollection services)
    {
        var settings = new Settings();
        settings.Load();

        var tokenStore = new TokenStore(System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
            "PostIt", "tokens.json"));

        var api = new YavscApiClient(settings, tokenStore);
        var client = new BlogApiClient(api, settings.BlogsApiUrl);
        var circleClient = new CircleApiClient(api, settings.BlogsApiUrl);
        var blogAclClient = new BlogAclApiClient(api, settings.BlogsApiUrl);
        var userSearchClient = new UserSearchClient(api, settings.BlogsApiUrl);
        var contactService = new ContactService();
        var userDirectory = new UserDirectory(userSearchClient);

        // Vues
        services.AddTransient<MainPage>();
        // SettingsPage is a singleton: there must be one and only one
        // instance of the settings UI for the lifetime of the app.
        // This guarantees that (a) the bindings always reflect the
        // current in-memory Settings state, (b) the page already has
        // its DataContext wired up at composition-root time (see
        // below), and (c) PushPageAsync's anti-empilement guard sees
        // the same instance across pushes, so a second Settings tap
        // is a no-op rather than re-pushing the page. Transient would
        // let the user accumulate stale SettingsPage instances on
        // the navigation stack, each bound to a fresh
        // SettingsViewModel and missing any in-flight edits.
        services.AddSingleton<SettingsPage>();
        services.AddTransient<HomePage>();
        services.AddTransient<SignaturePage>();
        services.AddTransient<CirclesPage>();
        // Dialogs (modal-light pages): the ViewLocator resolves
        // them when a caller pushes a PostAclDialogViewModel or
        // AddCircleMemberDialogViewModel via App.PushPageAsync.
        // App.PushPageAsync overwrites the page's DataContext with
        // the caller-built VM, so the parameterless ctor is enough
        // here — the parametrised ctors stay for direct test wiring.
        services.AddTransient<PostAclDialog>();
        services.AddTransient<AddCircleMemberDialog>();
        // ViewModels
        services.AddSingleton(settings);
        services.AddSingleton<YavscApiClient>(api);
        services.AddSingleton<IYavscApiClient>(api);
        services.AddSingleton(client);
        services.AddSingleton(circleClient);
        services.AddSingleton(blogAclClient);
        services.AddSingleton(userSearchClient);
        services.AddSingleton<IContactService>(contactService);
        services.AddSingleton<IUserDirectory>(userDirectory);
        services.AddTransient<MainPageViewModel>();
        services.AddTransient<HomePageViewModel>();
        services.AddTransient<SignaturePageViewModel>();
        services.AddTransient<CirclesPageViewModel>();

        // Persistent session banner: one instance for the lifetime of
        // the app so the same VM survives page navigation.
        var sessionStatus = new SessionStatusViewModel { Api = api };
        sessionStatus.Refresh();
        services.AddSingleton(sessionStatus);
        services.AddTransient<SessionStatusBanner>();

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Attach a pre-built DI container to this <see cref="App"/>
    /// instance. Used by headless tests after
    /// <see cref="BuildServices"/>; in production this happens
    /// implicitly via <see cref="OnFrameworkInitializationCompleted"/>.
    /// Idempotent w.r.t. <see cref="Settings.BindToServiceProvider"/>:
    /// re-binding from a second App boot is a no-op.
    /// </summary>
    internal void AttachServiceProvider(IServiceProvider sp)
    {
        ServiceProvider = sp;
        Settings.BindToServiceProvider(sp);
    }

    /// <summary>
    /// Test-only hook: bind a concrete <see cref="MainWindow"/> so
    /// command-driven navigation paths (<see cref="PushPage"/>) can
    /// push onto a real <see cref="NavigationPage"/> in headless
    /// fixtures that do not run the full desktop lifetime bootstrap.
    /// </summary>
    internal void AttachMainWindow(MainWindow mainWindow)
    {
        window = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
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
    public static Task PushMainPageAsync()
    {
        var app = (App)Current!;
        var mainVm = app.ServiceProvider!.GetRequiredService<MainPageViewModel>();
        return app.PushPageAsync(mainVm);
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

    internal void PushPage(ViewModelBase vm)
    {
        _ = PushPageAsync(vm);
    }

    internal async Task PushPageAsync(ViewModelBase vm)
    {
        if (window is null)
        {
            throw new InvalidOperationException("MainWindow is not initialized yet.");
        }

        var template = DataTemplates.FirstOrDefault(t => t.Match(vm));
        if (template is null)
        {
            throw new InvalidOperationException($"No IDataTemplate found for {vm.GetType().Name}.");
        }

        var view = template.Build(vm);
        if (view is null)
        {
            throw new InvalidOperationException(
                $"Template for {vm.GetType().Name} returned <null>.");
        }

        var page = view as Page;
        if (page is null)
        {
            // NavigationPage expects Page instances. Wrap any fallback control
            // (e.g. ViewLocator error TextBlock) into a ContentPage so it can render.
            page = new ContentPage { Content = view };
        }

        page.DataContext = vm;

        // Avoid stacking the same singleton page twice (e.g. SettingsPage).
        var stack = window.NavRoot.NavigationStack;
        if (stack.Count > 0 && ReferenceEquals(stack[stack.Count - 1], page))
        {
            return;
        }

        await window.NavRoot.PushAsync(page);
    }

    internal async Task GoBackAsync()
    {
        await window.NavRoot.PopAsync();
    }
}
