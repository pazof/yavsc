using Microsoft.Extensions.Localization;

namespace Yavsc.Helpers
{
    using Models.Workflow;
    using Models.Messaging;
    public static class EventHelpers
    {
        public static BookQueryEvent CreateEvent(this BookQuery query,
        IStringLocalizer SR)
        {
            var yaev = new BookQueryEvent
            {
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
        
    }
}
