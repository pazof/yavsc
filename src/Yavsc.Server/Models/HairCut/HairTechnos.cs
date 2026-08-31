using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Haircut
{

    public enum HairTechnos
    {
        [Display(Name="Aucune technique spécifique")]
        NoTech,

        [Display(Name="Couleur")]
        Color,

        [Display(Name="Permanente")]
        Permanent,
        [Display(Name="Défrisage")]
        Defris,
        [Display(Name="Mêches")]
        Mech,

        [Display(Name="Balayage")]
        Balayage
    }
}
