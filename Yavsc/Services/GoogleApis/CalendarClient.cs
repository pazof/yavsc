using Google.Apis.Requests;
using Google.Apis.Services;
using Yavsc.Models.Google;

namespace Yavsc.Services.GoogleApis
{
    public class CalendarClient : ClientServiceRequest<Resource>
    {
        public CalendarClient(IClientService service) : base (service) {

        }
        public override string HttpMethod
        {
            get
            {
                return "POST";
            }
        }
        public string calendarId
        {
            get; set;
        }
        public override string MethodName
        {
            get
            {
                return "calendar.events.insert";
            }
        }

        public override string RestPath
        {
            get
            {
                return "calendars/{calendarId}/events";
            }
        }
    }
}