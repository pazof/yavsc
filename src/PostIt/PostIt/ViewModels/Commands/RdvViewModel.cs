using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Services;
using Yavsc.Abstract.Workflow;
using Yavsc.Api.Client;

namespace PostIt.ViewModels.Commands;

public partial class RdvViewModel : BillingCommandPageViewModel
{
    private long? _existingLocationId;
    private bool _hydratingExistingQuery;

    public override string SupportMessage => "Complétez les informations du rendez-vous puis postez la commande.";

    public PhysicalAddressViewModel PhysicalAddress { get; }

    public string Address
    {
        get => PhysicalAddress.Address;
        set
        {
            if (PhysicalAddress.Address == value)
                return;

            PhysicalAddress.Address = value;
            if (!_hydratingExistingQuery)
                _existingLocationId = null;
        }
    }

    public string SuggestedAddress
    {
        get => PhysicalAddress.SuggestedAddress;
        set => PhysicalAddress.SuggestedAddress = value;
    }

    public bool IsResolvingAddress
    {
        get => PhysicalAddress.IsResolvingAddress;
        set => PhysicalAddress.IsResolvingAddress = value;
    }

    public double? Latitude
    {
        get => PhysicalAddress.Latitude;
        set
        {
            if (PhysicalAddress.Latitude == value)
                return;

            PhysicalAddress.Latitude = value;
            if (!_hydratingExistingQuery)
                _existingLocationId = null;
        }
    }

    public double? Longitude
    {
        get => PhysicalAddress.Longitude;
        set
        {
            if (PhysicalAddress.Longitude == value)
                return;

            PhysicalAddress.Longitude = value;
            if (!_hydratingExistingQuery)
                _existingLocationId = null;
        }
    }

    public IAsyncRelayCommand UseCurrentLocationCommand => PhysicalAddress.UseCurrentLocationCommand;
    public IRelayCommand ApplySuggestedAddressCommand => PhysicalAddress.ApplySuggestedAddressCommand;

    [ObservableProperty]
    public partial DateTime EventDate { get; set; }

    public DateTimeOffset? EventDateSelection
    {
        get => new(EventDate);
        set
        {
            if (!value.HasValue)
                return;

            EventDate = value.Value.LocalDateTime;
        }
    }

    public bool HasSuggestedAddress => PhysicalAddress.HasSuggestedAddress;
    public bool HasSuggestedAddressPanel => PhysicalAddress.HasSuggestedAddressPanel;

    public RdvViewModel(ActivityInfo activity, ActivityUserDisplayItem performer, CommandFormSummary form, BillingApiClient billingClient)
        : base(activity, performer, form, billingClient)
    {
        PhysicalAddress = new PhysicalAddressViewModel(new NominatimReverseGeocodingService());
        EventDate = DateTime.Now.AddDays(1);
    }

    protected override void ApplyExistingQuery(BillingQueryDetailsDto existingQuery)
    {
        _hydratingExistingQuery = true;
        ExistingQueryId = existingQuery.Id;
        CommandStatus = existingQuery.Status;
        Consent = existingQuery.Consent;
        Reason = existingQuery.Reason ?? string.Empty;
        AdditionalInfo = existingQuery.AdditionalInfo ?? string.Empty;

        if (existingQuery.EventDate is not null)
        {
            EventDate = existingQuery.EventDate.Value
                .ToLocalTime();
        }

        if (existingQuery.Location is not null)
        {
            _existingLocationId = existingQuery.Location.Id;
            PhysicalAddress.Address = existingQuery.Location.Address ?? string.Empty;
            PhysicalAddress.SuggestedAddress = string.Empty;
            PhysicalAddress.Latitude = existingQuery.Location.Latitude;
            PhysicalAddress.Longitude = existingQuery.Location.Longitude;
        }
        else
        {
            _existingLocationId = null;
        }

        _hydratingExistingQuery = false;

        this.SetInfoStatus($"Commande #{existingQuery.Id} chargée.");
    }

    protected static BillingLocationDto BuildLocationPayload(string address, double? latitude, double? longitude, long? locationId = null)
    {
        if (latitude.HasValue && longitude.HasValue)
        {
            return new BillingLocationDto
            {
                Id = locationId,
                Address = address,
                Latitude = latitude.Value,
                Longitude = longitude.Value,
            };
        }

        return new BillingLocationDto
        {
            Id = locationId,
            Address = address,
        };
    }

