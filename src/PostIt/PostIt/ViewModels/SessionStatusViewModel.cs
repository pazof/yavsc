using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Services;

namespace PostIt.ViewModels;

/// <summary>
/// Persistent session banner VM, hosted in
/// <c>MainWindow.axaml</c>. Mirrors <see cref="YavscApiClient"/>'s
/// session state ("Connecté" / "Déconnecté") and exposes a
/// <c>Logout</c> command that purges the token store and asks the
/// navigation owner to route the user back to <c>HomePage</c>.
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

    [ObservableProperty]
    public partial bool IsLoggedIn { get; private set; }

    [ObservableProperty]
    public partial string SessionLabel { get; private set; } = "Déconnecté";

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
        SessionLabel = has ? "Connecté" : "Déconnecté";
    }

    [RelayCommand]
    public async System.Threading.Tasks.Task LogoutAsync()
    {
        if (Api is null) return;
        await Api.LogoutAsync().ConfigureAwait(false);
        Refresh();
        LogoutCompleted?.Invoke();
    }
}
