using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Haircut
{
    
    public enum HairTechnos
    {
        Color,

        [Display(Name="Permantante")]
        Permanent,
        [Display(Name="Défrisage")]
        Defris,
        [Display(Name="Mêches")]
        Mech,
        Balayage
    }
}
