using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using PostIt.Services;
using PostIt.ViewModels;
using PostIt.Views;
using PostIt.Helpers;
using System.Globalization;
using Live.Avalonia;
using System.Diagnostics;
using System.Linq;
using ReactiveUI;
using System.Reactive;

namespace PostIt;

public partial class App : Application, ILiveView, IVMPusherApp
{
    public static bool LiveMode { get; private set; }
    private int _bootStarted;
    public static string? CliConfigFileSpecification { get; private set; }

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

#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (TryHandOffCustomSchemeUrl()) return;

        this.ServiceProvider = new ServiceCollection().BuildPostItServices();
        var settings = ServiceProvider.GetRequiredService<Settings>();

        if (!LiveMode)
        {
            // Debugging requires pdb loading etc, so we disable live reloading
            // during a test run with an attached debugger.

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                InitializeClassicDesktopLifetime(settings, desktop);
            }
            else if (ApplicationLifetime is IActivityApplicationLifetime singleViewFactoryApplicationLifetime)
            {
                InitializeActivityLifetime(settings, singleViewFactoryApplicationLifetime);
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                InitializeSingleViewLifetime(settings, singleViewPlatform);
            }
        }
        else
        {
            // Here, we create a new LiveViewHost, located in the 'Live.Avalonia'
            // namespace, and pass an ILiveView implementation to it. The ILiveView
            // implementation should have a parameterless constructor! Next, we
            // start listening for any changes in the source files. And then, we
            // show the LiveViewHost window. Simple enough, huh?
            var window = new LiveViewHost(this, Console.WriteLine);
            var view = ServiceProvider!.GetRequiredService<MainView>();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                InitializeClassicDesktopLifetime(window, view, settings, desktop);
            }
            else if (ApplicationLifetime is IActivityApplicationLifetime singleViewFactoryApplicationLifetime)
            {
                InitializeActivityLifetime(settings, singleViewFactoryApplicationLifetime);
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                InitializeSingleViewLifetime(settings, singleViewPlatform);
            }

            window.StartWatchingSourceFilesForHotReloading();
            Console.WriteLine("LiveViewHost started for hot reloading.");
            window.Show();
        }


#if DEBUG
        // Here we subscribe to ReactiveUI default exception handler to avoid app
        // termination in case if we do something wrong in our view models. See:
        // https://www.reactiveui.net/docs/handbook/default-exception-handler/
        //
        // In case if you are using another MV* framework, please refer to its
        // documentation explaining global exception handling.
        Console.WriteLine("Debug mode: ReactiveUI default exception handler is set.");
        RxApp.DefaultExceptionHandler = Observer.Create<Exception>(Console.WriteLine);
