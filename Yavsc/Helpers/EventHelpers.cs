using Microsoft.Extensions.Localization;

namespace Yavsc.Helpers
{
    using Models.Workflow;
    using Models.Messaging;
    using Yavsc.Models.Haircut;
    using Yavsc.Models;

    public static class EventHelpers
    {
        public static RdvQueryEvent CreateEvent(this RdvQuery query,
        IStringLocalizer SR, string subtopic)
        {
            var yaev = new RdvQueryEvent(subtopic)
            {
                Sender = query.ClientId,
                Reason = query.Reason,
                Client =  new ClientProviderInfo { 
                    UserName = query.Client.UserName ,
                    UserId = query.ClientId,
                    Avatar = query.Client.Avatar }  ,
                Previsional = query.Previsional,
                EventDate = query.EventDate,
                Location = query.Location,
                Id = query.Id,
                ActivityCode = query.ActivityCode
            };

            return yaev;
        }


        public static HairCutQueryEvent CreateNewHairCutQueryEvent(this HairCutQuery query,
        IStringLocalizer SR)
        {
            string evdate = query.EventDate?.ToString("dddd dd/MM/yyyy à hh:mm")??"[pas de date spécifiée]";
            string address = query.Location?.Address??"[pas de lieu spécifié]";
            var p = query.Prestation;
            string strprestation = query.GetDescription();

            var yaev = query.CreateEvent("NewHairCutQuery",
             string.Format(Startup.GlobalLocalizer["HairCutQueryValidation"],query.Client.UserName),
              $"{query.Client.UserName}") ;

            return yaev;
        }
        public static string GetSender(this ApplicationUser user)
        {
            return user.UserName+" ["+user.Id+"@"+Startup.Authority+"]";
        }
        public static HairCutQueryEvent CreateEvent(this HairMultiCutQuery query,
        IStringLocalizer SR, BrusherProfile bpr)
        {
            var yaev = new HairCutQueryEvent("newCommand")
            {
                Sender = query.ClientId,
                
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