    public void ApplyLocationFromMap(double latitude, double longitude)
    {
        PhysicalAddress.ApplyLocationFromMap(latitude, longitude);

        if (string.IsNullOrWhiteSpace(PhysicalAddress.Address))
        {
            this.SetInfoStatus("Position sélectionnée sur la carte. Complétez l'adresse puis envoyez la commande.");
            return;
        }

        this.SetInfoStatus("Position sélectionnée sur la carte.");
    }

    public void NotifyReverseGeocodingStarted()
    {
        PhysicalAddress.NotifyReverseGeocodingStarted();
        this.SetInfoStatus(string.IsNullOrWhiteSpace(PhysicalAddress.Address)
            ? "Recherche de l'adresse depuis la carte..."
            : "Recherche d'une adresse suggérée..."
        );
    }

    public void NotifyReverseGeocodingUnavailable()
    {
        PhysicalAddress.NotifyReverseGeocodingUnavailable();
        if (PhysicalAddress.HasSuggestedAddress || !string.IsNullOrWhiteSpace(PhysicalAddress.Address))
            return;

        this.SetInfoStatus("Position sélectionnée sur la carte. Complétez l'adresse puis envoyez la commande.");
    }

    public void ApplyResolvedAddress(string address)
    {
        PhysicalAddress.ApplyResolvedAddress(address);
        if (string.IsNullOrWhiteSpace(address))
            return;

        var trimmedAddress = address.Trim();
        if (string.IsNullOrWhiteSpace(PhysicalAddress.Address))
        {
            this.SetInfoStatus("Adresse mise à jour depuis la carte.");
            return;
        }

        if (string.Equals(PhysicalAddress.Address.Trim(), trimmedAddress, StringComparison.Ordinal))
        {
            return;
        }

        this.SetInfoStatus("Adresse suggérée depuis la carte. Appliquez-la si besoin.");
    }

    partial void OnEventDateChanged(DateTime value)
    {
        OnPropertyChanged(nameof(EventDateSelection));
    }

    protected override async Task SubmitAsync()
    {
        if (!IsSupported)
        {
            this.SetWarningStatus(SupportMessage);
            return;
        }

        if (!Consent)
        {
            this.SetWarningStatus("Le consentement est requis pour poster la commande.");
            return;
        }

        if (string.IsNullOrWhiteSpace(PhysicalAddress.Address))
        {
            this.SetWarningStatus("L'adresse du rendez-vous est requise.");
            return;
        }

        if (string.IsNullOrWhiteSpace(Reason))
        {
            this.SetWarningStatus("Le motif du rendez-vous est requis.");
            return;
        }

        IsBusy = true;
        try
        {
            var address = PhysicalAddress.Address.Trim();
            var locationPayload = BuildLocationPayload(address, PhysicalAddress.Latitude, PhysicalAddress.Longitude, IsEditingExisting ? _existingLocationId : null);

            var payload = new BillingQueryDetailsDto
            {
                Id = ExistingQueryId ?? 0,
                BillingCode = Form.ActionName,
                ActivityCode = Activity.Code,
                PerformerId = Performer.PerformerId,
                Consent = Consent,
                EventDate = EventDate,
                Status = CommandStatus,
                Reason = Reason.Trim(),
                AdditionalInfo = string.IsNullOrWhiteSpace(AdditionalInfo) ? string.Empty : AdditionalInfo.Trim(),
                Location = locationPayload
            };

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
                    EventDate = EventDate,
                    Location = locationPayload,
                    Reason = payload.Reason,
                    Status = payload.Status,
                }).ConfigureAwait(true);
            }

            this.SetInfoStatus(IsEditingExisting
                ? $"Commande #{ExistingQueryId} mise à jour sur {BillingRoute} pour {Performer.UserName}."
                : $"Commande transmise sur {BillingRoute} pour {Performer.UserName}.");
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            this.SetWarningStatus("Accès refusé au billing (scope 'api'). Déconnectez puis reconnectez-vous.");
        }
        catch (Exception ex)
        {
            this.SetErrorStatus($"Erreur lors de l'envoi: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public override Task LoadAsync()
    {
        return Task.CompletedTask;
    }
}