#endif

        base.OnFrameworkInitializationCompleted();
    }

    private bool IsProduction()
    {
#if DEBUG
        return false;
#else
    return true;
#endif
    }

    public async Task<Page> PushPageAsync(ViewModelBase viewModel)
    {
        var template = DataTemplates.FirstOrDefault(t => t.Match(viewModel));
        if (template is null)
        {
            throw new InvalidOperationException($"No IDataTemplate found for {viewModel.GetType().Name}.");
        }

        Control? view = template.Build(viewModel) ;
        if (view is null)
        {
            throw new InvalidOperationException(
                $"Template for {viewModel.GetType().Name} returned <null>.");
        }

        var page = view as Page;
        if (page is null)
        {
            // NavigationPage expects Page instances. Wrap any fallback control
            // (e.g. ViewLocator error TextBlock) into a ContentPage so it can render.
            page = new ContentPage { Content = view };
        }

        page.DataContext = viewModel;

        MainView? mainView = (Current as App).View;

        // Avoid stacking the same singleton page twice (e.g. SettingsPage).
        var stack = mainView.NavRoot.NavigationStack;
        if (stack.Count > 0 && ReferenceEquals(stack[stack.Count - 1], page))
        {
            return page;
        }

        await mainView.NavRoot.PushAsync(page);
        return page;
    }

    public void InitializeSingleViewLifetime(Settings settings, ISingleViewApplicationLifetime singleViewPlatform)
    {
        singleViewPlatform.MainView = View = ServiceProvider!.GetRequiredService<MainView>();
        ConfigureRootView(View);
        ApplyStettings(settings);
    }

    public void InitializeActivityLifetime(Settings settings, IActivityApplicationLifetime singleViewFactoryApplicationLifetime)
    {
        singleViewFactoryApplicationLifetime.MainViewFactory =
                        () =>
                        {
                            this.ConfigureRootView(ServiceProvider!.GetRequiredService<MainView>());
                            ApplyStettings(settings);
                            return View!;
                        };
    }

    public void InitializeClassicDesktopLifetime(Settings settings, IClassicDesktopStyleApplicationLifetime desktop)
    {
        var window = ServiceProvider!.GetRequiredService<MainWindow>();
        var mainView = (MainView)CreateView(window);
        InitializeClassicDesktopLifetime(window, mainView, settings, desktop);
    }

    public void InitializeClassicDesktopLifetime(Window window, MainView mainView,  Settings settings, IClassicDesktopStyleApplicationLifetime desktop)
    {
        this.ConfigureRootView(mainView);
        ApplyStettings(settings);
        desktop.MainWindow = window;
        window.Content = mainView;
        window.Show();
    }

    public void ConfigureRootView(MainView rootView)
    {
        rootView.AttachedToVisualTree += (_, _) =>
            Dispatcher.UIThread.Post(() => _ = BootOnceAsync());

        var sessionStatus = ServiceProvider!.GetRequiredService<SessionStatusViewModel>();
        sessionStatus.LogoutCompleted += () =>
        {
            // Remplacer Window.NavRoot par rootView.NavRoot
            rootView.NavRoot.PopToRootAsync();
        };
        if (rootView.DataContext==null)
            rootView.DataContext = ServiceProvider!.GetRequiredService<HomePageViewModel>();

        rootView.SessionBanner.DataContext = sessionStatus;
        this.View = rootView;
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

    private static void ApplyStettings(Settings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.SelectedLanguage))
            Yavsc.Lang.Resources.Culture = new CultureInfo(settings.SelectedLanguage);
        Current!.RequestedThemeVariant =
            settings.DarkMode ? ThemeVariant.Dark : ThemeVariant.Light;
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
        var homePageVm = provider.GetRequiredService<HomePageViewModel>();
        var app = (App)Current!;
        await app.PushPageAsync(homePageVm);
    }

    /// <summary>
    /// Resolve a fresh <c>MainPageViewModel</c> from DI and push its
    /// mapped page (via <see cref="ViewLocator"/>) on top
    /// of the current navigation stack. Used both by <see cref="BootAsync"/>
    /// (silent refresh at boot) and by <c>SessionStatusViewModel.LoginSucceeded</c>
    /// (interactive login from the banner). Pulled out as a helper so
    /// the two callers can't drift apart.
    /// </summary>
    public static async Task PushBlogsPageAsync()
    {
        var app = (App)Current!;
        var mainVm = app.ServiceProvider!.GetRequiredService<BlogsViewModel>();
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

    internal static void UseConfigFileWhenLoading(string configFile)
    {
        CliConfigFileSpecification = configFile;
    }

    internal static void EnableLiveMode()
    {
        LiveMode = true;
    }

    public object CreateView(Window window)
    {
        if (this.ServiceProvider == null)
        {
            var serviceCollection = new ServiceCollection();
            this.ServiceProvider = serviceCollection.BuildPostItServices();
        }
        if (window.DataContext == null)
            window.DataContext = ServiceProvider!.GetRequiredService<HomePageViewModel>();

        // The AppView class will inherit the DataContext
        // of the window. The AppView class can be a
        // UserControl, a Grid, a TextBlock, whatever.
        this.View = ServiceProvider!.GetRequiredService<MainView>();
        this.ConfigureRootView(this.View);
        return this.View;
    }
}
