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
using Yavsc.Models.Haircut;
namespace PostIt.ViewModels.Commands;
public partial class BrushViewModel : RdvViewModel
{

    [ObservableProperty]
    public partial ObservableCollection<HairPrestationDto> AvailablePrestations { get; set; } = new();

    [ObservableProperty]
    public partial HairPrestationDto? SelectedPrestation { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<SelectableHairPrestationItem> MultiPrestations { get; set; } = new();


    public BrushViewModel(ActivityInfo activity, ActivityUserDisplayItem performer, CommandFormSummary form, BillingApiClient billingClient)
        : base(activity, performer, form, billingClient)
    {
    }

    protected override async void ApplyExistingQuery(BillingQueryDetailsDto existingQuery)
    {
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

        IsBusy = true;
        try
        {
            var prestations = await _billingClient.GetHairPrestationsAsync
            (Form.ActionName).ConfigureAwait(true);
            AvailablePrestations = new ObservableCollection<HairPrestationDto>
            (prestations ?? new List<HairPrestationDto>());
            SelectedPrestation = AvailablePrestations.FirstOrDefault();
            MultiPrestations = new ObservableCollection<SelectableHairPrestationItem>(AvailablePrestations.Select(SelectableHairPrestationItem.FromDto));

            StatusMessage = AvailablePrestations.Count == 0
                ? "Aucune prestation coiffure disponible."
                : SupportMessage;
        }
        catch (HttpRequestException ex)
        when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
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
    }

    protected override async Task SubmitAsync()
    {



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

        if (IsBrush)
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
                        EventDate = (DateTime?)EventDate,
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
                        EventDate = EventDate,
                        Location = locationPayload,
                        Prestations = selectedPrestations.Select(x => new { PrestationId = x.Id }).ToList(),
                        Status = payload.Status,
                    }).ConfigureAwait(true);
                }
            }
        }
        catch (HttpRequestException ex)
        when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            StatusMessage = "Accès refusé au billing (scope 'api'). Déconnectez puis reconnectez-vous.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur lors de l'envoi de la commande: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }

    }
}
