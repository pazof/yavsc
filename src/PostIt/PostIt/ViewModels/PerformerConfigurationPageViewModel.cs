using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Services;
using Yavsc.Abstract.Workflow;
using Yavsc.Api.Client;

namespace PostIt.ViewModels;

public partial class ActivitySelectionOption : ObservableObject
{
    public string Code { get; }
    public string Name { get; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public ActivitySelectionOption(string code, string name)
    {
        Code = code;
        Name = name;
    }
}

public partial class PerformerConfigurationPageViewModel : ViewModelBase
{
    private readonly YavscApiClient? _api;
    private readonly ActivityApiClient? _activityClient;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string PerformerId { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string UserName { get; set; } = "—";

    [ObservableProperty]
    public partial string ExerciseCountryCode { get; set; } = "fr";

    [ObservableProperty]
    public partial string SIREN { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Website { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool Active { get; set; }

    [ObservableProperty]
    public partial bool AcceptNotifications { get; set; }

    [ObservableProperty]
    public partial bool AcceptPublicContact { get; set; }

    [ObservableProperty]
    public partial bool UseGeoLocalizationToReduceDistanceWithClients { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<ActivitySelectionOption> AvailableActivities { get; set; } = new();

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Prêt.";

    public override bool CanNavigateNext { get => false; protected set { _ = value; } }
    public override bool CanNavigatePrevious { get => true; protected set { _ = value; } }

    public PerformerConfigurationPageViewModel(YavscApiClient? api, ActivityApiClient? activityClient)
    {
        _api = api;
        _activityClient = activityClient;
    }

    public PerformerConfigurationPageViewModel() : this(null, null) { }

    public Task InitializeAsync() => RefreshAsync();

    [RelayCommand]
    public async Task RefreshAsync()
    {
        if (_api is null || _activityClient is null)
        {
            StatusMessage = "Client API de configuration indisponible.";
            return;
        }

        IsBusy = true;
        try
        {
            var endpoint = new Uri(new Uri(_api.Settings.ApiUrl.TrimEnd('/') + "/", UriKind.Absolute), "account/performer-profile").ToString();
            var dto = await _api.CallAsync<PerformerProfileSettings>(HttpMethod.Get, endpoint).ConfigureAwait(true);

            PerformerId = dto?.PerformerId ?? string.Empty;
            UserName = dto?.UserName ?? "—";
            ExerciseCountryCode = string.IsNullOrWhiteSpace(dto?.ExerciseCountryCode) ? "fr" : dto.ExerciseCountryCode;
            SIREN = dto?.SIREN ?? string.Empty;
            Website = dto?.WebSite ?? string.Empty;
            Active = dto?.Active ?? false;
            AcceptNotifications = dto?.AcceptNotifications ?? false;
            AcceptPublicContact = dto?.AcceptPublicContact ?? false;
            UseGeoLocalizationToReduceDistanceWithClients = dto?.UseGeoLocalizationToReduceDistanceWithClients ?? false;

            var catalog = await _activityClient.GetCatalogAsync().ConfigureAwait(true);
            var options = Flatten(catalog)
                .OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
                .Select(a => new ActivitySelectionOption(a.Code, a.Name)
                {
                    IsSelected = dto?.SelectedActivityCodes.Contains(a.Code, StringComparer.OrdinalIgnoreCase) == true
                })
                .ToList();

            AvailableActivities.Clear();
            foreach (var option in options)
            {
                AvailableActivities.Add(option);
            }

            StatusMessage = "Profil performer chargé.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Impossible de charger la configuration performer : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public PerformerProfileSettings BuildPerformerProfileSettings()
    {
        return new PerformerProfileSettings
        {
            PerformerId = string.IsNullOrWhiteSpace(PerformerId) ? string.Empty : PerformerId,
            UserName = UserName,
            ExerciseCountryCode = string.IsNullOrWhiteSpace(ExerciseCountryCode) ? "fr" : ExerciseCountryCode,
            SIREN = SIREN,
            WebSite = Website,
            Active = Active,
            AcceptNotifications = AcceptNotifications,
            AcceptPublicContact = AcceptPublicContact,
            UseGeoLocalizationToReduceDistanceWithClients = UseGeoLocalizationToReduceDistanceWithClients,
            SelectedActivityCodes = AvailableActivities
                .Where(a => a.IsSelected)
                .Select(a => a.Code)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
        };
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        if (_api is null)
        {
            StatusMessage = "Client API de configuration indisponible.";
            return;
        }

        IsBusy = true;
        try
        {
            var dto = BuildPerformerProfileSettings();

            var endpoint = new Uri(new Uri(_api.Settings.ApiUrl.TrimEnd('/') + "/", UriKind.Absolute), "account/performer-profile").ToString();
            var response = await _api.CallAsync<PerformerProfileSettings>(HttpMethod.Put, endpoint, dto).ConfigureAwait(true);
            StatusMessage = response is not null ? "Configuration performer enregistrée." : "Configuration performer enregistrée.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Impossible d’enregistrer la configuration performer : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static IEnumerable<ActivityInfo> Flatten(IEnumerable<ActivityInfo> activities)
    {
        foreach (var activity in activities)
        {
            yield return activity;
            foreach (var child in Flatten(activity.Children ?? Enumerable.Empty<ActivityInfo>()))
            {
                yield return child;
            }
        }
    }
}
