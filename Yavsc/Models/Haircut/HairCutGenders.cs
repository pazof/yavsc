using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Haircut
{
    public enum HairCutGenders : int
    {
        [Display(Name="Femme")]
        Women,

        [Display(Name="Homme")]
        Man,

        [Display(Name="Enfant")]
        Kid 
        
    }
}