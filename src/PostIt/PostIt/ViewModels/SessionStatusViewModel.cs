using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Services;

namespace PostIt.ViewModels;

/// <summary>
/// Persistent session banner VM, hosted in
/// <c>MainWindow.axaml</c>. Mirrors <see cref="YavscApiClient"/>'s
/// session state ("Connecté" / "Déconnecté") and exposes a
/// <c>Logout</c> command that purges the token store and asks the
/// navigation owner to route the user back to <c>HomePage</c>, plus
/// a <c>Login</c> command that drives the OIDC interactive flow
/// and raises a <see cref="LoginSucceeded"/> event on success so
/// <c>MainWindow</c> can push <c>MainPage</c> on top of
/// <c>HomePage</c>.
///
/// Construction is deferred until the API client exists; the
/// App.axaml.cs wiring sets <see cref="Api"/> after building both,
/// so the banner reflects reality from frame zero.
/// </summary>
public partial class SessionStatusViewModel : ViewModelBase
{
    /// <summary>Raised after <see cref="LogoutAsync"/> has purged the store;
    /// <c>App.axaml.cs</c> listens and swaps the navigation root.</summary>
    public event System.Action? LogoutCompleted;

    /// <summary>Raised after <see cref="LoginAsync"/> acquired a valid session;
    /// <c>App.axaml.cs</c> listens and pushes <c>MainPage</c> on top of
    /// <c>HomePage</c> so the user lands on the blog editor.</summary>
    public event System.Action? LoginSucceeded;

    [ObservableProperty]
    public partial bool IsLoggedIn { get; private set; }

    /// <summary>Inverse of <see cref="IsLoggedIn"/>, for XAML bindings
    /// (the banner shows the Login button when the user is logged out).
    /// Updated from <see cref="Refresh"/>.</summary>
    [ObservableProperty]
    public partial bool IsLoggedOut { get; private set; } = true;

    [ObservableProperty]
    public partial string SessionLabel { get; private set; } = "Déconnecté";

    /// <summary>True while a Login flow is in flight; the Login button
    /// binds <c>IsEnabled</c> to <c>!IsBusy</c> via
    /// <see cref="LoginCommand"/>'s <c>CanExecute</c>.</summary>
    [ObservableProperty]
    public partial bool IsBusy { get; private set; }

    /// <summary>The API client backing the banner. Set once at startup;
    /// the banner polls <c>HasValidSession</c> on demand rather than
    /// subscribing to a stream — the session state only changes at
    /// login, logout, and silent refresh, all of which already
    /// re-evaluate from the same <c>_tokens</c> snapshot.</summary>
    public YavscApiClient? Api { get; set; }

    public override bool CanNavigateNext
    {
        get => throw new System.NotImplementedException();
        protected set => throw new System.NotImplementedException();
    }

    public override bool CanNavigatePrevious
    {
        get => throw new System.NotImplementedException();
        protected set => throw new System.NotImplementedException();
    }

    public void Refresh()
    {
        var has = Api?.HasValidSession ?? false;
        IsLoggedIn = has;
        IsLoggedOut = !has;
        SessionLabel = has ? "Connecté" : "Déconnecté";
    }

    /// <summary>
    /// Override the banner label with an error message. Used when
    /// an interactive login attempt fails so the operator sees
    /// something on the persistent UI without us needing a
    /// dedicated error page. The next <see cref="Refresh"/> call
    /// reverts to "Connecté" / "Déconnecté".
    /// </summary>
    public void SetError(string message)
    {
        IsLoggedIn = false;
        IsLoggedOut = true;
        SessionLabel = message;
    }

    /// <summary>
    /// Drive the OIDC interactive login. On success, refreshes
    /// the banner state and raises <see cref="LoginSucceeded"/> so
    /// the navigation owner can push <c>MainPage</c>. On failure,
    /// surfaces the error in the banner via <see cref="SetError"/>.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanLogin))]
    public async Task LoginAsync()
    {
        if (Api is null) return;
        IsBusy = true;
        try
        {
            await Api.LoginInteractiveAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            SetError($"Login failed: {ex.Message}");
            return;
        }
        finally
        {
            IsBusy = false;
        }

        Refresh();
        if (Api.HasValidSession)
            LoginSucceeded?.Invoke();
    }

    private bool CanLogin() => !IsBusy;

    partial void OnIsBusyChanged(bool value) => LoginCommand.NotifyCanExecuteChanged();

    [RelayCommand]
    public async System.Threading.Tasks.Task LogoutAsync()
    {
        if (Api is null) return;
        await Api.LogoutAsync().ConfigureAwait(false);
        Refresh();
        LogoutCompleted?.Invoke();
    }
}
