using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Helpers;
using Yavsc.Api.Client;

namespace PostIt.ViewModels;

/// <summary>
/// Point de vue d'une liste de devis en cours (non encore validés par
/// le client).
/// </summary>
public enum EstimateListPerspective
{
    /// <summary>« Mes devis à valider » — l'utilisateur courant est le client.</summary>
    Client,

    /// <summary>« Mes devis en attente » — l'utilisateur courant est le fournisseur.</summary>
    Provider,
}

/// <summary>
/// Liste des devis en cours servie par <c>GET api/v1/estimate/asclient</c>
/// ou <c>GET api/v1/estimate/asprovider</c> selon la perspective choisie.
/// </summary>
public partial class EstimateListPageViewModel : ViewModelBase, IActionStatusViewModel
{
    private readonly EstimateApiClient _estimateClient;
    private readonly EstimateListPerspective _perspective;
    private List<EstimateDto> _allEstimates = new();

    public EstimateListPerspective Perspective => _perspective;

    public string Title => _perspective == EstimateListPerspective.Client
        ? "Mes devis à valider"
        : "Mes devis en attente";

    public string Subtitle => _perspective == EstimateListPerspective.Client
        ? "Point de vue client"
        : "Point de vue fournisseur";

    /// <summary>
    /// Libellé de la colonne « autre partie » : côté client on montre
    /// le fournisseur (OwnerId), côté fournisseur on montre le client.
    /// </summary>
    public string CounterpartLabel => _perspective == EstimateListPerspective.Client
        ? "Fournisseur"
        : "Client";

    [ObservableProperty]
    public partial ObservableCollection<EstimateDto> Estimates { get; set; } = new();

    [ObservableProperty]
    public partial string FilterText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial EstimateDto? SelectedEstimate { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Chargement des devis...";

    [ObservableProperty]
    public partial StatusNotice ActionStatus { get; set; } = StatusNotice.Info("Chargement des devis...");

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

    public EstimateListPageViewModel(EstimateApiClient estimateClient, EstimateListPerspective perspective)
    {
        _estimateClient = estimateClient ?? throw new ArgumentNullException(nameof(estimateClient));
        _perspective = perspective;
    }

    public Task InitializeAsync() => RefreshAsync();

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            var items = (_perspective == EstimateListPerspective.Client
                ? await _estimateClient.GetOngoingEstimatesAsClientAsync().ConfigureAwait(true)
                : await _estimateClient.GetOngoingEstimatesAsProviderAsync().ConfigureAwait(true))
                ?? new List<EstimateDto>();

            _allEstimates = items
                .OrderByDescending(x => x.Id)
                .ToList();

            ApplyFilter();
            this.SetInfoStatus(_allEstimates.Count == 0
                ? "Aucun devis en cours."
                : $"{_allEstimates.Count} devis en cours chargé(s).");
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            _allEstimates = new List<EstimateDto>();
            Estimates = new ObservableCollection<EstimateDto>();
            this.SetWarningStatus("Accès refusé (scope 'api'). Déconnectez puis reconnectez-vous.");
        }
        catch (Exception ex)
        {
            _allEstimates = new List<EstimateDto>();
            Estimates = new ObservableCollection<EstimateDto>();
            this.SetErrorStatus($"Erreur: {ex.Message}");
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

    private void ApplyFilter()
    {
        var query = FilterText?.Trim();
        var filtered = string.IsNullOrWhiteSpace(query)
            ? _allEstimates
            : _allEstimates.Where(x =>
                    ContainsInsensitive(x.Title, query)
                    || ContainsInsensitive(x.Description, query)
                    || ContainsInsensitive(x.CommandType, query)
                    || ContainsInsensitive(x.ClientId, query)
                    || ContainsInsensitive(x.OwnerId, query))
                .ToList();

        Estimates = new ObservableCollection<EstimateDto>(filtered);
    }

    private static bool ContainsInsensitive(string? value, string query)
        => !string.IsNullOrWhiteSpace(value)
           && value.Contains(query, StringComparison.OrdinalIgnoreCase);
}
