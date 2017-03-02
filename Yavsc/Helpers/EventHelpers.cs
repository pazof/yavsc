using Microsoft.Extensions.Localization;

namespace Yavsc.Helpers
{
    using Models.Workflow;
    using Models.Messaging;
    using Yavsc.Models.Haircut;

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
        IStringLocalizer SR)
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
                Reason = "Coupe particulier",
                ActivityCode = query.ActivityCode
            };
            return yaev;
        }

        public static HairCutQueryEvent CreateEvent(this HairMultiCutQuery query,
        IStringLocalizer SR)
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
