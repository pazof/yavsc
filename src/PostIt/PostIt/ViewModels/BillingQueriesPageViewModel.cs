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
using Yavsc;
using Yavsc.Api.Client;
using Yavsc.Abstract.Workflow;

namespace PostIt.ViewModels;

public partial class BillingQueriesPageViewModel : ViewModelBase
{
    private readonly BillingApiClient _billingClient;

    public ActivityInfo Activity { get; }
    public ActivityUserDisplayItem Performer { get; }
    public CommandFormSummary Form { get; }
    public bool IsReadOnly { get; }
    public bool OngoingOnly { get; }

    [ObservableProperty]
    public partial ObservableCollection<BillingQueryDisplayItem> Queries { get; set; } = new();

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(OpenSelectedQueryCommand))]
    public partial BillingQueryDisplayItem? SelectedQuery { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Chargement des commandes...";

    [ObservableProperty]
    public partial StatusNotice ActionStatus { get; set; } = StatusNotice.FromMessage("Chargement des commandes...");

    partial void OnStatusMessageChanged(string value)
    {
        ActionStatus = StatusNotice.FromMessage(value);
    }

    public string Title => IsReadOnly
        ? $"Demandes en cours ({Form.Title})"
        : $"Commandes {Form.Title}";
    public string ContextLabel => $"{Performer.UserName} · {Activity.Name}";
    public bool CanOpenDetails => !IsReadOnly;

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

    public BillingQueriesPageViewModel(
        ActivityInfo activity,
        ActivityUserDisplayItem performer,
        CommandFormSummary form,
        BillingApiClient billingClient,
        bool isReadOnly = false,
        bool ongoingOnly = false)
    {
        Activity = activity ?? throw new ArgumentNullException(nameof(activity));
        Performer = performer ?? throw new ArgumentNullException(nameof(performer));
        Form = form ?? throw new ArgumentNullException(nameof(form));
        _billingClient = billingClient ?? throw new ArgumentNullException(nameof(billingClient));
        IsReadOnly = isReadOnly;
        OngoingOnly = ongoingOnly;
    }

    public Task InitializeAsync() => RefreshAsync();

    private bool CanOpenSelectedQuery() => !IsReadOnly && SelectedQuery is not null;

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            var list = await _billingClient.GetQuerySummariesAsync(Form.ActionName).ConfigureAwait(true);
            var filtered = (list ?? new())
                .Where(q => q.ActivityCode == Activity.Code && q.PerformerId == Performer.PerformerId)
                .Where(q => !OngoingOnly || IsOngoingStatus(q.Status))
                .OrderByDescending(q => q.EventDate ?? DateTime.MinValue)
                .ThenByDescending(q => q.Id)
                .Select(BillingQueryDisplayItem.FromDto)
                .ToList();

            Queries = new ObservableCollection<BillingQueryDisplayItem>(filtered);
            StatusMessage = BuildLoadedStatusMessage(filtered.Count);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            Queries = new ObservableCollection<BillingQueryDisplayItem>();
            StatusMessage = "Accès refusé au billing (scope 'api'). Déconnectez puis reconnectez-vous.";
        }
        catch (Exception ex)
        {
            Queries = new ObservableCollection<BillingQueryDisplayItem>();
            StatusMessage = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedQuery))]
    public async Task OpenSelectedQueryAsync()
    {
        if (IsReadOnly)
        {
            StatusMessage = "Mode lecture seule: l'ouverture en modification est désactivée.";
            return;
        }

        if (SelectedQuery is null)
        {
            StatusMessage = "Sélectionnez une commande.";
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
            var details = await _billingClient.GetQueryAsync(Form.ActionName, SelectedQuery.Id).ConfigureAwait(true);
            var vm = Form.CreateCommandPageViewModel(Activity, Performer, _billingClient);
            await vm!.InitializeAsync(details).ConfigureAwait(true);
            await app.PushPageAsync(vm).ConfigureAwait(true);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            StatusMessage = "Accès refusé au billing (scope 'api'). Déconnectez puis reconnectez-vous.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur lors de l'ouverture: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private string BuildLoadedStatusMessage(int count)
    {
        if (count == 0)
        {
            return OngoingOnly
                ? "Aucune demande en cours pour ce formulaire."
                : "Aucune commande trouvée pour ce formulaire.";
        }

        if (OngoingOnly)
        {
            return $"{count} demande(s) en cours chargée(s) (lecture seule).";
        }

        return $"{count} commande(s) chargée(s).";
    }

    private static bool IsOngoingStatus(QueryStatus status)
        => status is QueryStatus.Inserted or QueryStatus.Accepted or QueryStatus.InProgress;
}
