
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Billing;
using Yavsc.Models.Relationship;
using YavscLib.HairCut;

namespace Yavsc.Models.Haircut
{
    public class HairCutQuery : NominativeServiceCommand, IHairCutQuery
    {
        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long PrestationId { get; set; }

        [ForeignKey("PrestationId"),Required]
        public virtual HairPrestation  Prestation { get; set; }

        [ForeignKey("LocationId")]

        public virtual Location Location { get; set; }

        public DateTime? EventDate
        {
            get;
            set;
        }

        public long? LocationId
        {
            get;

            set;
        }
    }
}
