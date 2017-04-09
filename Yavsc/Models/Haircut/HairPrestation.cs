using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Yavsc.Models.Haircut
{
    public class HairPrestation
    {
        // Homme ou enfant => Coupe seule
        // Couleur => Shampoing
        // Forfaits : Coupe + Technique
        // pas de coupe => technique

        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Display(Name="Longueur de cheveux")]
        public HairLength Length { get; set; }

        [Display(Name="Pour qui")]
        public HairCutGenders Gender { get; set; }

        [Display(Name="Coupe")]
        public bool Cut { get; set; }

        [Display(Name="Coiffage")]

        public HairDressings Dressing { get; set; }

        [Display(Name="Technique")]
        public HairTechnos Tech { get; set; }

        [Display(Name="Shampoing")]
        public bool Shampoo { get; set; }

        [Display(Name="Couleurs"),JsonIgnore,InverseProperty("Prestation")]

        public virtual List<HairTaintInstance> Taints { get; set; }

        [Display(Name="Soins")]
        public  bool Cares { get; set; }


    }
    public class HairTaintInstance {

        public long TaintId { get; set; }

        [ForeignKey("TaintId")]
        public virtual HairTaint Taint { get; set; }
        public long PrestationId { get; set; }

        [ForeignKey("PrestationId")]
        public virtual HairPrestation Prestation { get; set; }
    }
}
