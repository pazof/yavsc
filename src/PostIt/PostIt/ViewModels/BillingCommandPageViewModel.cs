using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Yavsc;
using Yavsc.Abstract.Workflow;
using Yavsc.Api.Client;
using Yavsc.Models.Billing;
using Yavsc.Models.Relationship;

namespace PostIt.ViewModels;

public partial class BillingCommandPageViewModel : ViewModelBase
{
    private readonly BillingApiClient _billingClient;

    public ActivityBrowseItemDto Activity { get; }
    public ActivityUserDisplayItem Performer { get; }
    public CommandFormSummaryDto Form { get; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; }

    [ObservableProperty]
    public partial string EventDateText { get; set; }

    [ObservableProperty]
    public partial string Reason { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Address { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LatitudeText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LongitudeText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool Consent { get; set; } = true;

    public string Title => Form.Title;
    public string PerformerLabel => Performer.UserName;
    public string ActivityLabel => Activity.Name;
    public bool IsSupported => string.Equals(Form.ActionName, BillingCodes.Rdv, StringComparison.Ordinal);
    public string BillingRoute => $"/billing/{Form.ActionName}";
    public string SupportMessage => IsSupported
        ? "Complétez les informations du rendez-vous puis postez la commande."
        : $"Le formulaire {Form.ActionName} n'est pas encore pris en charge dans PostIt.";

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
        ActivityBrowseItemDto activity,
        ActivityUserDisplayItem performer,
        CommandFormSummaryDto form,
        BillingApiClient billingClient)
    {
        Activity = activity ?? throw new ArgumentNullException(nameof(activity));
        Performer = performer ?? throw new ArgumentNullException(nameof(performer));
        Form = form ?? throw new ArgumentNullException(nameof(form));
        _billingClient = billingClient ?? throw new ArgumentNullException(nameof(billingClient));

        EventDateText = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
        StatusMessage = SupportMessage;
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (!IsSupported)
        {
            StatusMessage = SupportMessage;
            return;
        }

        if (!Consent)
        {
            StatusMessage = "Le consentement est requis pour poster la commande.";
            return;
        }

        if (!TryParseEventDate(out var eventDate))
        {
            StatusMessage = "La date doit être saisie au format yyyy-MM-dd HH:mm.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Reason))
        {
            StatusMessage = "Le motif du rendez-vous est requis.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Address))
        {
            StatusMessage = "L'adresse du rendez-vous est requise.";
            return;
        }

        if (!TryParseCoordinate(LatitudeText, out var latitude))
        {
            StatusMessage = "Latitude invalide.";
            return;
        }

        if (!TryParseCoordinate(LongitudeText, out var longitude))
        {
            StatusMessage = "Longitude invalide.";
            return;
        }

        IsBusy = true;
        try
        {
            await _billingClient.CreateAsync(Form.ActionName, new
            {
                ActivityCode = Activity.Code,
                PerformerId = Performer.PerformerId,
                Consent,
                EventDate = eventDate,
                Location = new Location
                {
                    Address = Address.Trim(),
                    Latitude = latitude,
                    Longitude = longitude,
                },
                Reason = Reason.Trim(),
                Status = QueryStatus.Inserted,
            }).ConfigureAwait(true);

            StatusMessage = $"Commande transmise sur {BillingRoute} pour {Performer.UserName}.";
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            StatusMessage = "Accès refusé au billing (scope 'api'). Déconnectez puis reconnectez-vous.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur lors de l'envoi: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool TryParseEventDate(out DateTime eventDate)
    {
        return DateTime.TryParse(
            EventDateText,
            CultureInfo.CurrentCulture,
            DateTimeStyles.AssumeLocal,
            out eventDate)
            || DateTime.TryParse(
                EventDateText,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out eventDate);
    }

    private static bool TryParseCoordinate(string text, out double value)
    {
        return double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out value)
            || double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value);
    }
}