using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Workflow;

namespace Yavsc.Models.Musical.Profiles
{
    public class Instrumentation : IUserSettings
    {
        public string UserId
        {
            get; set;
        }

        [ForeignKeyAttribute("UserId")]
        public virtual PerformerProfile User { get; set; }
        public long InstrumentId { get; set; }

        [ForeignKeyAttribute("InstrumentId")]
        public virtual Instrument Tool { get; set; }
    }
}
