using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
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
/// </summary>
public class MainPageButtonsTests
{
    /// <summary>
    /// Fake <see cref="YavscApiClient"/> that throws on any
    /// wire call. These tests never invoke a command that hits
    /// the API — only the click → nav side of the pipeline is
    /// asserted.
    /// </summary>
    private sealed class ThrowingApi : YavscApiClient
    {
        public ThrowingApi() : base(
            new Settings
            {
                Authentication = new AuthenticationSettings
                {
                    Authority = "https://stub.invalid",
                    ClientId = "stub",
                    Scopes = new[] { "openid" },
                },
            },
            new TokenStore(System.IO.Path.GetTempFileName()))
        { }
    }

    private static MainPageViewModel MakeViewModel(BlogPostDto? selectedPost = null)
    {
        var api = new ThrowingApi();
        var blog = new BlogApiClient(api, "http://localhost/");
        // Minimal DI graph: only what MainPageViewModel resolves
        // when the user clicks a navigation button. Today that's
        // SignaturePageViewModel (for the [DEV] Signature toolbar
        // shortcut). Anything the SignaturePage or its VM touch
        // transitively must be registered here too — the test
        // refuses to share App.BuildServices() because that one
        // constructs a real YavscApiClient pointing at the host's
        // token store, which is exactly the noise we want out of
        // a UI-driving test.
        var services = new ServiceCollection();
        services.AddTransient<SignaturePageViewModel>();
        var vm = new MainPageViewModel(blog, services: services.BuildServiceProvider());
        if (selectedPost is not null) vm.SelectedPost = selectedPost;
        return vm;
    }

    /// <summary>
    /// Mount a real <see cref="MainWindow"/> (as
    /// <c>SessionStatusBannerTests</c> does), push a
    /// <see cref="MainPage"/> with the given VM onto
    /// <c>NavRoot</c>. <c>PushAsync</c> is awaited (via
    /// <c>GetAwaiter().GetResult()</c>) so the page is on the
    /// nav stack before the test tries to interact with its
    /// named buttons. The window is shown so the visual tree is
    /// realised and <c>KeyPressQwerty</c> has a real
    /// <see cref="TopLevel"/> to dispatch against.
    /// </summary>
    private static (MainWindow window, MainPage page) MountMainPage(MainPageViewModel vm)
    {
        var window = new MainWindow();
        var page = new MainPage { DataContext = vm };
        window.Show();
        window.NavRoot.PushAsync(page).GetAwaiter().GetResult();
        return (window, page);
    }

    /// <summary>
    /// Click a button by focusing it and pressing Enter — the
    /// supported headless pattern (cf. CalculatorTests in the
    /// Avalonia.Samples repo). Returns the nav-stack count
    /// before the click so the caller can assert on the delta.
    /// KeyPressQwerty is dispatched on the <see cref="MainWindow"/>
    /// itself — it is the <see cref="TopLevel"/> that owns the
    /// headless implementation, and routing the key through any
    /// descendant TopLevel (e.g. one obtained via
    /// <c>TopLevel.GetTopLevel(button)</c>) fails with a
    /// <c>NullReferenceException</c> from the headless impl
    /// because the descendant does not carry the
    /// <c>PlatformHandle</c> the harness expects.
    /// </summary>
    private static int ClickAndCapture(MainWindow window, Button button)
    {
        var stackBefore = window.NavRoot.NavigationStack.Count;
        button.Focus();
        window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
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
        var vm = MakeViewModel(post);
        var (window, page) = MountMainPage(vm);

        // Sanity: the button's command is bound and CanExecute
        // is true. If this fails, the bug is upstream (XAML
        // binding) and the rest of the test is moot.
        var aclButton = page.ManageAclButton;
        Assert.NotNull(aclButton.Command);
        Assert.True(aclButton.Command.CanExecute(null));

        // Act
        var stackBefore = ClickAndCapture(window, aclButton);

        // Assert γ + sniff léger: stack grew, new top is a Page.
        Assert.True(window.NavRoot.NavigationStack.Count > stackBefore,
            $"Click on ACL must push a new page onto the nav stack. Stack size before: {stackBefore}, after: {window.NavRoot.NavigationStack.Count}.");
        var pushed = window.NavRoot.NavigationStack.Last();
        Assert.NotNull(pushed);
        Assert.IsAssignableFrom<Page>(pushed);
    }

    [AvaloniaFact]
    public void Circles_button_click_pushes_a_page_onto_nav_stack()
    {
        // Arrange: OpenCircles has no CanExecute guard today —
        // any click should fire it and push the page.
        var vm = MakeViewModel();
        var (window, page) = MountMainPage(vm);

        var circlesButton = page.OpenCirclesButton;
        Assert.NotNull(circlesButton.Command);

        // Act
        var stackBefore = ClickAndCapture(window, circlesButton);

        // Assert
        Assert.True(window.NavRoot.NavigationStack.Count > stackBefore,
            "Click on 'Mes cercles' must push a new page onto the nav stack.");
        var pushed = window.NavRoot.NavigationStack.Last();
        Assert.NotNull(pushed);
        Assert.IsAssignableFrom<Page>(pushed);
    }

    [AvaloniaFact]
    public void Signature_dev_button_click_pushes_a_page_onto_nav_stack()
    {
        // Arrange: the "[DEV] Signature" button is bound to the
        // MainPageViewModel.OpenSignatureDevCommand [RelayCommand].
        // The click must push SignaturePage on top of NavRoot.
        // The ServiceCollection registered in MakeViewModel provides
        // SignaturePageViewModel so the command can resolve it via
        // DI and assign it to CurrentViewModel; the ViewLocator
        // then maps SignaturePageViewModel -> SignaturePage and
        // the binding pushes the page.
        var vm = MakeViewModel();
        var (window, page) = MountMainPage(vm);

        var signatureButton = page.OpenSignatureDevButton;
        Assert.NotNull(signatureButton.Command);
        Assert.True(signatureButton.Command.CanExecute(null));

        // Act
        var stackBefore = ClickAndCapture(window, signatureButton);

        // Assert
        Assert.True(window.NavRoot.NavigationStack.Count > stackBefore,
            "Click on '[DEV] Signature' must push a new page onto the nav stack.");
        var pushed = window.NavRoot.NavigationStack.Last();
        Assert.NotNull(pushed);
        Assert.IsAssignableFrom<Page>(pushed);
    }
}
