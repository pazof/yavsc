using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Workflow;

namespace Yavsc.Models.Booking
{
    public class InstrumentRating
    {
        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id {get; set; }
        public Instrument Instrument { get; set; }
        public int Rate { get; set; }

        public string OwnerId { get; set; }

        [ForeignKey("OwnerId")]
        public virtual PerformerProfile Profile { get; set; } 

    }
}