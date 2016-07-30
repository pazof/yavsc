using Microsoft.Extensions.Localization;
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
                Title = query.Client.UserName + " "+ SR["is asking you for a date"]+".",
                Comment = (query.Previsional != null) ?
                SR["Deposit"] + string.Format(": {0:00}",
                    query.Previsional) : SR["No deposit."],
                Description = SR["Address"] + ": " + query.Location.Address + "\n" +
                SR["Date"] + ": " + query.EventDate.ToString("D"),
                StartDate = query.EventDate,
                Location = query.Location,
                CommandId = query.Id
            };
            return yaev;
        }
    }
}
