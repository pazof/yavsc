using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PostIt.Helpers;
using PostIt.Services;

namespace PostIt.ViewModels;

/// <summary>
/// Shared authentication state and commands for the modal and home menu.
/// </summary>
public partial class SessionStatusViewModel : ViewModelBase
{
    /// <summary>Raised after <see cref="LogoutAsync"/> has purged the store;
    /// <c>App.axaml.cs</c> listens and swaps the navigation root.</summary>
    public event System.Action? LogoutCompleted;

    /// <summary>Raised after an interactive login acquired a valid session.</summary>
    public event System.Action? LoginSucceeded;

    [ObservableProperty]
    public partial bool IsLoggedIn { get; private set; }

    [ObservableProperty]
    public partial bool IsAdministrator { get; private set; }

    private int _accessVersion;

    [ObservableProperty]
    public partial bool IsLoggedOut { get; private set; } = true;

    [ObservableProperty]
    public partial bool IsAuthenticationRequired { get; private set; }

    [ObservableProperty]
    public partial string SessionLabel { get; private set; } = "Déconnecté";

    [ObservableProperty]
    public partial string? ProfileError { get; private set; }

    [ObservableProperty]
    public partial bool IsBusy { get; private set; }

    private YavscApiClient? _api;
    public YavscApiClient? Api
    {
        get => _api;
        set
        {
            if (_api is not null) _api.SessionChanged -= OnSessionChanged;
            _api = value;
            if (_api is not null) _api.SessionChanged += OnSessionChanged;
        }
    }

    public Uri RegistrationUri => AccountUri("Register");
    public Uri ForgotPasswordUri => AccountUri("ForgotPassword");

    private Uri AccountUri(string action) => new(
        new Uri((Api?.Settings.Authentication.Authority ?? AuthenticationSettings.DefaultAuthority).TrimEnd('/') + "/"),
        "Account/" + action);

    private void OnSessionChanged(string? error)
    {
        void Update()
        {
            if (error is not null)
                SetError(error);
            else if (!IsLoggedIn || Api?.HasValidSession != true)
                _ = RefreshAsync();
        }
        if (Dispatcher.UIThread.CheckAccess()) Update();
        else Dispatcher.UIThread.Post(Update);
    }

    public async Task InitializeAsync()
    {
        if (Api is null)
            throw new InvalidOperationException("Client d'authentification indisponible.");
        if (await Api.TrySilentLoginAsync())
            await RefreshAsync();
        else
            await LoginAsync();
    }

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
        ++_accessVersion;
        IsAdministrator = false;
        var has = Api?.HasValidSession ?? false;
        IsLoggedIn = has;
        IsLoggedOut = !has;
        IsAuthenticationRequired = !has;
        ProfileError = null;
        SessionLabel = has ? "Connecté" : "Déconnecté";
    }

    public async Task RefreshAsync()
    {
        Refresh();
        var version = _accessVersion;
        var api = Api;
        if (!IsLoggedIn || api is null)
            return;

        try
        {
            var endpoint = new Uri(new Uri(api.Settings.ApiUrl.TrimEnd('/') + "/"), "account/me");
            var profile = await api.CallAsync<Yavsc.Abstract.Identity.Me>(HttpMethod.Get, endpoint.ToString());
            if (profile is null)
                throw new InvalidOperationException("Le serveur a renvoyé un profil vide.");
            if (version == _accessVersion)
                IsAdministrator = profile.Roles.Contains(Yavsc.Constants.AdminName, StringComparer.Ordinal);
        }
        catch (Exception ex)
        {
            if (version == _accessVersion)
            {
                SessionLabel = $"Impossible de vérifier les droits : {ex.Message}";
                ProfileError = SessionLabel;
            }
        }
    }

    public void SetError(string message)
    {
        ++_accessVersion;
        IsAdministrator = false;
        IsLoggedIn = false;
        IsLoggedOut = true;
        IsAuthenticationRequired = true;
        ProfileError = null;
        SessionLabel = message;
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    public async Task LoginAsync()
    {
        if (IsBusy) return;
        var api = Api;
        IsBusy = true;
        SessionLabel = "Connexion en cours dans le navigateur…";
        try
        {
            if (api is null)
                throw new InvalidOperationException("Client d'authentification indisponible.");
            await api.LoginInteractiveAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            SetError($"Connexion impossible : {ex.Message}");
            return;
        }
        finally
        {
            IsBusy = false;
        }

        await RefreshAsync();
        if (IsLoggedIn)
            LoginSucceeded?.Invoke();
    }

    private bool CanLogin() => !IsBusy;

    partial void OnIsBusyChanged(bool value)
    {
        LoginCommand.NotifyCanExecuteChanged();
        RegisterCommand.NotifyCanExecuteChanged();
        ForgotPasswordCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private Task Register() => OpenAccountPageAsync("Register");

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private Task ForgotPassword() => OpenAccountPageAsync("ForgotPassword");

    private async Task OpenAccountPageAsync(string action)
    {
        try
        {
            var uri = AccountUri(action);
            var view = (App.Current as App)?.View;
            var topLevel = view is null ? null : TopLevel.GetTopLevel(view);
            if (topLevel is null || !await topLevel.Launcher.LaunchUriAsync(uri))
                throw new InvalidOperationException("Impossible d'ouvrir le navigateur.");
            SessionLabel = "Terminez l'opération dans le navigateur, puis choisissez Se connecter.";
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
    }

    [RelayCommand]
    public async System.Threading.Tasks.Task LogoutAsync()
    {
        if (Api is null) return;
        try { await Api.LogoutAsync().ConfigureAwait(true); }
        catch (Exception ex)
        {
            Refresh();
            SessionLabel = $"Deconnecte ; nettoyage de la messagerie incomplet : {ex.Message}";
            LogoutCompleted?.Invoke();
            return;
        }
        Refresh();
        LogoutCompleted?.Invoke();
    }

    [RelayCommand]
    internal async Task OpenSettings()
    {
        var app = (App)App.Current!;
        await app.PushPageAsync(app.ServiceProvider!.GetRequiredService<Settings>()).ConfigureAwait(true);
    }

    [RelayCommand]
    internal async Task OpenAbout()
    {
        var app = (App)App.Current!;
        await app.PushPageAsync(app.ServiceProvider!.GetRequiredService<AboutPageViewModel>()).ConfigureAwait(true);
    }

}
