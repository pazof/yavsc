using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Helpers;
using Yavsc.Abstract.Workflow;
using Yavsc.Api.Client;

namespace PostIt.ViewModels;

public partial class ActivitiesPageViewModel : ViewModelBase
{
    private readonly ActivityApiClient _client;
    private readonly BillingApiClient _billingClient;
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

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(OpenCommandFormsCommand))]
    public partial ActivityUserDisplayItem? SelectedPerformer { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Choisissez une activité.";

    public ActivityBrowseItemDto? CurrentActivity => SelectedSpecialization ?? SelectedActivity;
    public string SelectedActivityLabel => SelectedActivity?.Name ?? "(aucune activité)";
    public string CurrentActivityLabel => CurrentActivity?.Name ?? "(aucune)";
    public int CurrentFormCount => CurrentActivity?.Forms?.Count ?? 0;

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

    public ActivitiesPageViewModel(ActivityApiClient client, BillingApiClient billingClient)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _billingClient = billingClient ?? throw new ArgumentNullException(nameof(billingClient));
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
        OnPropertyChanged(nameof(CurrentFormCount));
        Specializations = new ObservableCollection<ActivityBrowseItemDto>(activity?.Children ?? new());

        if (activity is null)
        {
            Performers = new ObservableCollection<ActivityUserDisplayItem>();
            SelectedPerformer = null;
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
        OnPropertyChanged(nameof(CurrentFormCount));

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
            SelectedPerformer = null;
            StatusMessage = $"{activity.Name} · {Performers.Count} utilisateur(s)";
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            Performers = new ObservableCollection<ActivityUserDisplayItem>();
            SelectedPerformer = null;
            StatusMessage = "Accès refusé pour les activités (scope 'api'). Déconnectez puis reconnectez-vous.";
        }
        catch (Exception ex)
        {
            Performers = new ObservableCollection<ActivityUserDisplayItem>();
            SelectedPerformer = null;
            StatusMessage = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            OpenCommandFormsCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanOpenCommandForms()
        => SelectedPerformer is not null && CurrentActivity?.Forms?.Count > 0;

    [RelayCommand(CanExecute = nameof(CanOpenCommandForms))]
    private async Task OpenCommandFormsAsync()
    {
        if (SelectedPerformer is null || CurrentActivity is null)
        {
            StatusMessage = "Sélectionnez un utilisateur et une activité avec formulaire.";
            return;
        }

        var app = (App?)Application.Current;
        if (app is null)
        {
            throw new InvalidOperationException("Application PostIt indisponible.");
        }

        var vm = new CommandFormsPageViewModel(CurrentActivity, SelectedPerformer, _billingClient);
        await app.PushPageAsync(vm);
    }
}
