using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Attributes.Validation;
using Yavsc.Models.Workflow;

namespace Yavsc.Models.Musical
{
    public class InstrumentRating
    {
        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id {get; set; }

        [YaRequired]
        public long InstrumentId { get; set; }
       
        [ForeignKey("InstrumentId")]
        public virtual Instrument Instrument { get; set; }

        [Range(-100,100)]
        public int Rate { get; set; }

        public string OwnerId { get; set; }

        [ForeignKey("OwnerId")]
        public virtual PerformerProfile Profile { get; set; } 

    }
}