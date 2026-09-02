using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Services;
using Yavsc;
using Yavsc.Abstract.Workflow;
using Yavsc.Api.Client;
using Yavsc.Models.Billing;
using Yavsc.Models.Haircut;

namespace PostIt.ViewModels;

public partial class BillingCommandPageViewModel : ViewModelBase
{
    private readonly BillingApiClient _billingClient;

    public ActivityInfo Activity { get; }
    public ActivityUserDisplayItem Performer { get; }
    public CommandFormSummary Form { get; }

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

    [ObservableProperty]
    public partial ObservableCollection<HairPrestationDto> AvailablePrestations { get; set; } = new();

    [ObservableProperty]
    public partial HairPrestationDto? SelectedPrestation { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<SelectableHairPrestationItem> MultiPrestations { get; set; } = new();

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
    public bool IsSupported => IsRdv || IsBrush || IsMultiBrush;
    public bool IsRdv => string.Equals(Form.ActionName, BillingCodes.Rdv, StringComparison.Ordinal);
    public bool IsBrush => string.Equals(Form.ActionName, BillingCodes.Brush, StringComparison.Ordinal);
    public bool IsMultiBrush => string.Equals(Form.ActionName, BillingCodes.MBrush, StringComparison.Ordinal);
    public bool ShowsReason => IsRdv;
    public bool ShowsAdditionalInfo => IsBrush;
    public bool ShowsSinglePrestation => IsBrush;
    public bool ShowsMultiplePrestations => IsMultiBrush;
    public string BillingRoute => $"/billing/{Form.ActionName}";
    public bool IsEditingExisting => ExistingQueryId.HasValue;
    public string SubmitLabel => IsEditingExisting ? "Mettre à jour la commande" : "Poster la commande";
    public string SupportMessage => IsSupported
        ? IsRdv
            ? "Complétez les informations du rendez-vous puis postez la commande."
            : IsBrush
                ? "Choisissez une prestation coiffure puis postez la commande."
                : "Choisissez une ou plusieurs prestations coiffure puis postez la commande."
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
        ActivityInfo activity,
        ActivityUserDisplayItem performer,
        CommandFormSummary form,
        BillingApiClient billingClient)
    {
        Activity = activity ?? throw new ArgumentNullException(nameof(activity));
        Performer = performer ?? throw new ArgumentNullException(nameof(performer));
        Form = form ?? throw new ArgumentNullException(nameof(form));
        _billingClient = billingClient ?? throw new ArgumentNullException(nameof(billingClient));

        EventDateText = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
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
        UseCurrentLocationCommand.NotifyCanExecuteChanged();
    }

