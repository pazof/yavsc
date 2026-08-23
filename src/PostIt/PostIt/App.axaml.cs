using System;
using System.Linq;
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
using Yavsc.Api.Client;

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

    MainWindow window;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (TryHandOffCustomSchemeUrl()) return;

        this.ServiceProvider = BuildServices(new ServiceCollection());
        var settings = ServiceProvider.GetRequiredService<Settings>();

        DataTemplates.Clear();
        DataTemplates.Add(new ViewLocator(ServiceProvider));

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = CreateMainWindow();
        }
        else if (ApplicationLifetime is IActivityApplicationLifetime singleViewFactoryApplicationLifetime)
        {
            singleViewFactoryApplicationLifetime.MainViewFactory =
                () => CreateMainWindow();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = CreateMainWindow();
        }
        ApplyDarkMode(settings);
        base.OnFrameworkInitializationCompleted();
    }

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
        services.AddSingleton(client);
        services.AddSingleton(circleClient);
        services.AddSingleton(blogAclClient);
        services.AddSingleton(userSearchClient);
        services.AddSingleton<IContactService>(contactService);
        services.AddSingleton<IUserDirectory>(userDirectory);
        services.AddTransient<MainViewModel>();
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
        var mainVm = app.ServiceProvider!.GetRequiredService<MainViewModel>();
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
