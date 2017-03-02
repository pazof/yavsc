using Yavsc.Interfaces.Workflow;

namespace Yavsc.Models.Haircut
{
    public class HairCutQueryEvent : RdvQueryProviderInfo, IEvent
    {
        public HairCutQueryEvent()
        {
            Topic = "HairCutQuery";
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

            set;
        }
        
        HairCutQuery Data { get; set; }
    }
}