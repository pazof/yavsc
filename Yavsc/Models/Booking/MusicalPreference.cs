
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Workflow;

namespace Yavsc.Models.Booking {

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
