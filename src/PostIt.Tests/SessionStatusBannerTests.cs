using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using PostIt.ViewModels;

namespace PostIt.Tests;

/// <summary>
/// UI tests for <see cref="SessionStatusBanner"/>. The shared
/// <see cref="PostItHeadlessFixture"/> provides the headless
/// <see cref="MainWindow"/> already attached to <see cref="App"/>
/// and shown, so each test only has to wire its
/// <see cref="SessionStatusViewModel"/> onto
/// <c>MainWindow.SessionBanner</c> and assert on the rendered
/// tree.
///
/// <para>The session banner's <c>DataContext</c> is not wired
/// by <see cref="App.OnFrameworkInitializationCompleted"/> in
/// these tests: production wires it at composition time, but a
/// unit test runs against a freshly-built <see cref="App"/> so
/// we set the <c>DataContext</c> on the banner directly. The
/// production code path is exercised end-to-end by the manual
/// launch, not here.</para>
///
/// <para>Pattern: <c>[AvaloniaFact]</c> (from
/// <c>Avalonia.Headless.XUnit</c>) instead of plain
/// <c>[Fact]</c> because the AvaloniaFact attribute schedules
/// the test body inside a dispatcher, which is the precondition
/// for the headless Window's
/// <c>PlatformManager.CreateWindow()</c> to find a registered
/// service. A plain <c>[Fact]</c> test that calls
/// <c>new Window().Show()</c> throws because the harness has
/// not been initialised for that thread.</para>
/// </summary>
[Collection("PostIt Headless")]
public sealed class SessionStatusBannerTests
{
    private readonly PostItHeadlessCollection _host;

    public SessionStatusBannerTests(PostItHeadlessCollection host)
    {
        _host = host;
    }

    [AvaloniaFact]
    public void Banner_renders_three_buttons_in_the_visual_tree()
    {
        var banner = _host.Window.SessionBanner;
        banner.DataContext = new SessionStatusViewModel();

        var buttons = banner.GetVisualDescendants()
            .OfType<Button>()
            .ToList();

        // Three buttons, named by their content text: Se
        // déconnecter, Se connecter, Paramètres. If any one is
        // missing, the user has no way to trigger the
        // corresponding navigation event.
        Assert.Equal(3, buttons.Count);
        Assert.Contains(buttons, b => b.Content as string == "Se déconnecter");
        Assert.Contains(buttons, b => b.Content as string == "Se connecter");
        Assert.Contains(buttons, b => b.Content as string == "Paramètres");
    }

    [AvaloniaFact]
    public void Banner_login_button_is_visible_when_logged_out()
    {
        var banner = _host.Window.SessionBanner;
        var vm = new SessionStatusViewModel();
        Assert.True(vm.IsLoggedOut);  // VM default
        banner.DataContext = vm;

        var login = banner.GetVisualDescendants()
            .OfType<Button>()
            .Single(b => b.Content as string == "Se connecter");

        // The XAML binds IsVisible to IsLoggedOut. After the
        // banner is on the realised visual tree, the binding
        // has been evaluated.
        Assert.True(login.IsVisible);
    }

    [AvaloniaFact]
    public void Banner_logout_button_is_hidden_when_logged_out()
    {
        var banner = _host.Window.SessionBanner;
        var vm = new SessionStatusViewModel();
        Assert.False(vm.IsLoggedIn);  // VM default
        banner.DataContext = vm;

        var logout = banner.GetVisualDescendants()
            .OfType<Button>()
            .Single(b => b.Content as string == "Se déconnecter");

        Assert.False(logout.IsVisible);
    }

    [AvaloniaFact]
    public void Banner_settings_button_is_visible_regardless_of_session()
    {
        var banner = _host.Window.SessionBanner;
        banner.DataContext = new SessionStatusViewModel();

        var settings = banner.GetVisualDescendants()
            .OfType<Button>()
            .Single(b => b.Content as string == "Paramètres");

        // Paramètres is the only button with no IsVisible
        // binding — always shown. The user's only path to the
        // settings page goes through this button.
        Assert.True(settings.IsVisible);
    }

    [AvaloniaFact]
    public void Banner_session_label_reflects_DataContext()
    {
        var banner = _host.Window.SessionBanner;
        banner.DataContext = new SessionStatusViewModel();

        var label = banner.GetVisualDescendants()
            .OfType<TextBlock>()
            .First(t => t.Text == "Déconnecté" || t.Text == "Connecté");

        // Default SessionLabel is "Déconnecté" until Refresh()
        // is called with a valid session. This pins the default
        // so a future refactor that breaks the initial value
        // (e.g. by removing the field initialiser) is caught.
        Assert.Equal("Déconnecté", label.Text);
    }
}
