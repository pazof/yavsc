
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Haircut
{
    public class HairPrestation 
    { 
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

        [Display(Name="Couleurs")]
        
        public virtual List<HairTaint> Taints { get; set; } 

        [Display(Name="Soins")]
        public  bool Cares { get; set; } 
        
    }
}