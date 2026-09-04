using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Haircut
{
    public enum HairDressings {

        [Display(Name="Coiffage")]
        Coiffage,
        
        [Display(Name="Brushing")]
        Brushing,
        
        [Display(Name="Mise en plis")]
        Folding
    }
}