using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Workflow
{
    using Yavsc;

    public class CoWorking: ICoWorking
    {
        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id {get; set; }

        public string PerformerId { get; set; }
        public string WorkingForId { get; set; }

        [ForeignKey("PerformerId")]
        public virtual PerformerProfile Performer { get; set; }

        [ForeignKey("WorkingForId")]
        public virtual ApplicationUser WorkingFor { get; set; }
    }
}
