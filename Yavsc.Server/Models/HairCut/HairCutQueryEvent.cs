using Yavsc.Interfaces.Workflow;

namespace Yavsc.Models.Haircut
{
    public class HairCutQueryEvent : RdvQueryProviderInfo, IEvent
    {
        public HairCutQueryEvent(string subTopic)
        {
            Topic = "/topic/HairCutQuery";
            if (subTopic!=null) Topic+="/"+subTopic;
        }
        public string CreateBody()
        {
            return $"{Reason}\r\n-- \r\n{Previsional}\r\n{EventDate}\r\n";
        }

        public string CreateBoby()
        {
            return string.Format(ResourcesHelpers.GlobalLocalizer["RdvToPerf"], Client.UserName,
            EventDate?.ToString("dddd dd/MM/yyyy à HH:mm"),
            Location.Address,
            ActivityCode);
        }

        public string Sender
        {
            get;

            set;
        }

        public string Topic
        {
            get;

            private set;
        }

        HairCutQuery Data { get; set; }
    }
}
