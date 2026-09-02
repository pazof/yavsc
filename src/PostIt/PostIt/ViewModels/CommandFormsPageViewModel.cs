using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Helpers;
using Yavsc.Abstract.Workflow;
using Yavsc.Api.Client;

namespace PostIt.ViewModels;

public partial class CommandFormsPageViewModel : ViewModelBase
{
    private readonly BillingApiClient _billingClient;

    public ActivityInfo Activity { get; }
    public ActivityUserDisplayItem Performer { get; }

    [ObservableProperty]
    public partial ObservableCollection<CommandFormSummary> Forms { get; set; }

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(OpenSelectedFormCommand)), NotifyCanExecuteChangedFor(nameof(OpenQueriesCommand)), NotifyCanExecuteChangedFor(nameof(OpenOngoingQueriesCommand))]
    public partial CommandFormSummary? SelectedForm { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; }

    public string Title => $"Formulaires pour {Performer.UserName}";
    public string ContextLabel => $"{Activity.Name} · {Forms.Count} formulaire(s)";

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

    public CommandFormsPageViewModel(
        ActivityInfo activity,
        ActivityUserDisplayItem performer,
        BillingApiClient billingClient)
    {
        Activity = activity ?? throw new ArgumentNullException(nameof(activity));
        Performer = performer ?? throw new ArgumentNullException(nameof(performer));
        _billingClient = billingClient ?? throw new ArgumentNullException(nameof(billingClient));

        Forms = new ObservableCollection<CommandFormSummary>((activity.Forms ?? new())
            .OrderBy(f => f.Title)
            .ThenBy(f => f.ActionName));
        SelectedForm = Forms.FirstOrDefault();
        StatusMessage = Forms.Count == 0
            ? "Aucun formulaire n'est disponible pour cette activité."
            : "Choisissez le formulaire à utiliser.";
    }

    private bool CanOpenSelectedForm() => SelectedForm is not null;

    private bool CanOpenQueries() => SelectedForm is not null;

    private bool CanOpenOngoingQueries() => SelectedForm is not null;

    [RelayCommand(CanExecute = nameof(CanOpenSelectedForm))]
    private async Task OpenSelectedFormAsync()
    {
        if (SelectedForm is null)
        {
            StatusMessage = "Sélectionnez un formulaire.";
            return;
        }

        var app = (App?)Application.Current;
        if (app is null)
        {
            throw new InvalidOperationException("Application PostIt indisponible.");
        }

        var vm = SelectedForm.CreateCommandPageViewModel(Activity, Performer,  _billingClient);
        await vm.InitializeAsync();
        await app.PushPageAsync(vm);
    }

    [RelayCommand(CanExecute = nameof(CanOpenQueries))]
    private async Task OpenQueriesAsync()
    {
        if (SelectedForm is null)
        {
            StatusMessage = "Sélectionnez un formulaire.";
            return;
        }

        var app = (App?)Application.Current;
        if (app is null)
        {
            throw new InvalidOperationException("Application PostIt indisponible.");
        }

        var vm = new BillingQueriesPageViewModel(Activity, Performer, SelectedForm, _billingClient);
        await vm.InitializeAsync();
        await app.PushPageAsync(vm);
    }

    [RelayCommand(CanExecute = nameof(CanOpenOngoingQueries))]
    private async Task OpenOngoingQueriesAsync()
    {
        if (SelectedForm is null)
        {
            StatusMessage = "Sélectionnez un formulaire.";
            return;
        }

        var app = (App?)Application.Current;
        if (app is null)
        {
            throw new InvalidOperationException("Application PostIt indisponible.");
        }

        var vm = new BillingQueriesPageViewModel(
            Activity,
            Performer,
            SelectedForm,
            _billingClient,
            isReadOnly: true,
            ongoingOnly: true);
        await vm.InitializeAsync();
        await app.PushPageAsync(vm);
    }
}
