using Microsoft.Extensions.Localization;

namespace Yavsc.Helpers
{
    using Models.Workflow;
    using Models.Messaging;
    using Yavsc.Models.Haircut;
    using Yavsc.Models;
    using Yavsc.Models.Billing;
    using Yavsc.Abstract.Identity;

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
                ActivityCode = query.ActivityCode,
                BillingCode = BillingCodes.Rdv
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
             string.Format(ResourcesHelpers.GlobalLocalizer["HairCutQueryValidation"],query.Client.UserName),
              $"{query.Client.Id}");
              

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
                ActivityCode = query.ActivityCode,
                BillingCode = BillingCodes.MBrush
            };
            return yaev;
        }
    }
}
