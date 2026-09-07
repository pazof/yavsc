using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Helpers;
using Yavsc;
using Yavsc.Abstract.Workflow;
using Yavsc.Api.Client;

namespace PostIt.ViewModels;

public partial class BillingQueryDetailsPageViewModel : ViewModelBase, IActionStatusViewModel
{
    private readonly BillingApiClient _billingClient;
    private readonly BillingQueryDetailsDto _details;

    public ActivityInfo Activity { get; }
    public ActivityUserDisplayItem Performer { get; }
    public CommandFormSummary Form { get; }
    public bool IsReadOnly { get; }

    public long Id => _details.Id;
    public string Title => $"Detail commande #{_details.Id}";
    public string ContextLabel => $"{Performer.UserName} · {Activity.Name} · {Form.Title}";
    public string StatusLabel => _details.Status.ToString();
    public string StatusGlyph => GetStatusGlyph(_details.Status);
    public string StatusBadgeBackground => GetStatusBadgeBackground(_details.Status);
    public string StatusBadgeBorder => GetStatusBadgeBorder(_details.Status);
    public string StatusBadgeForeground => GetStatusBadgeForeground(_details.Status);
    public string TitleForeground => StatusBadgeForeground;
    public string BillingCode => _details.BillingCode;
    public string Description => EmptyAsPlaceholder(_details.Description, "(sans description)");
    public string Reason => EmptyAsPlaceholder(_details.Reason, "(aucun motif)");
    public string AdditionalInfo => EmptyAsPlaceholder(_details.AdditionalInfo, "(aucune info complementaire)");
    public string ClientId => EmptyAsPlaceholder(_details.ClientId, "(non renseigne)");
    public string EventDateLabel => _details.EventDate?.ToLocalTime().ToString("f") ?? "Date non precisee";
    public string ConsentLabel => _details.Consent ? "Oui" : "Non";
    public string ProvisionalLabel => _details.Provisional.HasValue ? _details.Provisional.Value.ToString("0.00") : "(non renseigne)";
    public string LocationLabel => BuildLocationLabel(_details.Location);
    public string PrestationsLabel => BuildPrestationsLabel(_details);
    public bool CanEdit => !IsReadOnly;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Pret.";

    [ObservableProperty]
    public partial StatusNotice ActionStatus { get; set; } = StatusNotice.Info("Pret.");

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

    public BillingQueryDetailsPageViewModel(
        ActivityInfo activity,
        ActivityUserDisplayItem performer,
        CommandFormSummary form,
        BillingApiClient billingClient,
        BillingQueryDetailsDto details,
        bool isReadOnly)
    {
        Activity = activity ?? throw new ArgumentNullException(nameof(activity));
        Performer = performer ?? throw new ArgumentNullException(nameof(performer));
        Form = form ?? throw new ArgumentNullException(nameof(form));
        _billingClient = billingClient ?? throw new ArgumentNullException(nameof(billingClient));
        _details = details ?? throw new ArgumentNullException(nameof(details));
        IsReadOnly = isReadOnly;

        this.SetInfoStatus("Details de commande charges.");
    }

    [RelayCommand]
    private async Task OpenEditorAsync()
    {
        if (IsReadOnly)
        {
            this.SetWarningStatus("Mode lecture seule: edition desactivee.");
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
            var vm = Form.CreateCommandPageViewModel(Activity, Performer, _billingClient);
            if (vm is null)
            {
                this.SetWarningStatus("Ce formulaire n'est pas encore pris en charge en edition.");
                return;
            }

            await vm.InitializeAsync(_details).ConfigureAwait(true);
            await app.PushPageAsync(vm).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            this.SetErrorStatus($"Erreur lors de l'ouverture en edition: {ex.Message}");
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

    private static string EmptyAsPlaceholder(string? value, string placeholder)
        => string.IsNullOrWhiteSpace(value) ? placeholder : value;

    private static string BuildLocationLabel(BillingLocationDto? location)
    {
        if (location is null)
        {
            return "(non renseignee)";
        }

        var text = EmptyAsPlaceholder(location.Address, "adresse vide");
        if (location.Latitude.HasValue && location.Longitude.HasValue)
        {
            text += $" ({location.Latitude.Value:0.####}, {location.Longitude.Value:0.####})";
        }

        return text;
    }

    private static string BuildPrestationsLabel(BillingQueryDetailsDto details)
    {
        if (details.PrestationIds.Count > 0)
        {
            return string.Join(", ", details.PrestationIds.Select(static id => id.ToString()));
        }

        return details.PrestationId.HasValue
            ? details.PrestationId.Value.ToString()
            : "(aucune)";
    }

    private static string GetStatusBadgeBackground(QueryStatus status)
        => status switch
        {
            QueryStatus.Accepted => "#E6F7EC",
            QueryStatus.InProgress => "#FFF4D6",
            QueryStatus.Rejected => "#FDECEA",
            QueryStatus.Failed => "#ECEFF1",
            QueryStatus.Success => "#E8F8EF",
            _ => "#EAF3FF",
        };

    private static string GetStatusBadgeBorder(QueryStatus status)
        => status switch
        {
            QueryStatus.Accepted => "#2E7D32",
            QueryStatus.InProgress => "#B26A00",
            QueryStatus.Rejected => "#C62828",
            QueryStatus.Failed => "#607D8B",
            QueryStatus.Success => "#1E8E3E",
            _ => "#2A5EA8",
        };

    private static string GetStatusBadgeForeground(QueryStatus status)
        => status switch
        {
            QueryStatus.Accepted => "#1B5E20",
            QueryStatus.InProgress => "#7A4A00",
            QueryStatus.Rejected => "#8E0000",
            QueryStatus.Failed => "#37474F",
            QueryStatus.Success => "#145A2A",
            _ => "#1A4178",
        };

    private static string GetStatusGlyph(QueryStatus status)
        => status switch
        {
            QueryStatus.Accepted => "OK",
            QueryStatus.InProgress => "~",
            QueryStatus.Rejected => "!",
            QueryStatus.Failed => "X",
            QueryStatus.Success => "V",
            _ => "i",
        };
}