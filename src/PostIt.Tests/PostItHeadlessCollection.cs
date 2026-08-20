using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using PostIt.ViewModels;
using PostIt.Views;
using Yavsc.Api.Client;

namespace PostIt.Tests;

/// <summary>
/// xUnit collection grouping every headless UI test in
/// <c>PostIt.Tests</c>. The Avalonia headless harness instantiates
/// a single <see cref="PostItHeadlessFixture"/> per test class
/// (<c>IClassFixture&lt;PostItHeadlessFixture&gt;</c>); the
/// collection marker here exists for two reasons:
///
/// <list type="bullet">
///   <item><description>It documents the shared lifecycle
///   contract: every test class that opts in gets the same
///   <see cref="MainWindow"/>, the same DI service provider,
///   the same <see cref="ViewLocator"/> on
///   <see cref="Application.DataTemplates"/>, and the same
///   <see cref="PostIt.App.MainWindow"/> attachment that
///   <c>App.PushPageAsync</c> relies on.</description></item>
///   <item><description>It disables parallelisation across the
///   whole collection. The Avalonia headless platform is
///   process-global (one <see cref="Application.Current"/> per
///   process, one dispatcher per thread), so two collection
///   members running in parallel would race on the same
///   static state and produce flaky failures with no useful
///   diagnostic. Same rationale as
///   <c>JwtClaimMappingCollection</c>.</description></item>
/// </list>
///
/// Mirrors the convention used by
/// <c>Yavsc.Org.Tests.WebServerFixture</c> (collection
/// <c>"Yavsc Server"</c>) and
/// <c>Yavsc.Blogs.Tests.JwtClaimMappingCollection</c>.
/// </summary>
[CollectionDefinition("PostIt Headless")]
public sealed class PostItHeadlessCollection: IDisposable
{

    /// <summary>The DI service provider the fixture booted
    /// (production graph from <see cref="App.BuildServices"/>).
    /// Identical across every <see cref="PostItHeadlessFixture"/>
    /// instance — see class remarks.</summary>
     public IServiceProvider Services { get; private set; }

    /// <summary>The headless <see cref="MainWindow"/> for this
    /// fixture instance. Already <see cref="WindowBase.Show"/>n,
    /// so its visual tree is realised and
    /// <see cref="Button.Command"/> bindings have been
    /// evaluated. The window is per-instance, not
    /// process-shared, so each test class gets a clean nav
    /// stack out of the box.</summary>
    public MainWindow Window { get; private set; }

    /// <summary>The <see cref="App"/> instance the Avalonia
    /// headless harness set as <see cref="Application.Current"/>.
    /// Convenience accessor for tests that need to call
    /// <c>App.PushPageAsync</c> directly.</summary>
    public App App { get; private set; }

    /// <summary>The navigation surface the
    /// <see cref="MainWindow"/> hosts. Tests can read
    /// <c>NavigationStack</c> directly or call
    /// <see cref="PushAsync"/> to push onto it.</summary>
    public NavigationPage NavRoot { get; private set; }

    public PostItHeadlessCollection()
    {

        // First fixture to construct in this process:
        // build the production DI container and attach
        // it to the App.
        App = (App)Application.Current!;

        var testingServices = new ServiceCollection();
           var api = new RecordingYavscApiClient(new CallRecorder());
        var blog = new BlogApiClient(api, "http://localhost/");

        // The recording fake is sufficient on its own — no
        // production wiring needed. Swap it in via the fixture
        // so the App.PushPageAsync path resolves the same way
        // it would in production (minus the token store).

        testingServices.AddSingleton(api);
        testingServices.AddSingleton(blog);

        Services = App.BuildServices(testingServices);

        App.AttachServiceProvider(Services);

        // The ViewLocator is the single entry point
        // App.PushPageAsync uses to map a VM to a Page.
        App.DataTemplates.Clear();
        App.DataTemplates.Add(new ViewLocator(Services));

        // Per-instance window: each test class gets its own.
        Window = new MainWindow();
        App.AttachMainWindow(Window);
        Window.Show();
    }



    /// <summary>
    /// Push a view model or page onto <see cref="NavRoot"/>.
    /// Awaits the push asynchronously so the headless
    /// dispatcher can pump frames while the push is in flight;
    /// the caller can then assert on the resulting stack
    /// (<c>NavRoot.NavigationStack[^1]</c>).
    /// </summary>
    /// <remarks>
    /// Tests that want a clean stack (most of them, since
    /// the fixture's <see cref="MainWindow"/> is shared
    /// across every test class) should call
    /// <see cref="ClearNavigationStack"/> before pushing, or
    /// use <see cref="MountAsync"/> which clears by default.
    /// </remarks>
    /// <returns>The page that was pushed, so the caller can
    /// assert on its type or bind a <c>DataContext</c>.</returns>
    public Page PushAsync(object vmOrPage)
    {
        if (vmOrPage is null) throw new ArgumentNullException(nameof(vmOrPage));


        // Synchronous push: the Avalonia headless
        // NavigationPage.PushAsync returns a Task that
        // completes once the transition animation finishes,
        // and in headless that animation is driven by the
        // dispatcher pump. We block on the Task with
        // GetAwaiter().GetResult() rather than awaiting it
        // because the test body is itself running on the
        // dispatcher thread (the [AvaloniaFact] attribute
        // schedules the test there); an await would capture
        // the dispatcher as the continuation target and
        // deadlock waiting for the push to complete on a
        // thread that's already busy running the test.
        if (vmOrPage is Page page)
        {
            Window.NavRoot.PushAsync(page).GetAwaiter().GetResult();
            // Pump the dispatcher once so the pushed page
            // is actually on NavigationStack (the Awaiter
            // above unblocks before the stack is updated).
            Dispatcher.UIThread.RunJobs();
            return page;
        }

        // VM push: route through the production
        // App.PushPageAsync pipeline.
        var app = (App)Application.Current!;
        var vm = (ViewModelBase)vmOrPage;
        app.PushPageAsync(vm).GetAwaiter().GetResult();
        Dispatcher.UIThread.RunJobs();
        return Window.NavRoot.NavigationStack[^1];
    }

    public void Dispose()
    {
            // Last fixture out: tear the shared state down so
            // the next test run starts clean. We don't shut
            // down the Avalonia headless platform — that's
            // owned by the [AvaloniaTestApplication] attribute
            // on TestAppBuilder and gets torn down when the
            // process exits.
            try
            {
                var app = (App)Application.Current!;
                app.DataTemplates.Clear();
            }
            catch { /* best effort */ }
            Services = null;
    }
}
