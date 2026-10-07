using System;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Services;
using Yavsc.Abstract.Identity;

namespace PostIt.ViewModels;

public partial class UserProfilePageViewModel : ViewModelBase
{
    private readonly YavscApiClient? _api;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string UserName { get; set; } = "—";

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

    public override bool CanNavigateNext { get => false; protected set { _ = value; } }
    public override bool CanNavigatePrevious { get => true; protected set { _ = value; } }

    public UserProfilePageViewModel(YavscApiClient? api)
    {
        _api = api;
    }

    public UserProfilePageViewModel() : this(null) { }

    public Task InitializeAsync() => RefreshAsync();

    [RelayCommand]
    public async Task RefreshAsync()
    {
        if (_api is null)
        {
            StatusMessage = "Client API de profil indisponible.";
            return;
        }

        IsBusy = true;
        try
        {
            var me = await _api.CallAsync<Me>(HttpMethod.Get, new Uri(new Uri(_api.Settings.ApiUrl.TrimEnd('/') + "/", UriKind.Absolute), "account/me").ToString()).ConfigureAwait(true);
            UserName = me?.UserName ?? "—";
            FullName = me?.FullName ?? "—";
            Email = me?.Email ?? "—";
            Address = me?.Address ?? "—";
            Avatar = string.IsNullOrWhiteSpace(me?.Avatar) ? "Aucun avatar" : me.Avatar;
            Roles = me?.Roles is { Length: > 0 } ? string.Join(", ", me.Roles) : "Aucun rôle";
            DedicatedGoogleCalendar = string.IsNullOrWhiteSpace(me?.DedicatedGoogleCalendar) ? "Non configuré" : me.DedicatedGoogleCalendar;
            BankInfoSummary = string.IsNullOrWhiteSpace(me?.BankInfoSummary) ? "Aucune information bancaire" : me.BankInfoSummary;
            StatusMessage = "Profil chargé.";
        }
        catch (Exception ex)
        {
            UserName = "—";
            FullName = "—";
            Email = "—";
            Address = "—";
            Avatar = "—";
            Roles = "—";
            DedicatedGoogleCalendar = "Non configuré";
            BankInfoSummary = "Aucune information bancaire";
            StatusMessage = $"Impossible de charger le profil : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
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
            var payload = new ProfileUpdateRequest
            {
                FullName = FullName,
                Address = Address,
                GoogleCalendarId = DedicatedGoogleCalendar == "Non configuré" ? string.Empty : DedicatedGoogleCalendar,
                BankInfoSummary = BankInfoSummary == "Aucune information bancaire" ? string.Empty : BankInfoSummary
            };

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
}
