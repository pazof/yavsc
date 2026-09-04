using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Yavsc;
using Yavsc.Abstract.Workflow;
using Yavsc.Api.Client;
using Yavsc.Models.Billing;

namespace PostIt.ViewModels;

public abstract partial class BillingCommandPageViewModel : RemoteViewModelBase
{
    protected readonly BillingApiClient _billingClient;

    public ActivityInfo Activity { get; }
    public ActivityUserDisplayItem Performer { get; }
    public CommandFormSummary Form { get; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; }

    [ObservableProperty]
    public partial string Reason { get; set; } = string.Empty;



    [ObservableProperty]
    public partial bool Consent { get; set; } = true;


    [ObservableProperty]
    public partial string AdditionalInfo { get; set; } = string.Empty;

    [ObservableProperty]
    public partial long? ExistingQueryId { get; set; }

    [ObservableProperty]
    public partial QueryStatus CommandStatus { get; set; } = QueryStatus.Inserted;

    public bool CanUseCurrentLocation => IsSupported && !IsBusy;

    public string Title => Form.Title;
    public string PerformerLabel => Performer.UserName;
    public string ActivityLabel => Activity.Name;
    public virtual bool IsSupported => true;
    public string BillingRoute => $"/billing/{Form.ActionName}";
    public bool IsEditingExisting => ExistingQueryId.HasValue;
    public string SubmitLabel => IsEditingExisting ? "Mettre à jour la commande" : "Poster la commande";
    public virtual string SupportMessage => $"Le formulaire {Form.ActionName} n'est pas encore pris en charge dans PostIt.";

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

    public BillingCommandPageViewModel(
        ActivityInfo activity,
        ActivityUserDisplayItem performer,
        CommandFormSummary form,
        BillingApiClient billingClient)
    {
        Activity = activity ?? throw new ArgumentNullException(nameof(activity));
        Performer = performer ?? throw new ArgumentNullException(nameof(performer));
        Form = form ?? throw new ArgumentNullException(nameof(form));
        _billingClient = billingClient ?? throw new ArgumentNullException(nameof(billingClient));

        StatusMessage = SupportMessage;
    }

    partial void OnExistingQueryIdChanged(long? value)
    {
        OnPropertyChanged(nameof(IsEditingExisting));
        OnPropertyChanged(nameof(SubmitLabel));
    }

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(CanUseCurrentLocation));
    }

    public async Task InitializeAsync(BillingQueryDetailsDto? existingQuery = null)
    {
        await LoadAsync();
        if (existingQuery is not null)
        {
            ApplyExistingQuery(existingQuery);
            return;
        }
    }

    protected abstract void ApplyExistingQuery(BillingQueryDetailsDto existingQuery);



    [RelayCommand]
    protected abstract Task SubmitAsync();
}
