
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Billing;
using Yavsc.Models.Relationship;

namespace Yavsc.Models.Haircut
{
    public class HairCutQuery : NominativeServiceCommand
    {
        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long PrestationId { get; set; }

        [ForeignKey("PrestationId")]
        public virtual HairPrestation  Prestation { get; set; }

        [Required]

        public Location Location { get; set; }

        public DateTime EventDate
        {
            get;
            set;
        }
    }
}