
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

        [ForeignKey("PrestationId"),Required,Display(Name="Préstation")]
        public virtual HairPrestation  Prestation { get; set; }

        [ForeignKey("LocationId")]
        [Display(Name="Lieu du rendez-vous")]

        public virtual Location Location { get; set; }

        [Display(Name="Date et heure")]
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

        [Display(Name="Informations complémentaires"),
        StringLengthAttribute(512)]
        public string AdditionalInfo { get; set; }
    }
}
