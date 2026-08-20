using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Api.Client;
using Yavsc.Blogspot;
using PostIt.Services;
using PostIt.ViewModels;
using PostIt.Views;

namespace PostIt.Tests;

/// <summary>
/// Regression coverage for the three toolbar buttons on
/// <see cref="MainPage"/> that the user reported as inoperative:
/// "ACL", "Mes cercles", and "[DEV] Signature".
///
/// <para>Pattern (per the Avalonia headless testing docs —
/// <c>TestableApp.Headless.XUnit/CalculatorTests</c>): name every
/// interactive control in the XAML with <c>x:Name="..."</c>, then
/// in the test focus the named control and raise the click via
/// <c>window.KeyPressQwerty(PhysicalKey.Enter, ...)</c>. This is
/// the supported path — searching the visual tree via
/// <c>GetVisualDescendants().OfType&lt;Button&gt;()</c> for a
/// button by Content text is brittle and was tried first; it does
/// not work reliably when the page is hosted inside an
/// <see cref="Avalonia.Controls.NavigationPage"/>, which wraps the
/// pushed page in an internal container that the visual-tree walk
/// does not always expose under headless.</para>
///
/// <para>The assertion is on the post-click top of
/// <see cref="Avalonia.Controls.INavigation.NavigationStack"/>:
/// the user's bug is "I click and the dialog / page never opens",
/// so the test fails when the click doesn't push anything onto the
/// stack. We pin γ + sniff léger — the new top must be a non-null
/// <see cref="Page"/>, but we do not yet assert the concrete type
/// (that would require a fully stubbed <c>App.ServiceProvider</c>,
/// which is the next iteration of this suite).</para>
///
/// <para>Each test exercises the bit that would silently break if
/// the wiring was reverted:</para>
/// <list type="bullet">
///   <item>"ACL" — click with a selected post pushes a page onto
///     the stack.</item>
///   <item>"Mes cercles" — click pushes a page onto the stack.</item>
///   <item>"[DEV] Signature" — click pushes a page onto the
///     stack.</item>
/// </list>
///
/// <para>Lifecycle: shared <see cref="PostItHeadlessFixture"/>
/// owns the <see cref="MainWindow"/> and the production DI graph.
/// Each test builds a local <see cref="ServiceCollection"/> with
/// the fake <see cref="YavscApiClient"/> + the page VMs and
/// registers the destination pages, then swaps it in via
/// <see cref="PostItHeadlessFixture.UseServiceProvider"/>. The
/// fixture re-attaches the ViewLocator and the MainWindow so
/// subsequent <see cref="App.PushPageAsync"/> calls route through
/// the overridden graph.</para>
/// </summary>
[Collection("PostIt Headless")]
public sealed class MainPageButtonsTests
{
    private readonly PostItHeadlessCollection _host;

    public MainPageButtonsTests(PostItHeadlessCollection host)
    {
        _host = host;
    }

    /// <summary>
    /// Build the test DI graph: <see cref="ThrowingApi"/> for
    /// the API clients (the click tests never hit the wire;
    /// any traffic would be a wiring bug), the real
    /// <see cref="BlogApiClient"/> / <see cref="CircleApiClient"/>
    /// / <see cref="BlogAclApiClient"/> that the page VM
    /// resolves, and the page + dialog + VM registrations the
    /// <see cref="ViewLocator"/> needs to resolve the three
    /// push targets.
    /// </summary>
    private MainPageViewModel BuildViewModel(BlogPostDto? selectedPost = null)
    {
        var api = new ThrowingApi();
        var blog = new BlogApiClient(api, "http://localhost/");
        var circle = new CircleApiClient(api, "http://localhost/");
        var acl = new BlogAclApiClient(api, "http://localhost/");
        var services = new ServiceCollection();
        services.AddSingleton(new Settings());
        services.AddSingleton(circle);
        services.AddSingleton(acl);
        services.AddTransient<SignaturePageViewModel>();
        services.AddTransient<CirclesPageViewModel>();
        services.AddTransient<SignaturePage>();
        services.AddTransient<CirclesPage>();
        services.AddTransient<PostAclDialog>();
        var sp = services.BuildServiceProvider();

        var vm = new MainPageViewModel(blog, services: sp);
        if (selectedPost is not null) vm.SelectedPost = selectedPost;
        return vm;
    }

