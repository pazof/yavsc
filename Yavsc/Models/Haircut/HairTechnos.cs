using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Haircut
{

    public enum HairTechnos
    {
        [Display(Name="Aucune technique spécifique")]
        NoTech,

        [Display(Name="Couleur")]
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
