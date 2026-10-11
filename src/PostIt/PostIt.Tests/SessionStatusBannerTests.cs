using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using PostIt.ViewModels;
using PostIt.Views;

namespace PostIt.Tests;

/// <summary>
/// UI tests for <see cref="SessionStatusBanner"/>. Mounted inside
/// a real <see cref="MainView"/> via the headless Avalonia
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
/// <para>These tests set the DataContext directly on the banner
/// mounted in the window. The production code path is exercised
/// end-to-end by the manual launch, not here.</para>
/// </summary>
public class SessionStatusBannerTests : IDisposable
{
    private MainWindow? _window;

    [AvaloniaFact]
    public void Banner_renders_four_buttons_in_the_visual_tree()
    {
        SessionStatusBanner banner = CreateBanner();

        var buttons = banner.GetVisualDescendants()
            .OfType<Button>()
            .ToList();

        Assert.Equal(4, buttons.Count);
        Assert.Contains(buttons, b => b.Content as string == "Se déconnecter");
        Assert.Contains(buttons, b => b.Content as string == "Se connecter");
        Assert.Contains(buttons, b => b.Content as string == "Paramètres");
        Assert.Contains(buttons, b => b.Content as string == "À propos");
    }

    [AvaloniaFact]
    public void Banner_login_button_is_visible_when_logged_out()
    {
        SessionStatusBanner banner = CreateBanner();

        var login = banner.GetVisualDescendants()
            .OfType<Button>()
            .Single(b => b.Content as string == "Se connecter");

        // The XAML binds IsVisible to IsLoggedOut. After Show,
        // the binding has been evaluated.
        Assert.True(login.IsVisible);
    }

    [AvaloniaFact]
    public void Banner_logout_button_is_hidden_when_logged_out()
    {
        SessionStatusBanner banner = CreateBanner();

        Assert.False(Assert.IsType<SessionStatusViewModel>(banner.DataContext).IsLoggedIn);
        var logout = banner.GetVisualDescendants()
            .OfType<Button>()
            .Single(b => b.Content as string == "Se déconnecter");

        Assert.False(logout.IsVisible);
    }

    [AvaloniaFact]
    public void Banner_settings_button_is_visible_regardless_of_session()
    {
        SessionStatusBanner banner = CreateBanner();

        var settings = banner.GetVisualDescendants()
            .OfType<Button>()
            .Single(b => b.Content as string == "Paramètres");

        // Paramètres has no IsVisible binding and is always shown.
        Assert.True(settings.IsVisible);
    }

    private SessionStatusBanner CreateBanner()
    {
        _window = new MainWindow();
        _window.Show();
        var banner = _window.GetVisualDescendants()
            .OfType<SessionStatusBanner>()
            .Single();
        banner.DataContext = new SessionStatusViewModel();
        return banner;
    }

    public void Dispose() => _window?.Close();

    [AvaloniaFact]
    public void Banner_session_label_reflects_DataContext()
    {
        SessionStatusBanner banner = CreateBanner();
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