    public async Task InitializeAsync(BillingQueryDetailsDto? existingQuery = null)
    {
        if (!IsBrush && !IsMultiBrush)
        {
            if (existingQuery is not null)
            {
                ApplyExistingQuery(existingQuery);
            }

            return;
        }

        IsBusy = true;
        try
        {
            var prestations = await _billingClient.GetHairPrestationsAsync(Form.ActionName).ConfigureAwait(true);
            AvailablePrestations = new ObservableCollection<HairPrestationDto>(prestations ?? new List<HairPrestationDto>());
            SelectedPrestation = AvailablePrestations.FirstOrDefault();
            MultiPrestations = new ObservableCollection<SelectableHairPrestationItem>(AvailablePrestations.Select(SelectableHairPrestationItem.FromDto));

            StatusMessage = AvailablePrestations.Count == 0
                ? "Aucune prestation coiffure disponible."
                : SupportMessage;
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            StatusMessage = "Accès refusé au catalogue de prestations (scope 'api'). Déconnectez puis reconnectez-vous.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur lors du chargement des prestations: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }

        if (existingQuery is not null)
        {
            ApplyExistingQuery(existingQuery);
        }
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

        if (IsRdv && string.IsNullOrWhiteSpace(Reason))
        {
            StatusMessage = "Le motif du rendez-vous est requis.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Address))
        {
            StatusMessage = "L'adresse du rendez-vous est requise.";
            return;
        }

        if (!TryParseCoordinates(out var latitude, out var longitude, out var coordinateError))
        {
            StatusMessage = coordinateError;
            return;
        }

        IsBusy = true;
        try
        {
            var address = Address.Trim();
            var locationPayload = BuildLocationPayload(address, latitude, longitude);

            var payload = new BillingQueryDetailsDto
            {
                Id = ExistingQueryId ?? 0,
                BillingCode = Form.ActionName,
                ActivityCode = Activity.Code,
                PerformerId = Performer.PerformerId,
                Consent = Consent,
                EventDate = eventDate,
                Status = CommandStatus,
                Reason = Reason.Trim(),
                AdditionalInfo = string.IsNullOrWhiteSpace(AdditionalInfo) ? string.Empty : AdditionalInfo.Trim(),
                Location = new BillingLocationDto
                {
                    Address = address,
                    Latitude = latitude,
                    Longitude = longitude,
                }
            };

            if (IsRdv)
            {
                if (IsEditingExisting)
                {
                    await _billingClient.UpdateAsync(Form.ActionName, ExistingQueryId!.Value, payload).ConfigureAwait(true);
                }
                else
                {
                    await _billingClient.CreateAsync(Form.ActionName, new
                    {
                        ActivityCode = Activity.Code,
                        PerformerId = Performer.PerformerId,
                        Consent,
                        EventDate = eventDate,
                        Location = locationPayload,
                        Reason = payload.Reason,
                        Status = payload.Status,
                    }).ConfigureAwait(true);
                }
            }
            else if (IsBrush)
            {
                if (SelectedPrestation is null)
                {
                    StatusMessage = "Sélectionnez une prestation coiffure.";
                    return;
                }

                payload.PrestationId = SelectedPrestation.Id;

                if (IsEditingExisting)
                {
                    await _billingClient.UpdateAsync(Form.ActionName, ExistingQueryId!.Value, payload).ConfigureAwait(true);
                }
                else
                {
                    await _billingClient.CreateAsync(Form.ActionName, new
                    {
                        ActivityCode = Activity.Code,
                        PerformerId = Performer.PerformerId,
                        Consent,
                        EventDate = (DateTime?)eventDate,
                        Location = locationPayload,
                        PrestationId = SelectedPrestation.Id,
                        AdditionalInfo = string.IsNullOrWhiteSpace(AdditionalInfo) ? null : AdditionalInfo.Trim(),
                        Status = payload.Status,
                    }).ConfigureAwait(true);
                }
            }
            else if (IsMultiBrush)
            {
                var selectedPrestations = MultiPrestations.Where(x => x.IsSelected).ToList();
                if (selectedPrestations.Count == 0)
                {
                    StatusMessage = "Sélectionnez au moins une prestation coiffure.";
                    return;
                }

                payload.PrestationIds = selectedPrestations.Select(x => x.Id).ToList();

                if (IsEditingExisting)
                {
                    await _billingClient.UpdateAsync(Form.ActionName, ExistingQueryId!.Value, payload).ConfigureAwait(true);
                }
                else
                {
                    await _billingClient.CreateAsync(Form.ActionName, new
                    {
                        ActivityCode = Activity.Code,
                        PerformerId = Performer.PerformerId,
                        Consent,
                        EventDate = eventDate,
                        Location = locationPayload,
                        Prestations = selectedPrestations.Select(x => new { PrestationId = x.Id }).ToList(),
                        Status = payload.Status,
                    }).ConfigureAwait(true);
                }
            }

            StatusMessage = IsEditingExisting
                ? $"Commande #{ExistingQueryId} mise à jour sur {BillingRoute} pour {Performer.UserName}."
                : $"Commande transmise sur {BillingRoute} pour {Performer.UserName}.";
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

    [RelayCommand(CanExecute = nameof(CanUseCurrentLocation))]
    private async Task UseCurrentLocationAsync()
    {
        if (!CanUseCurrentLocation)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var result = await Platform.TryGetCurrentLocationAsync(default).ConfigureAwait(true);
            if (!result.IsSuccess || !result.Latitude.HasValue || !result.Longitude.HasValue)
            {
                StatusMessage = result.Message;
                return;
            }

            LatitudeText = result.Latitude.Value.ToString(CultureInfo.InvariantCulture);
            LongitudeText = result.Longitude.Value.ToString(CultureInfo.InvariantCulture);
            StatusMessage = string.IsNullOrWhiteSpace(Address)
                ? "Position récupérée. Complétez l'adresse puis envoyez la commande."
                : result.Message;
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "La récupération de la position a été annulée.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Impossible de récupérer la position: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyExistingQuery(BillingQueryDetailsDto existingQuery)
    {
        ExistingQueryId = existingQuery.Id;
        CommandStatus = existingQuery.Status;
        Consent = existingQuery.Consent;
        Reason = existingQuery.Reason ?? string.Empty;
        AdditionalInfo = existingQuery.AdditionalInfo ?? string.Empty;

        if (existingQuery.EventDate is not null)
        {
            EventDateText = existingQuery.EventDate.Value
                .ToLocalTime()
                .ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
        }

        if (existingQuery.Location is not null)
        {
            Address = existingQuery.Location.Address ?? string.Empty;
            LatitudeText = existingQuery.Location.Latitude?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            LongitudeText = existingQuery.Location.Longitude?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        }

        if (IsBrush && existingQuery.PrestationId is not null)
        {
            SelectedPrestation = AvailablePrestations.FirstOrDefault(x => x.Id == existingQuery.PrestationId.Value);
        }

        if (IsMultiBrush)
        {
            var selectedIds = existingQuery.PrestationIds is null
                ? new HashSet<long>()
                : new HashSet<long>(existingQuery.PrestationIds);
            foreach (var item in MultiPrestations)
            {
                item.IsSelected = selectedIds.Contains(item.Id);
            }
        }

        StatusMessage = $"Commande #{existingQuery.Id} chargée.";
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

    private static object BuildLocationPayload(string address, double? latitude, double? longitude)
    {
        if (latitude.HasValue && longitude.HasValue)
        {
            return new
            {
                Address = address,
                Latitude = latitude.Value,
                Longitude = longitude.Value,
            };
        }

        return new
        {
            Address = address,
        };
    }

    private bool TryParseCoordinates(out double? latitude, out double? longitude, out string error)
    {
        latitude = null;
        longitude = null;
        error = string.Empty;

        var latitudeMissing = string.IsNullOrWhiteSpace(LatitudeText);
        var longitudeMissing = string.IsNullOrWhiteSpace(LongitudeText);

        if (latitudeMissing && longitudeMissing)
        {
            return true;
        }

        if (latitudeMissing != longitudeMissing)
        {
            error = "Latitude et longitude doivent être renseignées ensemble, ou laissées vides toutes les deux.";
            return false;
        }

        if (!TryParseCoordinate(LatitudeText, out var parsedLatitude))
        {
            error = "Latitude invalide.";
            return false;
        }

        if (!TryParseCoordinate(LongitudeText, out var parsedLongitude))
        {
            error = "Longitude invalide.";
            return false;
        }

        latitude = parsedLatitude;
        longitude = parsedLongitude;
        return true;
    }
}
