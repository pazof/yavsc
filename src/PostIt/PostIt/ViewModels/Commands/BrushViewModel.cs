using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Yavsc.Abstract.Workflow;
using Yavsc.Api.Client;
using Yavsc.Models.Billing;
using Yavsc.Models.Haircut;
namespace PostIt.ViewModels.Commands;

public partial class BrushViewModel : RdvViewModel
{
    public override string SupportMessage => "Choisissez une prestation coiffure puis postez la commande.";

    [ObservableProperty]
    public partial ObservableCollection<HairPrestationDto> AvailablePrestations { get; set; } = new();

    [ObservableProperty]
    public partial HairPrestationDto? SelectedPrestation { get; set; }

    public BrushViewModel(ActivityInfo activity, ActivityUserDisplayItem performer, CommandFormSummary form, BillingApiClient billingClient)
        : base(activity, performer, form, billingClient)
    {
    }

    public override async Task LoadAsync()
    {
        var prestations = await _billingClient.GetHairPrestationsAsync(Form.ActionName);

        AvailablePrestations = new ObservableCollection<HairPrestationDto>
            (prestations ?? new List<HairPrestationDto>());

        if (SelectedPrestation is null)
        {
            SelectedPrestation = AvailablePrestations.FirstOrDefault();
        }

    }

    protected override void ApplyExistingQuery(BillingQueryDetailsDto existingQuery)
    {
        base.ApplyExistingQuery(existingQuery);

        if (existingQuery.PrestationId is not null)
        {
            SelectedPrestation = AvailablePrestations.FirstOrDefault(x => x.Id == existingQuery.PrestationId.Value);
        }

        IsBusy = true;
        try
        {
            if (SelectedPrestation is null)
            {
                SelectedPrestation = AvailablePrestations.FirstOrDefault();
            }

            this.SetStatus(
                AvailablePrestations.Count == 0
                    ? "Aucune prestation coiffure disponible."
                    : SupportMessage,
                AvailablePrestations.Count == 0 ? StatusSeverity.Warning : StatusSeverity.Info);
        }
        catch (HttpRequestException ex)
        when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            this.SetWarningStatus("Accès refusé au catalogue de prestations (scope 'api'). Déconnectez puis reconnectez-vous.");
        }
        catch (Exception ex)
        {
            this.SetErrorStatus($"Erreur lors du chargement des prestations: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected override async Task SubmitAsync()
    {
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

        if (SelectedPrestation is null)
        {
            this.SetWarningStatus("Sélectionnez une prestation coiffure.");
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
                    EventDate = (DateTime?)EventDate,
                    Location = locationPayload,
                    PrestationId = SelectedPrestation.Id,
                    AdditionalInfo = string.IsNullOrWhiteSpace(AdditionalInfo) ? null : AdditionalInfo.Trim(),
                    Status = payload.Status,
                }).ConfigureAwait(true);
            }

            this.SetInfoStatus(IsEditingExisting
                ? $"Commande #{ExistingQueryId} mise à jour sur {BillingRoute} pour {Performer.UserName}."
                : $"Commande transmise sur {BillingRoute} pour {Performer.UserName}.");
        }
        catch (HttpRequestException ex)
        when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            this.SetWarningStatus("Accès refusé au billing (scope 'api'). Déconnectez puis reconnectez-vous.");
        }
        catch (Exception ex)
        {
            this.SetErrorStatus($"Erreur lors de l'envoi de la commande: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }

    }
}
