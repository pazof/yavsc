
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Haircut
{
    public enum HairLength : int
    {
        [Display(Name="Cheveux mi-longs")]
        HalfLong=0,

        [Display(Name="Cheveux courts")]
        Short=1,

        [Display(Name="Cheveux longs")]
        Long=2
    }
}
