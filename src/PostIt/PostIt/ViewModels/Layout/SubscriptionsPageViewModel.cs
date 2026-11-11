using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Yavsc.Abstract.Resources;
using Yavsc.Api.Client;

namespace PostIt.ViewModels;

public partial class SubscriptionsPageViewModel : ViewModelBase
{
    private readonly ResourceSubscriptionsApiClient _client;
    private readonly Func<Uri, Task<bool>> _launch;

    public SubscriptionsPageViewModel(ResourceSubscriptionsApiClient client, Func<Uri, Task<bool>>? launch = null)
    {
        _client = client;
        _launch = launch ?? LaunchAsync;
    }

    public ObservableCollection<ResourceSubscriptionOfferResponse> Offers { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubscribeCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelSubscriptionCommand))]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubscribeCommand))]
    public partial ResourceSubscriptionOfferResponse? SelectedOffer { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubscribeCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelSubscriptionCommand))]
    public partial ResourceSubscriptionResponse? CurrentSubscription { get; set; }

    [ObservableProperty]
    public partial ResourceUsageSummary? Usage { get; set; }

    [ObservableProperty]
    public partial string? StatusMessage { get; set; }

    [ObservableProperty]
    public partial bool ConfirmCancellation { get; set; }

    public override bool CanNavigateNext { get => false; protected set { } }
    public override bool CanNavigatePrevious { get => true; protected set { } }

    public Task InitializeAsync() => RunAsync(async () => Apply(await _client.GetAsync()));

    private bool CanRefresh() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private Task RefreshAsync() => RunAsync(async () =>
    {
        Apply(await _client.RefreshAsync());
        StatusMessage = "État et paiements vérifiés auprès de PayPal.";
    });

    private bool CanSubscribe() => !IsBusy && SelectedOffer is { Available: true }
        && (CurrentSubscription is null || (CurrentSubscription.Pack == SelectedOffer.Pack
            && CurrentSubscription.State is "CREATING" or "APPROVAL_PENDING"));

    [RelayCommand(CanExecute = nameof(CanSubscribe))]
    private Task SubscribeAsync() => RunAsync(async () =>
    {
        var checkout = await _client.CreateAsync(SelectedOffer!.Pack);
        CurrentSubscription = checkout.Subscription;
        if (checkout.ApprovalUrl is null)
        {
            Apply(await _client.GetAsync());
            StatusMessage = "Abonnement rapproché. Seuls les paiements confirmés accordent des crédits.";
            return;
        }
        if (!Uri.TryCreate(checkout.ApprovalUrl, UriKind.Absolute, out var uri)
            || uri.Scheme != "https" || !string.IsNullOrEmpty(uri.UserInfo) || !uri.IsDefaultPort
            || uri.Host is not ("www.paypal.com" or "www.sandbox.paypal.com"))
            throw new InvalidOperationException("Lien d'approbation PayPal invalide.");
        if (!await _launch(uri))
            throw new InvalidOperationException("Impossible d'ouvrir PayPal. Réessayez pour reprendre l'approbation.");
        StatusMessage = "Approuvez l'abonnement mensuel dans PayPal, puis revenez ici et appuyez sur Actualiser. Aucun paiement n'est confirmé par l'ouverture du navigateur.";
    });

    private bool CanCancelSubscription() => !IsBusy && CurrentSubscription is { State: not "CREATING" };

    [RelayCommand(CanExecute = nameof(CanCancelSubscription))]
    private async Task CancelSubscriptionAsync()
    {
        if (!ConfirmCancellation)
        {
            var confirmed = await App.ConfirmAsync(
                "Résilier l’abonnement ?",
                "Cette action met fin aux futurs renouvellements. Les droits déjà acquis restent conservés.",
                confirmText: "Résilier",
                cancelText: "Annuler");
            if (!confirmed)
            {
                StatusMessage = "Cochez la confirmation de résiliation avant de continuer.";
                return;
            }
            ConfirmCancellation = true;
        }

        Apply(await _client.CancelAsync(CurrentSubscription!.Id));
        ConfirmCancellation = false;
        StatusMessage = "Abonnement résilié. Les droits déjà acquis restent soumis aux périodes payées.";
    }

    private void Apply(ResourceSubscriptionsResponse response)
    {
        var pack = SelectedOffer?.Pack;
        Offers.Clear();
        foreach (var offer in response.Offers) Offers.Add(offer);
        SelectedOffer = Offers.FirstOrDefault(o => o.Pack == pack) ?? Offers.FirstOrDefault();
        CurrentSubscription = response.Subscriptions.FirstOrDefault(s => s.State is not ("CANCELLED" or "EXPIRED"));
        Usage = response.Usage;
    }

    private async Task RunAsync(Func<Task> action)
    {
        IsBusy = true;
        StatusMessage = null;
        try { await action(); }
        catch (Exception ex) { StatusMessage = $"Opération d'abonnement impossible : {ex.Message}"; }
        finally { IsBusy = false; }
    }

    private static async Task<bool> LaunchAsync(Uri uri)
    {
        var topLevel = TopLevel.GetTopLevel((App.Current as App)?.View);
        return topLevel is not null && await topLevel.Launcher.LaunchUriAsync(uri);
    }

    public static async Task OpenAsync()
    {
        var app = App.Current as App ?? throw new InvalidOperationException("Application PostIt indisponible.");
        var vm = app.ServiceProvider!.GetRequiredService<SubscriptionsPageViewModel>();
        await vm.InitializeAsync();
        await app.PushPageAsync(vm);
    }
}
