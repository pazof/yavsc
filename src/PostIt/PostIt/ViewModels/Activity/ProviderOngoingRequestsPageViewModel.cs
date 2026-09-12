using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Helpers;
using Yavsc;
using Yavsc.Abstract.Workflow;
using Yavsc.Api.Client;

namespace PostIt.ViewModels;

public partial class ProviderOngoingRequestsPageViewModel : ViewModelBase, IActionStatusViewModel
{
    public const string SortByDate = "Date (plus récent d'abord)";
    public const string SortByDateAsc = "Date (plus ancien d'abord)";
    public const string SortByStatus = "Statut (en cours d'abord)";

    private readonly BillingApiClient _billingClient;
    private readonly Settings? _settings;
    private List<BillingQuerySummaryDto> _allQueries = new();

    [ObservableProperty]
    public partial ObservableCollection<BillingQuerySummaryDto> Queries { get; set; } = new();

    [ObservableProperty]
    public partial string FilterText { get; set; } = string.Empty;

    public IReadOnlyList<string> SortOptions { get; } = new[]
    {
        SortByDate,
        SortByDateAsc,
        SortByStatus,
    };

    [ObservableProperty]
    public partial string SelectedSortOption { get; set; } = SortByDate;

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(OpenSelectedQueryCommand))]
    public partial BillingQuerySummaryDto? SelectedQuery { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Chargement des demandes fournisseur...";

    [ObservableProperty]
    public partial StatusNotice ActionStatus { get; set; } = StatusNotice.Info("Chargement des demandes fournisseur...");

    public string Title => "Mes demandes en cours";

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

    public ProviderOngoingRequestsPageViewModel(BillingApiClient billingClient, Settings? settings = null)
    {
        _billingClient = billingClient ?? throw new ArgumentNullException(nameof(billingClient));
        _settings = settings;

        if (_settings is not null)
        {
            var preferredSort = NormalizeSortOption(_settings.ProviderOngoingRequestsSortOption);
            if (!string.Equals(preferredSort, SelectedSortOption, StringComparison.Ordinal))
            {
                SelectedSortOption = preferredSort;
            }
        }
    }

    public Task InitializeAsync() => RefreshAsync();

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            var items = await _billingClient.GetProviderOngoingQueriesAsync().ConfigureAwait(true) ?? new();
            _allQueries = items
                .Where(x => !string.IsNullOrWhiteSpace(x.BillingCode))
                .OrderByDescending(x => x.EventDate ?? DateTime.MinValue)
                .ThenByDescending(x => x.Id)
                .ToList();

            ApplyFilter();
            this.SetInfoStatus(_allQueries.Count == 0
                ? "Aucune demande en cours pour votre profil fournisseur."
                : $"{_allQueries.Count} demande(s) en cours chargée(s).");
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            _allQueries = new List<BillingQuerySummaryDto>();
            Queries = new ObservableCollection<BillingQuerySummaryDto>();
            this.SetWarningStatus("Accès refusé au billing (scope 'api'). Déconnectez puis reconnectez-vous.");
        }
        catch (Exception ex)
        {
            _allQueries = new List<BillingQuerySummaryDto>();
            Queries = new ObservableCollection<BillingQuerySummaryDto>();
            this.SetErrorStatus($"Erreur: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanOpenSelectedQuery() => SelectedQuery is not null;

    private bool CanOpenSelectedEditor() => SelectedQuery is not null;

    [RelayCommand(CanExecute = nameof(CanOpenSelectedQuery))]
    public async Task OpenSelectedQueryAsync()
    {
        if (SelectedQuery is null)
        {
            this.SetWarningStatus("Sélectionnez une demande.");
            return;
        }

        var app = (App?)Application.Current;
        if (app is null)
        {
            throw new InvalidOperationException("Application PostIt indisponible.");
        }

        IsBusy = true;
        try
        {
            var details = await _billingClient
                .GetQueryAsync(SelectedQuery.BillingCode, SelectedQuery.Id)
                .ConfigureAwait(true);
            var (activity, performer, form) = BuildNavigationContext(SelectedQuery);

            var vm = new BillingQueryDetailsPageViewModel(
                activity,
                performer,
                form,
                _billingClient,
                details,
                isReadOnly: false);

            await app.PushPageAsync(vm).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            this.SetErrorStatus($"Erreur lors de l'ouverture: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedEditor))]
    public async Task OpenSelectedEditorAsync()
    {
        if (SelectedQuery is null)
        {
            this.SetWarningStatus("Sélectionnez une demande.");
            return;
        }

        var app = (App?)Application.Current;
        if (app is null)
        {
            throw new InvalidOperationException("Application PostIt indisponible.");
        }

        IsBusy = true;
        try
        {
            var details = await _billingClient
                .GetQueryAsync(SelectedQuery.BillingCode, SelectedQuery.Id)
                .ConfigureAwait(true);

            var (activity, performer, form) = BuildNavigationContext(SelectedQuery);
            var vm = form.CreateCommandPageViewModel(activity, performer, _billingClient);
            if (vm is null)
            {
                this.SetWarningStatus($"Le formulaire '{form.ActionName}' n'est pas pris en charge en édition.");
                return;
            }

            await vm.InitializeAsync(details).ConfigureAwait(true);
            await app.PushPageAsync(vm).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            this.SetErrorStatus($"Erreur lors de l'ouverture en édition: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnFilterTextChanged(string value)
    {
        ApplyFilter();
    }

    partial void OnSelectedSortOptionChanged(string value)
    {
        var normalized = NormalizeSortOption(value);
        if (!string.Equals(normalized, value, StringComparison.Ordinal))
        {
            SelectedSortOption = normalized;
            return;
        }

        PersistSortPreference(value);
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var query = FilterText?.Trim();
        var filtered = string.IsNullOrWhiteSpace(query)
            ? _allQueries
            : _allQueries.Where(x =>
                    ContainsInsensitive(x.Description, query)
                    || ContainsInsensitive(x.ActivityCode, query)
                    || ContainsInsensitive(x.BillingCode, query)
                    || ContainsInsensitive(x.ClientId, query)
                    || ContainsInsensitive(x.Status.ToString(), query))
                .ToList();

        var sorted = ApplySort(filtered);
        Queries = new ObservableCollection<BillingQuerySummaryDto>(sorted);
    }

    private List<BillingQuerySummaryDto> ApplySort(IEnumerable<BillingQuerySummaryDto> source)
    {
        if (string.Equals(SelectedSortOption, SortByStatus, StringComparison.Ordinal))
        {
            return source
                .OrderBy(x => GetStatusRank(x.Status))
                .ThenByDescending(x => x.EventDate ?? DateTime.MinValue)
                .ThenByDescending(x => x.Id)
                .ToList();
        }

            if (string.Equals(SelectedSortOption, SortByDateAsc, StringComparison.Ordinal))
            {
                return source
                .OrderBy(x => x.EventDate ?? DateTime.MinValue)
                .ThenBy(x => x.Id)
                .ToList();
            }

        return source
            .OrderByDescending(x => x.EventDate ?? DateTime.MinValue)
            .ThenByDescending(x => x.Id)
            .ToList();
    }

    private void PersistSortPreference(string selectedSort)
    {
        if (_settings is null)
        {
            return;
        }

        if (string.Equals(_settings.ProviderOngoingRequestsSortOption, selectedSort, StringComparison.Ordinal))
        {
            return;
        }

        _settings.ProviderOngoingRequestsSortOption = selectedSort;

        try
        {
            _settings.Save();
        }
        catch
        {
            this.SetWarningStatus("Le tri a été appliqué, mais sa sauvegarde a échoué.");
        }
    }

    private static string NormalizeSortOption(string? sortOption)
    {
        if (string.Equals(sortOption, SortByDate, StringComparison.Ordinal)
            || string.Equals(sortOption, SortByDateAsc, StringComparison.Ordinal)
            || string.Equals(sortOption, SortByStatus, StringComparison.Ordinal))
        {
            return sortOption!;
        }

        return SortByDate;
    }

    private static int GetStatusRank(QueryStatus status)
        => status switch
        {
            QueryStatus.InProgress => 0,
            QueryStatus.Accepted => 1,
            QueryStatus.Inserted => 2,
            QueryStatus.Success => 3,
            QueryStatus.Rejected => 4,
            QueryStatus.Failed => 5,
            _ => 99,
        };

    private static bool ContainsInsensitive(string? source, string query)
        => !string.IsNullOrWhiteSpace(source)
           && source.Contains(query, StringComparison.OrdinalIgnoreCase);

    private static (ActivityInfo activity, ActivityUserDisplayItem performer, CommandFormSummary form)
        BuildNavigationContext(BillingQuerySummaryDto query)
    {
        var activity = new ActivityInfo
        {
            Code = query.ActivityCode,
            Name = string.IsNullOrWhiteSpace(query.ActivityCode)
                ? "Activité"
                : query.ActivityCode,
        };

        var performer = new ActivityUserDisplayItem
        {
            PerformerId = query.PerformerId,
            UserName = "Mon profil fournisseur",
            AvatarFallbackLabel = "M",
            IsPerformerActive = true,
            PerformerStatusBadgeLabel = "Actif",
            PerformerStatusBadgeBackground = "#E6F7EC",
            PerformerStatusBadgeBorder = "#2E7D32",
            PerformerStatusBadgeForeground = "#1B5E20",
        };

        var form = new CommandFormSummary
        {
            ActionName = query.BillingCode,
            Title = query.BillingCode,
        };

        return (activity, performer, form);
    }
}