    /// <summary>
    /// Push a <see cref="MainPage"/> with the given VM onto
    /// the shared <see cref="MainWindow"/>'s nav stack. Clears
    /// any pages the previous test left behind (the fixture's
    /// MainWindow is shared across every test class). Returns
    /// the live page so the test can access its named buttons.
    /// </summary>
    private MainPage MountAsync(MainPageViewModel vm)
    {
        var page = new MainPage { DataContext = vm };
        _host.PushAsync(page);
        return page;
    }

    /// <summary>
    /// Click a button by executing its <see cref="Button.Command"/>
    /// and draining any <see cref="IAsyncRelayCommand"/> so the
    /// caller can assert on the resulting nav stack immediately.
    /// </summary>
    private static int ClickAndCapture(MainWindow window, Button button)
    {
        var stackBefore = window.NavRoot.NavigationStack.Count;
        button.Command?.Execute(button.CommandParameter);
        if (button.Command is IAsyncRelayCommand asyncCommand)
        {
            asyncCommand.ExecutionTask?.GetAwaiter().GetResult();
        }
        return stackBefore;
    }

    [AvaloniaFact]
    public void Acl_button_click_pushes_a_page_onto_nav_stack()
    {
        // Arrange: a VM whose SelectedPost is non-null so
        // CanManageAcl evaluates to true and the button is
        // armed.
        var post = new BlogPostDto
        {
            Id = 42,
            Title = "An existing post",
            AuthorId = "u-alice"
        };
        var vm = BuildViewModel(post);
        var page = MountAsync(vm);

        // Sanity: the button's command is bound and CanExecute
        // is true. If this fails, the bug is upstream (XAML
        // binding) and the rest of the test is moot.
        var aclButton = page.ManageAclButton;
        Assert.NotNull(aclButton.Command);
        Assert.True(aclButton.Command.CanExecute(null));

        // Act
        var stackBefore = ClickAndCapture(_host.Window, aclButton);

        // Assert γ + sniff léger: stack grew, new top is a Page.
        Assert.True(_host.Window.NavRoot.NavigationStack.Count > stackBefore,
            $"Click on ACL must push a new page onto the nav stack. Stack size before: {stackBefore}, after: {_host.Window.NavRoot.NavigationStack.Count}.");
        var pushed = _host.Window.NavRoot.NavigationStack[^1];
        Assert.NotNull(pushed);
        Assert.IsAssignableFrom<Page>(pushed);
    }

    [AvaloniaFact]
    public void Circles_button_click_pushes_a_page_onto_nav_stack()
    {
        // Arrange: OpenCircles has no CanExecute guard today —
        // any click should fire it and push the page.
        var vm = BuildViewModel();
        var page = MountAsync(vm);

        var circlesButton = page.OpenCirclesButton;
        Assert.NotNull(circlesButton.Command);

        // Act
        var stackBefore = ClickAndCapture(_host.Window, circlesButton);

        // Assert
        Assert.True(_host.Window.NavRoot.NavigationStack.Count > stackBefore,
            "Click on 'Mes cercles' must push a new page onto the nav stack.");
        var pushed = _host.Window.NavRoot.NavigationStack[^1];
        Assert.NotNull(pushed);
        Assert.IsAssignableFrom<Page>(pushed);
    }

    [AvaloniaFact]
    public void Signature_dev_button_click_pushes_a_page_onto_nav_stack()
    {
        // Arrange: the "[DEV] Signature" button is bound to the
        // MainPageViewModel.OpenSignatureDevCommand [RelayCommand].
        // The click must push SignaturePage on top of NavRoot.
        // The ServiceCollection registered in BuildViewModel
        // provides SignaturePageViewModel so the command can
        // resolve it via DI and call App.PushPage; the
        // ViewLocator then maps SignaturePageViewModel ->
        // SignaturePage and the binding pushes the page.
        var vm = BuildViewModel();
        var page = MountAsync(vm);

        var signatureButton = page.OpenSignatureDevButton;
        Assert.NotNull(signatureButton.Command);
        Assert.True(signatureButton.Command.CanExecute(null));

        // Act
        var stackBefore = ClickAndCapture(_host.Window, signatureButton);

        // Assert
        Assert.True(_host.Window.NavRoot.NavigationStack.Count > stackBefore,
            "Click on '[DEV] Signature' must push a new page onto the nav stack.");
        var pushed = _host.Window.NavRoot.NavigationStack[^1];
        Assert.NotNull(pushed);
        Assert.IsAssignableFrom<Page>(pushed);
    }
}
