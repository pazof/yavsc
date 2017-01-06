using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Workflow;

namespace Yavsc.Models.Booking
{
    public class MusicianPerformerProfile : PerformerProfile
    {
        [InverseProperty("Profile")]
        public virtual List<InstrumentRating> Instrumentation {
            get; set;
        }
    }
}