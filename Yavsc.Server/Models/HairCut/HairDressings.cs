using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Haircut
{
    public enum HairDressings {

        Coiffage,
        
        Brushing,
        
        [Display(Name="Mise en plis")]
        Folding
    }
}