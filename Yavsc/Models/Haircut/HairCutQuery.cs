using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Billing;

namespace Yavsc.Models.Haircut
{
    public class HairCutQuery : NominativeServiceCommand
    {
        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        HairPrestation [] Prestations { get; set; }
    }
}