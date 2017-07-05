
using Google.Apis.Calendar.v3.Data;

namespace Yavsc.ViewModels.Calendar
{
    public class SetGoogleCalendarViewModel
    {
          public string GoogleCalendarId { get; set; }

          public string ReturnUrl { get; set; }

          public CalendarList Calendars { get; set; }
    }

}
