using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Yavsc.Abstract.Workflow;
using Yavsc.Api.Client;

namespace PostIt.ViewModels;

public partial class ActivitiesPageViewModel : ViewModelBase
{
    private readonly ActivityApiClient _client;
    private bool _syncingSelection;

    [ObservableProperty]
    public partial ObservableCollection<ActivityBrowseItemDto> Activities { get; set; } = new();

    [ObservableProperty]
    public partial ActivityBrowseItemDto? SelectedActivity { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<ActivityBrowseItemDto> Specializations { get; set; } = new();

    [ObservableProperty]
    public partial ActivityBrowseItemDto? SelectedSpecialization { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<ActivityUserDisplayItem> Performers { get; set; } = new();

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Choisissez une activité.";

    public ActivityBrowseItemDto? CurrentActivity => SelectedSpecialization ?? SelectedActivity;
    public string SelectedActivityLabel => SelectedActivity?.Name ?? "(aucune activité)";
    public string CurrentActivityLabel => CurrentActivity?.Name ?? "(aucune)";

    public override bool CanNavigateNext
    {
        get => false;
        protected set { _ = value; }
    }

    public override bool CanNavigatePrevious
    {
        get => true;
        protected set { _ = value; }
    }

    public ActivitiesPageViewModel(ActivityApiClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    partial void OnSelectedActivityChanged(ActivityBrowseItemDto? value)
    {
        if (_syncingSelection) return;
        _ = ShowActivitySafeAsync(value);
    }

    partial void OnSelectedSpecializationChanged(ActivityBrowseItemDto? value)
    {
        if (_syncingSelection) return;
        _ = ShowSpecializationSafeAsync(value);
    }

    private async Task ShowActivitySafeAsync(ActivityBrowseItemDto? value)
    {
        try
        {
            await ShowActivityAsync(value);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            StatusMessage = "Accès refusé pour les activités (scope 'api'). Déconnectez puis reconnectez-vous.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur: {ex.Message}";
        }
    }

    private async Task ShowSpecializationSafeAsync(ActivityBrowseItemDto? value)
    {
        try
        {
            await ShowSpecializationAsync(value);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            StatusMessage = "Accès refusé pour les activités (scope 'api'). Déconnectez puis reconnectez-vous.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            var list = await _client.GetCatalogAsync();
            Activities = new ObservableCollection<ActivityBrowseItemDto>(list ?? new());

            var first = Activities.FirstOrDefault();
            await ShowActivityAsync(first);
            if (first is null)
            {
                StatusMessage = "Aucune activité disponible.";
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            Activities = new ObservableCollection<ActivityBrowseItemDto>();
            Specializations = new ObservableCollection<ActivityBrowseItemDto>();
            Performers = new ObservableCollection<ActivityUserDisplayItem>();
            StatusMessage = "Accès refusé pour les activités (scope 'api'). Déconnectez puis reconnectez-vous.";
        }
        catch (Exception ex)
        {
            Activities = new ObservableCollection<ActivityBrowseItemDto>();
            Specializations = new ObservableCollection<ActivityBrowseItemDto>();
            Performers = new ObservableCollection<ActivityUserDisplayItem>();
            StatusMessage = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task ShowActivityAsync(ActivityBrowseItemDto? activity)
    {
        _syncingSelection = true;
        try
        {
            SelectedActivity = activity;
            SelectedSpecialization = null;
        }
        finally
        {
            _syncingSelection = false;
        }

        OnPropertyChanged(nameof(CurrentActivity));
        OnPropertyChanged(nameof(SelectedActivityLabel));
        OnPropertyChanged(nameof(CurrentActivityLabel));
        Specializations = new ObservableCollection<ActivityBrowseItemDto>(activity?.Children ?? new());

        if (activity is null)
        {
            Performers = new ObservableCollection<ActivityUserDisplayItem>();
            return;
        }

        await LoadPerformersAsync(activity);
    }

    public async Task ShowSpecializationAsync(ActivityBrowseItemDto? specialization)
    {
        _syncingSelection = true;
        try
        {
            SelectedSpecialization = specialization;
        }
        finally
        {
            _syncingSelection = false;
        }

        OnPropertyChanged(nameof(CurrentActivity));
        OnPropertyChanged(nameof(CurrentActivityLabel));

        if (specialization is null)
        {
            if (SelectedActivity is not null)
            {
                await LoadPerformersAsync(SelectedActivity);
            }
            return;
        }

        await LoadPerformersAsync(specialization);
    }

    private async Task LoadPerformersAsync(ActivityBrowseItemDto activity)
    {
        IsBusy = true;
        try
        {
            var list = await _client.GetUsersAsync(activity.Code);
            Performers = new ObservableCollection<ActivityUserDisplayItem>((list ?? new())
                .Select(ActivityUserDisplayItem.FromDto));
            StatusMessage = $"{activity.Name} · {Performers.Count} utilisateur(s)";
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            Performers = new ObservableCollection<ActivityUserDisplayItem>();
            StatusMessage = "Accès refusé pour les activités (scope 'api'). Déconnectez puis reconnectez-vous.";
        }
        catch (Exception ex)
        {
            Performers = new ObservableCollection<ActivityUserDisplayItem>();
            StatusMessage = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
