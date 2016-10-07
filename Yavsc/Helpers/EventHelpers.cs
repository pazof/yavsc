using Microsoft.Extensions.Localization;
using Yavsc.Model;
using Yavsc.Models.Booking;
using Yavsc.Models.Messaging;

namespace Yavsc.Helpers
{
    public static class EventHelpers
    {
        public static BookQueryEvent CreateEvent(this BookQuery query,
        IStringLocalizer SR)
        {
            var yaev = new BookQueryEvent
            {
                Client =  new ClientProviderInfo { UserName = query.Client.UserName , UserId = query.ClientId }  ,
                Previsional = query.Previsional,
                EventDate = query.EventDate,
                Location = query.Location,
                Id = query.Id
            };
            return yaev;
        }

    }
}
