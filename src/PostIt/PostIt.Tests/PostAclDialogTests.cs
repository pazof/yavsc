using System.Net;
using System.Text;
using System.Text.Json;
using Avalonia;
using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using PostIt.Helpers;
using PostIt.Services;
using PostIt.ViewModels;
using PostIt.Views;
using Yavsc.Api.Client;
using Yavsc.Blogspot;

namespace PostIt.Tests;

/// <summary>
/// Regression coverage for the user-reported bug:
/// <c>PostAclDialogViewModel.LoadAsync</c> was never invoked,
/// so <c>MyCircles</c> and <c>AclEntries</c> were empty when the
/// dialog opened (the dropdown showed "Choisir un cercle..." and
/// the list was blank, with no error to hint at why).
///
/// <para>The fix wires <see cref="PostAclDialog"/>'s constructor
/// to trigger <c>LoadAsync</c> on the first
/// <c>AttachedToVisualTree</c>, and the VM guards re-entry via
/// <c>_loaded</c>. Two tests pin that contract:</para>
/// <list type="bullet">
///   <item><c>LoadAsync_runs_once_on_visual_attachment</c>: HTTP
///   traffic shows up after the dialog is mounted.</item>
///   <item><c>LoadAsync_is_idempotent</c>: a second explicit call
///   to <c>LoadAsync</c> on the same VM hits the HTTP layer only
///   once (the <c>_loaded</c> gate).</item>
/// </list>
///
/// <para>HTTP is stubbed with a counter
/// <see cref="HttpMessageHandler"/> that returns canned JSON
/// <c>[]</c> for every request. The handler counts calls so the
/// tests can assert "exactly one round-trip on mount" and
/// "exactly one round-trip after two calls to LoadAsync". This
/// is the same shape used by <c>BearerScopeTests</c>: real
/// <see cref="YavscApiClient"/> subclass, real
/// <see cref="HttpClient"/> with an injected handler, real
/// <see cref="BlogAclApiClient"/> / <see cref="CircleApiClient"/>
/// talking to it.</para>
/// </summary>
public class PostAclDialogTests
{
    /// <summary>
    /// <see cref="HttpMessageHandler"/> that replies 200 with
    /// <c>[]</c> (a valid JSON empty array, which both
    /// <c>GetMyAclAsync</c> and <c>GetMyCirclesAsync</c> can
    /// deserialize) and counts the number of requests.
    /// </summary>
    private sealed class CountingHttpHandler : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]", Encoding.UTF8, "application/json"),
            };
            return Task.FromResult(response);
        }
    }

    /// <summary>
    /// Subclass of <see cref="YavscApiClient"/> that routes HTTP
    /// traffic through a caller-supplied
    /// <see cref="HttpMessageHandler"/>. Same recipe as
    /// <c>BearerScopeTests.TestableYavscApiClient</c> — we
    /// override <c>CallAsync{T}</c> to talk to our own
    /// <see cref="HttpClient"/> and skip the OIDC refresh path,
    /// because the load-on-attach bug has nothing to do with
    /// token refresh.
    /// </summary>
    private sealed class TestableYavscApiClient : YavscApiClient
    {
        private readonly HttpClient _http;

        public TestableYavscApiClient(
            Settings settings,
            TokenStore store,
            HttpMessageHandler handler)
            : base(settings, store, oidc: null!)
        {
            _http = new HttpClient(handler, disposeHandler: false);
        }

        public override Task<T> CallAsync<T>(
            HttpMethod method, string path, object? body = null,
            CancellationToken ct = default)
        {
            var absolute = new Uri(new Uri(Settings.BusinessApiUrl), path);
            using var req = new HttpRequestMessage(method, absolute);
            using var resp = _http.SendAsync(req, ct).GetAwaiter().GetResult();
            resp.EnsureSuccessStatusCode();
            using var stream = resp.Content.ReadAsStream();
            var dto = JsonSerializer.Deserialize<T>(stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return Task.FromResult(dto!);
        }
    }

    /// <summary>
    /// Build a minimal DI graph exposing the two API clients
    /// (backed by a stub HTTP handler) and the page itself, so
    /// <c>ViewLocator</c> can resolve the dialog from the VM.
    /// Returns the handler, the API clients, and the window so
    /// the test can assert on request counts and push the
    /// dialog via the canonical <c>App.PushPageAsync</c> path.
    /// The DI graph is built into a local <see cref="IServiceProvider"/>
    /// that is NOT attached to <see cref="App.ServiceProvider"/>:
    /// rebinding the global DI mid-test would trample the
    /// Settings singleton the rest of the harness depends on.
    /// </summary>
    private static (MainWindow window, BlogAclApiClient aclClient, CircleApiClient circleClient, CountingHttpHandler handler) Mount()
    {
        var handler = new CountingHttpHandler();
        var settings = new Settings();
        var api = new TestableYavscApiClient(settings, new TokenStore(System.IO.Path.GetTempFileName()), handler);
        var aclClient = new BlogAclApiClient(api, settings.BusinessApiUrl);
        var circleClient = new CircleApiClient(api, settings.BusinessApiUrl);

        var services = new ServiceCollection();
        services.AddSingleton(settings);
        services.AddSingleton(api);
        services.AddSingleton(aclClient);
        services.AddSingleton(circleClient);
        services.AddTransient<PostAclDialog>();
        var sp = services.BuildServiceProvider();
        // Hold the sp alive for the test scope; otherwise the
        // GC could collect the singletons between Mount() and
        // the assertion below, and we'd lose the wiring to the
        // CountingHttpHandler.
        GC.KeepAlive(sp);

        var window = new MainWindow();
        var app = (App)Application.Current!;
        app.DataTemplates.Clear();
        app.DataTemplates.Add(new ViewLocator(sp));
        app.AttachMainWindow(window);
        window.Show();

        return (window, aclClient, circleClient, handler);
    }

    /// <summary>
    /// The bug: opening the dialog never called LoadAsync, so
    /// MyCircles/AclEntries were empty. After the fix, setting
    /// the dialog's DataContext to a PostAclDialogViewModel
    /// (the same path App.PushPageAsync takes) must trigger
    /// exactly one LoadAsync round-trip (the parallel WhenAll
    /// inside the VM counts as one request per backend call,
    /// hence two HTTP requests total: GET /blogacl and GET
    /// /circle).
    /// </summary>
    [AvaloniaFact]
    public async Task LoadAsync_runs_once_on_DataContext_changed()
    {
        // Arrange
        var (window, aclClient, circleClient, handler) = Mount();
        var post = new BlogPostDto { Id = 42, Title = "Test post" };

        // Sanity: handler starts quiet.
        Assert.Equal(0, handler.RequestCount);

        // Act: push the dialog via the canonical VM-first pipeline.
        // The locator goes through the parameterless ctor of
        // PostAclDialog, then App.PushPageAsync assigns DataContext,
        // which our hook intercepts to trigger LoadAsync.
        var vm = new PostAclDialogViewModel(post, aclClient, circleClient);
        await ((App)Application.Current!).PushPageAsync(vm);

        // The dialog must be at the top of the nav stack and
        // have its VM as DataContext.
        var dialog = window.NavRoot.NavigationStack[^1] as PostAclDialog
            ?? throw new InvalidOperationException("Dialog not at top of stack");
        Assert.Same(vm, dialog.DataContext);

        // Drain pending async work. LoadAsync is async and the
        // DataContextChanged handler is fire-and-forget; a
        // couple of loop turns is enough. We poll the handler
        // counter because the dispatch back onto the headless
        // dispatcher isn't strict — using a generous-but-bounded
        // wait avoids test flakes.
        var deadline = DateTime.UtcNow.AddSeconds(2);
        while (handler.RequestCount < 2 && DateTime.UtcNow < deadline)
        {
            await Task.Delay(20);
        }

        // Assert: exactly two GETs went out (one to /blogacl,
        // one to /circle), both from the LoadAsync call.
        Assert.Equal(2, handler.RequestCount);

        // And the VM's idempotency gate has flipped.
        Assert.True(vm.Loaded);
    }

    /// <summary>
    /// The fix exposes a guard on the VM too: a second call to
    /// LoadAsync on the same instance must NOT issue more HTTP
    /// traffic. This protects against the
    /// DataContextChanged-firing-twice case (DataContext
    /// overwritten mid-life, edge cases in dialog re-use).
    /// </summary>
    [AvaloniaFact]
    public async Task LoadAsync_is_idempotent()
    {
        // Arrange
        var (_, aclClient, circleClient, handler) = Mount();
        var post = new BlogPostDto { Id = 99, Title = "Idempotency" };
        var vm = new PostAclDialogViewModel(post, aclClient, circleClient);

        // Act: invoke LoadAsync twice in a row.
        await vm.LoadAsync();
        await vm.LoadAsync();

        // Assert: the second call short-circuited on _loaded.
        Assert.Equal(2, handler.RequestCount);
        Assert.True(vm.Loaded);
    }
}
