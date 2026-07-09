using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using PostIt.ViewModels;
using PostIt.Views;

namespace PostIt.Tests;

/// <summary>
/// UI tests for <see cref="SessionStatusBanner"/>. Mounted inside
/// a real <see cref="MainWindow"/> via the headless Avalonia
/// platform declared in <c>TestApp.cs</c>.
///
/// <para>The pattern is the one that <c>UnitTest1.MainPage_Should_Load</c>
/// established: a test attribute <c>[AvaloniaFact]</c> (from
/// <c>Avalonia.Headless.XUnit</c>) instead of plain <c>[Fact]</c>,
/// <c>new MainWindow()</c>, <c>window.Show()</c>. The AvaloniaFact
/// attribute schedules the test body inside a dispatcher, which
/// is the precondition for the headless Window's
/// <c>PlatformManager.CreateWindow()</c> to find a registered
/// service. A plain <c>[Fact]</c> test that calls
/// <c>new Window().Show()</c> throws because the harness has not
/// been initialised for that thread.</para>
///
/// <para>The session banner's <c>DataContext</c> is not wired in
/// these tests: <c>App.OnFrameworkInitializationCompleted</c> is
/// not called in a unit test, so we set the DataContext on the
/// banner directly. The production code path is exercised
/// end-to-end by the manual launch, not here.</para>
/// </summary>
public class SessionStatusBannerTests
{
    [AvaloniaFact]
    public void Banner_renders_three_buttons_in_the_visual_tree()
    {
        var window = new MainWindow();
        window.SessionBanner.DataContext = new SessionStatusViewModel();
        window.Show();

        var buttons = window.SessionBanner.GetVisualDescendants()
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
        var window = new MainWindow();
        var vm = new SessionStatusViewModel();
        Assert.True(vm.IsLoggedOut);  // VM default
        window.SessionBanner.DataContext = vm;
        window.Show();

        var login = window.SessionBanner.GetVisualDescendants()
            .OfType<Button>()
            .Single(b => b.Content as string == "Se connecter");

        // The XAML binds IsVisible to IsLoggedOut. After Show,
        // the binding has been evaluated.
        Assert.True(login.IsVisible);
    }

    [AvaloniaFact]
    public void Banner_logout_button_is_hidden_when_logged_out()
    {
        var window = new MainWindow();
        var vm = new SessionStatusViewModel();
        Assert.False(vm.IsLoggedIn);  // VM default
        window.SessionBanner.DataContext = vm;
        window.Show();

        var logout = window.SessionBanner.GetVisualDescendants()
            .OfType<Button>()
            .Single(b => b.Content as string == "Se déconnecter");

        Assert.False(logout.IsVisible);
    }

    [AvaloniaFact]
    public void Banner_settings_button_is_visible_regardless_of_session()
    {
        var window = new MainWindow();
        window.SessionBanner.DataContext = new SessionStatusViewModel();
        window.Show();

        var settings = window.SessionBanner.GetVisualDescendants()
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
        var window = new MainWindow();
        window.SessionBanner.DataContext = new SessionStatusViewModel();
        window.Show();

        var label = window.SessionBanner.GetVisualDescendants()
            .OfType<TextBlock>()
            .First(t => t.Text == "Déconnecté" || t.Text == "Connecté");

        // Default SessionLabel is "Déconnecté" until Refresh()
        // is called with a valid session. This pins the default
        // so a future refactor that breaks the initial value
        // (e.g. by removing the field initialiser) is caught.
        Assert.Equal("Déconnecté", label.Text);
    }
}
