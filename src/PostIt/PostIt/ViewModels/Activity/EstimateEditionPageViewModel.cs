using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Helpers;
using Yavsc.Api.Client;

namespace PostIt.ViewModels;

/// <summary>
/// Edition d'un devis (<c>Estimate</c>) créé en réponse à une demande
/// client (<see cref="BillingQuerySummaryDto"/>) consultée depuis la
/// page « Mes demandes en cours ». L'envoi poste le devis sur
/// <c>api/v1/estimate</c>; côté serveur, la commande liée
/// (<see cref="BillingQuerySummaryDto.Id"/>) est alors marquée comme
/// validée par le prestataire.
/// </summary>
public partial class EstimateEditionPageViewModel : ViewModelBase, IActionStatusViewModel
{
    private readonly EstimateApiClient _estimateClient;
    private readonly BillingQuerySummaryDto _query;

    public long QueryId => _query.Id;
    public string ClientId => _query.ClientId;
    public string BillingCode => _query.BillingCode;

    public string Title => $"Devis — demande #{_query.Id}";

    public string ContextLabel
        => $"Demande #{_query.Id} · {BillingCode} · client {ClientId}";

    public string QueryDescription => string.IsNullOrWhiteSpace(_query.Description)
        ? "(sans description)"
        : _query.Description;

    [ObservableProperty]
    public partial string EstimateTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EstimateDescription { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<EstimateLineItemViewModel> Lines { get; set; } = new();

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(RemoveLineCommand))]
    public partial EstimateLineItemViewModel? SelectedLine { get; set; }

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SendCommand))]
    public partial bool IsBusy { get; set; }

    /// <summary>
    /// True une fois le devis accepté par le serveur: l'envoi est
    /// désactivé pour éviter les doublons, il ne reste que « Retour ».
    /// </summary>
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SendCommand))]
    public partial bool HasSent { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Prêt.";

    [ObservableProperty]
    public partial StatusNotice ActionStatus { get; set; } = StatusNotice.Info("Prêt.");

    public decimal Total => Lines.Sum(line => line.LineTotal);

    public string TotalLabel => $"{Total:0.00} {Lines.FirstOrDefault()?.Currency ?? "EUR"}";

    public string SendLabel => HasSent ? "Devis envoyé" : "Envoyer le devis";

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

    public EstimateEditionPageViewModel(BillingQuerySummaryDto query, EstimateApiClient estimateClient)
    {
        _query = query ?? throw new ArgumentNullException(nameof(query));
        _estimateClient = estimateClient ?? throw new ArgumentNullException(nameof(estimateClient));

        EstimateDescription = query.Description ?? string.Empty;
        Lines.CollectionChanged += OnLinesCollectionChanged;

        AddLine();
        this.SetInfoStatus("Complétez le devis puis envoyez-le. La demande associée sera validée.");
    }

    [RelayCommand]
    private void AddLine()
    {
        var line = new EstimateLineItemViewModel();
        Lines.Add(line);
        SelectedLine = line;
    }

    private bool CanRemoveLine() => SelectedLine is not null && !IsBusy && !HasSent;

    [RelayCommand(CanExecute = nameof(CanRemoveLine))]
    private void RemoveLine()
    {
        if (SelectedLine is null)
        {
            return;
        }

        var index = Lines.IndexOf(SelectedLine);
        Lines.Remove(SelectedLine);
        SelectedLine = Lines.Count == 0
            ? null
            : Lines[Math.Min(index, Lines.Count - 1)];
    }

    private bool CanSend() => !IsBusy && !HasSent;

    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task SendAsync()
    {
        if (!TryValidate(out var validationMessage))
        {
            this.SetWarningStatus(validationMessage);
            return;
        }

        IsBusy = true;
        try
        {
            var payload = BuildPayload();
            var created = await _estimateClient.CreateAsync(payload).ConfigureAwait(true);

            HasSent = true;
            OnPropertyChanged(nameof(SendLabel));
            this.SetInfoStatus(
                $"Devis #{created.Id} envoyé ({created.Bill.Count} ligne(s)). La demande #{QueryId} est validée.");
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            this.SetWarningStatus("Accès refusé à l'API devis (scope 'api'). Déconnectez puis reconnectez-vous.");
        }
        catch (Exception ex)
        {
            this.SetErrorStatus($"Erreur lors de l'envoi du devis: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task BackAsync()
    {
        var app = (App?)Application.Current;
        if (app is null)
        {
            throw new InvalidOperationException("Application PostIt indisponible.");
        }

        await app.GoBackAsync().ConfigureAwait(true);
    }

    internal EstimateDto BuildPayload()
    {
        return new EstimateDto
        {
            CommandId = QueryId,
            ClientId = ClientId,
            CommandType = BillingCode,
            Title = EstimateTitle.Trim(),
            Description = EstimateDescription.Trim(),
            Bill = Lines.Select(line => new EstimateLineDto
            {
                Id = line.Id,
                Name = line.Name.Trim(),
                Description = line.Description.Trim(),
                Count = Math.Max(1, (int)Math.Round(line.Count)),
                UnitaryCost = line.UnitaryCost,
                Currency = string.IsNullOrWhiteSpace(line.Currency) ? "EUR" : line.Currency.Trim(),
            }).ToList(),
        };
    }

    private bool TryValidate(out string message)
    {
        if (string.IsNullOrWhiteSpace(EstimateTitle))
        {
            message = "Le titre du devis est requis.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(ClientId))
        {
            message = "La demande sélectionnée n'identifie pas de client.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(BillingCode))
        {
            message = "La demande sélectionnée n'a pas de code de facturation.";
            return false;
        }

        if (Lines.Count == 0)
        {
            message = "Ajoutez au moins une ligne au devis.";
            return false;
        }

        foreach (var line in Lines)
        {
            if (string.IsNullOrWhiteSpace(line.Name))
            {
                message = "Chaque ligne doit avoir un nom.";
                return false;
            }

            if (line.Name.Trim().Length > 256)
            {
                message = $"Le nom de la ligne « {line.Name.Trim()[..20]}… » dépasse 256 caractères.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(line.Description))
            {
                message = $"La ligne « {line.Name.Trim()} » doit avoir une description.";
                return false;
            }

            if (line.Description.Trim().Length > 512)
            {
                message = $"La description de la ligne « {line.Name.Trim()} » dépasse 512 caractères.";
                return false;
            }

            if (line.Count < 1)
            {
                message = $"La quantité de la ligne « {line.Name.Trim()} » doit être d'au moins 1.";
                return false;
            }
        }

        message = string.Empty;
        return true;
    }

    private void OnLinesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems.OfType<EstimateLineItemViewModel>())
            {
                item.PropertyChanged -= OnLinePropertyChanged;
            }
        }

        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems.OfType<EstimateLineItemViewModel>())
            {
                item.PropertyChanged += OnLinePropertyChanged;
            }
        }

        RaiseTotalsChanged();
    }

    private void OnLinePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(EstimateLineItemViewModel.LineTotal)
            or nameof(EstimateLineItemViewModel.Currency))
        {
            RaiseTotalsChanged();
        }
    }

    private void RaiseTotalsChanged()
    {
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(TotalLabel));
    }
}
