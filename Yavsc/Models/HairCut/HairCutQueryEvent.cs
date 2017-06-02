using Yavsc.Interfaces.Workflow;

namespace Yavsc.Models.Haircut
{
    public class HairCutQueryEvent : RdvQueryProviderInfo, IEvent
    {
        public HairCutQueryEvent(string subTopic)
        {

            Topic = GetType().Name+"/"+subTopic;
        }
        public string Message
        {
            get;

            set;
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
