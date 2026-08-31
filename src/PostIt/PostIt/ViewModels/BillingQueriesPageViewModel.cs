using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Yavsc.Api.Client;
using Yavsc.Abstract.Workflow;

namespace PostIt.ViewModels;

public partial class BillingQueriesPageViewModel : ViewModelBase
{
    private readonly BillingApiClient _billingClient;

    public ActivityBrowseItemDto Activity { get; }
    public ActivityUserDisplayItem Performer { get; }
    public CommandFormSummaryDto Form { get; }

    [ObservableProperty]
    public partial ObservableCollection<BillingQueryDisplayItem> Queries { get; set; } = new();

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Chargement des commandes...";

    public string Title => $"Commandes {Form.Title}";
    public string ContextLabel => $"{Performer.UserName} · {Activity.Name}";

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
        ActivityBrowseItemDto activity,
        ActivityUserDisplayItem performer,
        CommandFormSummaryDto form,
        BillingApiClient billingClient)
    {
        Activity = activity ?? throw new ArgumentNullException(nameof(activity));
        Performer = performer ?? throw new ArgumentNullException(nameof(performer));
        Form = form ?? throw new ArgumentNullException(nameof(form));
        _billingClient = billingClient ?? throw new ArgumentNullException(nameof(billingClient));
    }

    public Task InitializeAsync() => RefreshAsync();

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            var list = await _billingClient.GetQuerySummariesAsync(Form.ActionName).ConfigureAwait(true);
            var filtered = (list ?? new())
                .Where(q => q.ActivityCode == Activity.Code && q.PerformerId == Performer.PerformerId)
                .OrderByDescending(q => q.EventDate ?? DateTime.MinValue)
                .ThenByDescending(q => q.Id)
                .Select(BillingQueryDisplayItem.FromDto)
                .ToList();

            Queries = new ObservableCollection<BillingQueryDisplayItem>(filtered);
            StatusMessage = filtered.Count == 0
                ? "Aucune commande trouvée pour ce formulaire."
                : $"{filtered.Count} commande(s) chargée(s).";
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
}