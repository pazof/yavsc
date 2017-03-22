using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Haircut
{
    public enum HairLength : int
    {
        [Display(Name="Mi-longs")]
        HalfLong,

        [Display(Name="Courts")]
        Short = 1,

        [Display(Name="Longs")]
        Long
    }
}
