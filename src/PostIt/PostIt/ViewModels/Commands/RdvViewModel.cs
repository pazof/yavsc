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
    public override string SupportMessage => "Complétez les informations du rendez-vous puis postez la commande.";

    [ObservableProperty]
    public partial string Address { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SuggestedAddress { get; set; } = string.Empty;

    [ObservableProperty]
    public partial double? Latitude { get; set; }

    [ObservableProperty]
    public partial double? Longitude { get; set; }


    [ObservableProperty]
    public partial DateTime EventDate { get; set; }

    public RdvViewModel(ActivityInfo activity, ActivityUserDisplayItem performer, CommandFormSummary form, BillingApiClient billingClient)
        : base(activity, performer, form, billingClient)
    {
        EventDate = DateTime.Now.AddDays(1);
    }

    public bool HasSuggestedAddress => !string.IsNullOrWhiteSpace(SuggestedAddress);

    protected override void ApplyExistingQuery(BillingQueryDetailsDto existingQuery)
    {
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
            Address = existingQuery.Location.Address ?? string.Empty;
            SuggestedAddress = string.Empty;
            Latitude = existingQuery.Location.Latitude;
            Longitude = existingQuery.Location.Longitude;
        }

        this.SetInfoStatus($"Commande #{existingQuery.Id} chargée.");
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
                this.SetWarningStatus(result.Message);
                return;
            }

            Latitude = result.Latitude.Value;
            Longitude = result.Longitude.Value;
            this.SetInfoStatus(string.IsNullOrWhiteSpace(Address)
                ? "Position récupérée. Complétez l'adresse puis envoyez la commande."
                : result.Message);
        }
        catch (OperationCanceledException)
        {
            this.SetWarningStatus("La récupération de la position a été annulée.");
        }
        catch (Exception ex)
        {
            this.SetErrorStatus($"Impossible de récupérer la position: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected static object BuildLocationPayload(string address, double? latitude, double? longitude)
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

    public void ApplyLocationFromMap(double latitude, double longitude)
    {
        Latitude = Math.Round(latitude, 6);
        Longitude = Math.Round(longitude, 6);

        if (string.IsNullOrWhiteSpace(Address))
        {
            this.SetInfoStatus("Position sélectionnée sur la carte. Complétez l'adresse puis envoyez la commande.");
            return;
        }

        this.SetInfoStatus("Position sélectionnée sur la carte.");
    }

    public void ApplyResolvedAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return;

        var trimmedAddress = address.Trim();
        if (string.IsNullOrWhiteSpace(Address))
        {
            Address = trimmedAddress;
            SuggestedAddress = string.Empty;
            this.SetInfoStatus("Adresse mise à jour depuis la carte.");
            return;
        }

        if (string.Equals(Address.Trim(), trimmedAddress, StringComparison.Ordinal))
        {
            SuggestedAddress = string.Empty;
            return;
        }

        SuggestedAddress = trimmedAddress;
        this.SetInfoStatus("Adresse suggérée depuis la carte. Appliquez-la si besoin.");
    }

    [RelayCommand(CanExecute = nameof(HasSuggestedAddress))]
    private void ApplySuggestedAddress()
    {
        if (string.IsNullOrWhiteSpace(SuggestedAddress))
            return;

        Address = SuggestedAddress.Trim();
        SuggestedAddress = string.Empty;
        this.SetInfoStatus("Adresse suggérée appliquée.");
    }

    partial void OnSuggestedAddressChanged(string value)
    {
        OnPropertyChanged(nameof(HasSuggestedAddress));
        ApplySuggestedAddressCommand.NotifyCanExecuteChanged();
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

        if (string.IsNullOrWhiteSpace(Address))
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
            var address = Address.Trim();
            var locationPayload = BuildLocationPayload(address, Latitude, Longitude);

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
                Location = new BillingLocationDto
                {
                    Address = address,
                    Latitude = Latitude,
                    Longitude = Longitude,
                }
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
