using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Services;
using Yavsc.Abstract.Identity;

namespace PostIt.ViewModels;

public partial class UserProfilePageViewModel : ViewModelBase
{
    private readonly YavscApiClient? _api;
    private readonly SessionStatusViewModel? _sessionStatus;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveProfileCommand))]
    [NotifyCanExecuteChangedFor(nameof(SetMonthlyEmailPreferenceCommand))]
    [NotifyCanExecuteChangedFor(nameof(SetTwoFactorCommand))]
    [NotifyCanExecuteChangedFor(nameof(ChangePasswordCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddBankAccountCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteBankAccountCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveExternalLoginCommand))]
    [NotifyCanExecuteChangedFor(nameof(SendEmailConfirmationCommand))]
    [NotifyCanExecuteChangedFor(nameof(LoadCalendarsCommand))]
    [NotifyCanExecuteChangedFor(nameof(ChooseCalendarCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteAccountCommand))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveProfileCommand))]
    [NotifyCanExecuteChangedFor(nameof(SetMonthlyEmailPreferenceCommand))]
    [NotifyCanExecuteChangedFor(nameof(SetTwoFactorCommand))]
    [NotifyCanExecuteChangedFor(nameof(ChangePasswordCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddBankAccountCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteBankAccountCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveExternalLoginCommand))]
    [NotifyCanExecuteChangedFor(nameof(SendEmailConfirmationCommand))]
    [NotifyCanExecuteChangedFor(nameof(LoadCalendarsCommand))]
    [NotifyCanExecuteChangedFor(nameof(ChooseCalendarCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteAccountCommand))]
    public partial bool IsProfileLoaded { get; set; }

    [ObservableProperty]
    public partial string UserName { get; set; } = "—";

    [ObservableProperty]
    public partial string CurrentUserName { get; set; } = "—";

    [ObservableProperty]
    public partial string UserId { get; set; } = "—";

    [ObservableProperty]
    public partial string FullName { get; set; } = "—";

    [ObservableProperty]
    public partial string Email { get; set; } = "—";

    [ObservableProperty]
    public partial string Address { get; set; } = "—";

    [ObservableProperty]
    public partial string Avatar { get; set; } = "—";

    [ObservableProperty]
    public partial string Roles { get; set; } = "—";

    [ObservableProperty]
    public partial string DedicatedGoogleCalendar { get; set; } = "Non configuré";

    [ObservableProperty]
    public partial string BankInfoSummary { get; set; } = "Aucune information bancaire";

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Prêt.";

    [ObservableProperty]
    public partial bool EmailConfirmed { get; set; }

    [ObservableProperty]
    public partial bool AllowMonthlyEmail { get; set; }

    [ObservableProperty]
    public partial bool HasPassword { get; set; }

    [ObservableProperty]
    public partial bool TwoFactorEnabled { get; set; }

    [ObservableProperty]
    public partial int ExternalLoginCount { get; set; }

    [ObservableProperty]
    public partial long PostsCounter { get; set; }

    [ObservableProperty]
    public partial long DiskUsage { get; set; }

    [ObservableProperty]
    public partial long DiskQuota { get; set; }

    [ObservableProperty]
    public partial decimal Credits { get; set; }

    [ObservableProperty]
    public partial string CurrentPassword { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewPassword { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ConfirmPassword { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string BankIban { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string BankBic { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string BankCode { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string BankWicketCode { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string BankAccountNumber { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string UsernameConfirmation { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int BankedKey { get; set; }

    public ObservableCollection<BankAccountInfo> BankAccounts { get; } = [];
    public ObservableCollection<LinkedExternalLogin> ExternalLogins { get; } = [];
    public ObservableCollection<CalendarOption> Calendars { get; } = [];

    public override bool CanNavigateNext { get => false; protected set { _ = value; } }
    public override bool CanNavigatePrevious { get => true; protected set { _ = value; } }

    public UserProfilePageViewModel(YavscApiClient? api, SessionStatusViewModel? sessionStatus = null)
    {
        _api = api;
        _sessionStatus = sessionStatus;
    }

    public UserProfilePageViewModel() : this(null, null) { }

    public Task InitializeAsync() => RefreshAsync();

    [RelayCommand]
    public async Task RefreshAsync()
    {
        if (_api is null)
        {
            IsProfileLoaded = false;
            StatusMessage = "Client API de profil indisponible.";
            return;
        }

        IsBusy = true;
        IsProfileLoaded = false;
        try
        {
            var me = await _api.CallAsync<Me>(HttpMethod.Get, new Uri(new Uri(_api.Settings.ApiUrl.TrimEnd('/') + "/", UriKind.Absolute), "account/me").ToString()).ConfigureAwait(true);
            if (me is null)
            {
                throw new InvalidOperationException("Le serveur a renvoyé un profil vide.");
            }

            UserName = me.UserName ?? "—";
            CurrentUserName = UserName;
            UserId = me.UserId;
            FullName = me.FullName ?? "—";
            Email = me.Email ?? "—";
            Address = me.Address ?? "—";
            Avatar = string.IsNullOrWhiteSpace(me.Avatar) ? "Aucun avatar" : me.Avatar;
            Roles = me.Roles is { Length: > 0 } ? string.Join(", ", me.Roles) : "Aucun rôle";
            DedicatedGoogleCalendar = string.IsNullOrWhiteSpace(me.DedicatedGoogleCalendar) ? "Non configuré" : me.DedicatedGoogleCalendar;
            BankInfoSummary = string.IsNullOrWhiteSpace(me.BankInfoSummary) ? "Aucune information bancaire" : me.BankInfoSummary;
            EmailConfirmed = me.EmailConfirmed;
            AllowMonthlyEmail = me.AllowMonthlyEmail;
            HasPassword = me.HasPassword;
            TwoFactorEnabled = me.TwoFactorEnabled;
            ExternalLoginCount = me.ExternalLoginCount;
            PostsCounter = me.PostsCounter;
            DiskUsage = me.DiskUsage;
            DiskQuota = me.DiskQuota;
            Credits = me.Credits;
            BankAccounts.Clear();
            foreach (var account in me.BankAccounts)
            {
                BankAccounts.Add(account);
            }
            ExternalLogins.Clear();
            foreach (var login in me.ExternalLogins)
            {
                ExternalLogins.Add(login);
            }
            IsProfileLoaded = true;
            StatusMessage = "Profil chargé.";
        }
        catch (Exception ex)
        {
            IsProfileLoaded = false;
            UserName = "—";
            CurrentUserName = "—";
            UserId = "—";
            FullName = "—";
            Email = "—";
            Address = "—";
            Avatar = "—";
            Roles = "—";
            DedicatedGoogleCalendar = "Non configuré";
            BankInfoSummary = "Aucune information bancaire";
            EmailConfirmed = false;
            AllowMonthlyEmail = false;
            HasPassword = false;
            TwoFactorEnabled = false;
            ExternalLoginCount = 0;
            PostsCounter = 0;
            DiskUsage = 0;
            DiskQuota = 0;
            Credits = 0;
            BankAccounts.Clear();
            ExternalLogins.Clear();
            Calendars.Clear();
            StatusMessage = $"Impossible de charger le profil : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public ProfileUpdateRequest BuildProfileUpdateRequest()
    {
        static string NormalizeField(string? value, string placeholder)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var trimmed = value.Trim();
            return string.Equals(trimmed, placeholder, StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : trimmed;
        }

        return new ProfileUpdateRequest
        {
            UserName = NormalizeField(UserName, "—"),
            FullName = NormalizeField(FullName, "—"),
            Address = NormalizeField(Address, "—"),
            GoogleCalendarId = NormalizeField(DedicatedGoogleCalendar, "Non configuré")
        };
    }

    private bool CanSaveProfile() => _api is not null && IsProfileLoaded && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanSaveProfile))]
    public async Task SaveProfileAsync()
    {
        if (_api is null)
        {
            StatusMessage = "Client API de profil indisponible.";
            return;
        }

        IsBusy = true;
        try
        {
            var payload = BuildProfileUpdateRequest();

            var endpoint = new Uri(new Uri(_api.Settings.ApiUrl.TrimEnd('/') + "/", UriKind.Absolute), "account/me").ToString();
            await _api.CallAsync(HttpMethod.Put, endpoint, payload).ConfigureAwait(true);
            StatusMessage = "Profil enregistré.";
            await RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Impossible d’enregistrer le profil : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanModifyAccount))]
    public async Task SetMonthlyEmailPreferenceAsync()
    {
        if (_api is null)
        {
            StatusMessage = "Client API de profil indisponible.";
            return;
        }

        await UpdateBooleanPreferenceAsync(
            "account/preferences/monthly-email",
            new MonthlyEmailPreferenceRequest { Enabled = AllowMonthlyEmail },
            enabled => AllowMonthlyEmail = enabled,
            "préférence de courriel");
    }

    [RelayCommand(CanExecute = nameof(CanModifyAccount))]
    public async Task SetTwoFactorAsync()
    {
        if (_api is null)
        {
            StatusMessage = "Client API de profil indisponible.";
            return;
        }

        await UpdateBooleanPreferenceAsync(
            "account/security/two-factor",
            new TwoFactorRequest { Enabled = TwoFactorEnabled },
            enabled => TwoFactorEnabled = enabled,
            "authentification à deux facteurs");
    }

    [RelayCommand(CanExecute = nameof(CanModifyAccount))]
    public async Task ChangePasswordAsync()
    {
        if (_api is null)
        {
            StatusMessage = "Client API de profil indisponible.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewPassword)
            || !string.Equals(NewPassword, ConfirmPassword, StringComparison.Ordinal))
        {
            StatusMessage = "Le nouveau mot de passe et sa confirmation doivent être identiques.";
            return;
        }

        IsBusy = true;
        try
        {
            var endpoint = BuildEndpoint("account/security/password");
            await _api.CallAsync(HttpMethod.Put, endpoint, new ChangePasswordRequest
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword,
                ConfirmPassword = ConfirmPassword
            }).ConfigureAwait(true);
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
            HasPassword = true;
            StatusMessage = "Mot de passe mis à jour.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Impossible de modifier le mot de passe : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanModifyAccount))]
    public async Task AddBankAccountAsync()
    {
        if (_api is null)
        {
            StatusMessage = "Client API de profil indisponible.";
            return;
        }

        if (string.IsNullOrWhiteSpace(BankIban) && string.IsNullOrWhiteSpace(BankBic))
        {
            StatusMessage = "Renseignez un IBAN ou un BIC.";
            return;
        }

        IsBusy = true;
        try
        {
            var endpoint = BuildEndpoint("account/bank-info");
            await _api.CallAsync<BankAccountInfo>(HttpMethod.Post, endpoint, new BankAccountInfoRequest
            {
                IBAN = BankIban.Trim(),
                BIC = BankBic.Trim(),
                BankCode = BankCode.Trim(),
                WicketCode = BankWicketCode.Trim(),
                AccountNumber = BankAccountNumber.Trim(),
                BankedKey = BankedKey
            }).ConfigureAwait(true);
            BankIban = string.Empty;
            BankBic = string.Empty;
            BankCode = string.Empty;
            BankWicketCode = string.Empty;
            BankAccountNumber = string.Empty;
            BankedKey = 0;
            await RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Impossible d’ajouter le compte bancaire : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanModifyAccount))]
    public async Task DeleteBankAccountAsync(BankAccountInfo? account)
    {
        if (_api is null || account is null)
        {
            return;
        }

        IsBusy = true;
        try
        {
            await _api.CallAsync(HttpMethod.Delete, BuildEndpoint($"account/bank-info/{account.Id}")).ConfigureAwait(true);
            await RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Impossible de supprimer le compte bancaire : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanModifyAccount))]
    public async Task RemoveExternalLoginAsync(LinkedExternalLogin? login)
    {
        if (_api is null || login is null)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var query = $"provider={Uri.EscapeDataString(login.Provider)}&providerKey={Uri.EscapeDataString(login.ProviderKey)}";
            await _api.CallAsync(HttpMethod.Delete, BuildEndpoint($"account/security/external-logins?{query}")).ConfigureAwait(true);
            await RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Impossible de supprimer cette connexion externe : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanModifyAccount))]
    public async Task SendEmailConfirmationAsync()
    {
        if (_api is null)
        {
            StatusMessage = "Client API de profil indisponible.";
            return;
        }

        IsBusy = true;
        try
        {
            await _api.CallAsync<JsonElement>(HttpMethod.Post, BuildEndpoint("account/email/confirmation")).ConfigureAwait(true);
            StatusMessage = "E-mail de confirmation envoyé.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Impossible d’envoyer l’e-mail de confirmation : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanModifyAccount))]
    public async Task LoadCalendarsAsync()
    {
        if (_api is null)
        {
            StatusMessage = "Client API de profil indisponible.";
            return;
        }

        IsBusy = true;
        try
        {
            var calendars = await _api.CallAsync<CalendarOption[]>(
                HttpMethod.Get,
                BuildEndpoint("account/calendars")).ConfigureAwait(true);
            Calendars.Clear();
            foreach (var calendar in calendars ?? [])
            {
                Calendars.Add(calendar);
            }

            StatusMessage = Calendars.Count == 0
                ? "Aucun agenda Google dont vous êtes propriétaire n’est disponible."
                : "Agendas Google chargés.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Impossible de charger les agendas Google : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanModifyAccount))]
    public async Task ChooseCalendarAsync(CalendarOption? calendar)
    {
        if (calendar is null)
        {
            DedicatedGoogleCalendar = "Non configuré";
        }
        else
        {
            DedicatedGoogleCalendar = calendar.Id;
        }

        await SaveProfileAsync().ConfigureAwait(true);
    }

    [RelayCommand(CanExecute = nameof(CanModifyAccount))]
    public async Task DeleteAccountAsync()
    {
        if (_api is null)
        {
            StatusMessage = "Client API de profil indisponible.";
            return;
        }

        if (!string.Equals(UsernameConfirmation, CurrentUserName, StringComparison.Ordinal))
        {
            StatusMessage = "Saisissez exactement votre nom d’utilisateur pour confirmer la suppression.";
            return;
        }

        IsBusy = true;
        try
        {
            await _api.CallAsync(HttpMethod.Delete, BuildEndpoint("account/me"), new DeleteAccountRequest
            {
                UsernameConfirmation = UsernameConfirmation
            }).ConfigureAwait(true);
            UsernameConfirmation = string.Empty;
            StatusMessage = "Compte supprimé.";

            if (_sessionStatus is not null)
            {
                await _sessionStatus.LogoutAsync().ConfigureAwait(true);
            }
            else
            {
                await _api.LogoutAsync().ConfigureAwait(true);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Impossible de supprimer le compte : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task UpdateBooleanPreferenceAsync<TRequest>(
        string path,
        TRequest request,
        Action<bool> applyValue,
        string preferenceName)
        where TRequest : class
    {
        IsBusy = true;
        try
        {
            var endpoint = BuildEndpoint(path);
            var response = await _api!.CallAsync<System.Text.Json.JsonElement>(HttpMethod.Put, endpoint, request).ConfigureAwait(true);
            var enabled = response.GetProperty("enabled").GetBoolean();
            applyValue(enabled);
            StatusMessage = $"Préférence mise à jour : {preferenceName}.";
        }
        catch (Exception ex)
        {
            await RefreshAsync().ConfigureAwait(true);
            StatusMessage = $"Impossible de mettre à jour {preferenceName} : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private string BuildEndpoint(string path) =>
        new Uri(new Uri(_api!.Settings.ApiUrl.TrimEnd('/') + "/", UriKind.Absolute), path).ToString();

    private bool CanModifyAccount() => _api is not null && IsProfileLoaded && !IsBusy;
}
