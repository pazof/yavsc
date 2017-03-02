using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Billing;
using Yavsc.Models.Relationship;

namespace Yavsc.Models.Haircut
{
    public class HairMultiCutQuery : NominativeServiceCommand
    {
        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        HairPrestation [] Prestations { get; set; }

        public Location Location { get; set; }

        public DateTime EventDate
        {
            get;
            set;
        }
    }
}