
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Booking
{

    public class MusicalPreference   {
    
    [Key]
    public string OwnerProfileId
    {
        get; set;
    }


    public int Rate { get; set; }
    
    [Required]
    public long TendencyId {get; set; }
    


  } 

}
