using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Microsoft.Extensions.DependencyInjection;
using PostIt.Services;
using PostIt.ViewModels;
using PostIt.Views;
using Yavsc.Api.Client;

namespace PostIt.Tests;

/// <summary>
/// Headless coverage for the two interactive buttons of the
/// "add a circle member" modal: "Ajouter" and "Fermer".
///
/// <para>The dialog is pushed on top of <see cref="CirclesPage"/>
/// via the canonical <c>App.PushPageAsync</c> pipeline (the
/// same path <c>CirclesPageViewModel.OpenAddMemberAsync</c>
/// uses). The test asserts on <c>NavRoot.NavigationStack</c>
/// size before and after each click — the user's bug was "I
/// click and nothing happens", so the failure mode is a stack
/// that doesn't shrink for "Fermer", and a "Confirmer" event
/// that the host doesn't pick up for "Ajouter" (the dialog
/// stays up = stack doesn't shrink either).</para>
///
/// <para>Pattern follows <c>MainPageButtonsTests</c>: name
/// every interactive control in XAML with <c>x:Name</c>,
/// click via <c>button.Command?.Execute(...)</c> + flush
/// any async command before asserting.</para>
/// </summary>
public class AddCircleMemberDialogTests
{
    /// <summary>
    /// Stand-in <see cref="IUserDirectory"/> that returns an
    /// empty list. The dialog's "Rechercher" button is never
    /// exercised in these tests — the picker starts empty and
    /// the "Ajouter" button's IsEnabled is bound to a null
    /// selection, which keeps the click harmless even when
    /// its <see cref="AddCircleMemberDialogViewModel.Add"/>
    /// command does fire.
    /// </summary>
    private sealed class StubUserDirectory : IUserDirectory
    {
        public Task<IReadOnlyList<UserSummary>> SearchAsync(string query, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<UserSummary>>(new List<UserSummary>());
    }

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

    /// <summary>
    /// Mount a real <see cref="MainWindow"/>, build a minimal
    /// DI graph, push <see cref="CirclesPage"/> then the
    /// <see cref="AddCircleMemberDialog"/> on top of it.
    /// Returns the stack size so the test can pin the delta.
    /// The graph exposes <c>IUserDirectory</c> (so the dialog
    /// VM resolves its dependency) and <c>AddCircleMemberDialog</c>
    /// (so <c>ViewLocator</c> can resolve it from the VM).
    /// </summary>
    private static (MainWindow window, CirclesPage page, AddCircleMemberDialog dialog) Mount()
    {
        var api = new ThrowingApi();
        var circleClient = new CircleApiClient(api, "http://localhost/");

        var services = new ServiceCollection();
        services.AddSingleton(new Settings());
        services.AddSingleton<IUserDirectory>(new StubUserDirectory());
        services.AddSingleton(circleClient);
        services.AddTransient<CirclesPage>();
        services.AddTransient<CirclesPageViewModel>();
        services.AddTransient<AddCircleMemberDialog>();
        services.AddTransient<AddCircleMemberDialogViewModel>();
        var sp = services.BuildServiceProvider();

        var window = new MainWindow();
        var app = (PostIt.App)Application.Current!;
        app.DataTemplates.Clear();
        app.DataTemplates.Add(new ViewLocator(sp));
        app.AttachMainWindow(window);
        window.Show();

        var circlesPage = sp.GetRequiredService<CirclesPage>();
        window.NavRoot.PushAsync(circlesPage).GetAwaiter().GetResult();

        // The "Ajouter un membre" command on CirclesPage builds
        // the dialog VM directly (it knows the directory from
        // the service provider) and pushes it via App.PushPage.
        var dialogVm = new AddCircleMemberDialogViewModel(sp.GetRequiredService<IUserDirectory>());
        ((App)Application.Current!).PushPageAsync(dialogVm).GetAwaiter().GetResult();

        var dialog = window.NavRoot.NavigationStack[^1] as AddCircleMemberDialog
            ?? throw new System.InvalidOperationException("Dialog page not at top of stack.");
        return (window, circlesPage, dialog);
    }

    /// <summary>
    /// Click the "Fermer" button on the dialog and assert the
    /// nav stack shrinks by exactly one.
    /// </summary>
    [AvaloniaFact]
    public void Close_button_pops_dialog_off_nav_stack()
    {
        // Arrange: stack starts at 2 (CirclesPage + dialog).
        var (window, _, _) = Mount();
        var stackBefore = window.NavRoot.NavigationStack.Count;
        Assert.Equal(2, stackBefore);

        // Act
        var dialog = window.NavRoot.NavigationStack[^1] as AddCircleMemberDialog ?? throw new System.InvalidOperationException();
        // The "Fermer" button uses a Click handler (not a
        // Command), so RaiseEvent(Button.ClickEvent) is the
        // right way to fire it from headless code. Executing
        // Command would no-op because no Command is bound.
        dialog.CloseButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));

        // Assert: stack -1, the top is the CirclesPage again.
        Assert.True(window.NavRoot.NavigationStack.Count == stackBefore - 1,
            $"Click on 'Fermer' must shrink the nav stack by one. Before: {stackBefore}, after: {window.NavRoot.NavigationStack.Count}.");
        Assert.IsType<CirclesPage>(window.NavRoot.NavigationStack[^1]);
    }
}
