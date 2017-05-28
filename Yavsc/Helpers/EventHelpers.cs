using Microsoft.Extensions.Localization;

namespace Yavsc.Helpers
{
    using Models.Workflow;
    using Models.Messaging;
    using Yavsc.Models.Haircut;
    using System.Linq;
    using System.Globalization;

    public static class EventHelpers
    {
        public static RdvQueryEvent CreateEvent(this RdvQuery query,
        IStringLocalizer SR)
        {
            var yaev = new RdvQueryEvent
            {
                Sender = query.ClientId,
                Message = string.Format(SR["RdvToPerf"],
                query.Client.UserName,
                query.EventDate.ToString("dddd dd/MM/yyyy à HH:mm"),
                query.Location.Address,
                query.ActivityCode)+
                "\n"+query.Reason,
                Client =  new ClientProviderInfo { 
                    UserName = query.Client.UserName ,
                    UserId = query.ClientId,
                    Avatar = query.Client.Avatar }  ,
                Previsional = query.Previsional,
                EventDate = query.EventDate,
                Location = query.Location,
                Id = query.Id,
                Reason = query.Reason,
                ActivityCode = query.ActivityCode
            };
            return yaev;
        }
        public static HairCutQueryEvent CreateEvent(this HairCutQuery query,
        IStringLocalizer SR, BrusherProfile bpr)
        {
            string head = SR["HaircutRdvQuery"];
            string evdate = query.EventDate?.ToString("dddd dd/MM/yyyy à HH:mm")??"[pas de date spécifiée]";
            string address = query.Location?.Address??"[pas de lieu spécifié]";
            var p = query.Prestation;
            string total = query.GetBillItems().Addition().ToString("C",CultureInfo.CurrentUICulture);
            string strprestation = query.Description;
            string bill = string.Join("\n", query.GetBillItems().Select(
                l=> $"{l.Name} {l.Description} {l.UnitaryCost} {l.Count}"));

            var yaev = new HairCutQueryEvent
            {
                Sender = query.Client.UserName,
                Message =  $@"{head} pour {query.Client.UserName},
Date: {evdate},
Adresse: {address}
-----
{strprestation}:

{bill}

{total}
--
{query.AdditionalInfo}
" ,
Client =  new ClientProviderInfo { 
    UserName = query.Client.UserName ,
    UserId = query.ClientId,
    Avatar = query.Client.Avatar }  ,
Previsional = query.Previsional,
EventDate = query.EventDate,
Location = query.Location,
Id = query.Id,
Reason = $"{query.AdditionalInfo}",
ActivityCode = query.ActivityCode

            };
            return yaev;
        }

        public static HairCutQueryEvent CreateEvent(this HairMultiCutQuery query,
        IStringLocalizer SR, BrusherProfile bpr)
        {
            var yaev = new HairCutQueryEvent
            {
                Sender = query.ClientId,
                Message = string.Format(SR["RdvToPerf"],
                query.Client.UserName,
                query.EventDate.ToString("dddd dd/MM/yyyy à HH:mm"),
                query.Location.Address,
                query.ActivityCode),
                Client =  new ClientProviderInfo { 
                    UserName = query.Client.UserName ,
                    UserId = query.ClientId,
                    Avatar = query.Client.Avatar }  ,
                Previsional = query.Previsional,
                EventDate = query.EventDate,
                Location = query.Location,
                Id = query.Id,
                Reason = "Commande groupée!",
                ActivityCode = query.ActivityCode
            };
            return yaev;
        }
    }
}
