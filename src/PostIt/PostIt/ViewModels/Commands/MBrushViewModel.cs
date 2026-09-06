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

namespace PostIt.ViewModels.Commands;

public partial class MBrushViewModel : BrushViewModel
{
    public override string SupportMessage => "Choisissez une ou plusieurs prestations coiffure puis postez la commande.";

    [ObservableProperty]
    public partial ObservableCollection<SelectableHairPrestationItem> MultiPrestations { get; set; } = new();

    public MBrushViewModel(ActivityInfo activity, ActivityUserDisplayItem performer, CommandFormSummary form, BillingApiClient billingClient)
        : base(activity, performer, form, billingClient)
    {
    }

    public override async Task LoadAsync()
    {
        await base.LoadAsync().ConfigureAwait(true);
        MultiPrestations = new ObservableCollection<SelectableHairPrestationItem>(
            AvailablePrestations.Select(SelectableHairPrestationItem.FromDto));
    }

    protected override void ApplyExistingQuery(BillingQueryDetailsDto existingQuery)
    {
        base.ApplyExistingQuery(existingQuery);

        var selectedIds = existingQuery.PrestationIds is null
            ? new HashSet<long>()
            : new HashSet<long>(existingQuery.PrestationIds);

        foreach (var item in MultiPrestations)
        {
            item.IsSelected = selectedIds.Contains(item.Id);
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

        var selectedPrestations = MultiPrestations.Where(x => x.IsSelected).ToList();
        if (selectedPrestations.Count == 0)
        {
            this.SetWarningStatus("Sélectionnez au moins une prestation coiffure.");
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
                Location = new BillingLocationDto
                {
                    Address = address,
                    Latitude = Latitude,
                    Longitude = Longitude,
                },
                PrestationIds = selectedPrestations.Select(x => x.Id).ToList(),
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
                    Prestations = selectedPrestations.Select(x => new { PrestationId = x.Id }).ToList(),
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
