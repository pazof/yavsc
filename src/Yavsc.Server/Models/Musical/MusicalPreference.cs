
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Attributes.Validation;

namespace Yavsc.Models.Musical
{

    public class MusicalPreference   {
    
    [Key]
    public string OwnerProfileId
    {
        get; set;
    }


    public int Rate { get; set; }
    
    [Required]
    public long TendencyId { get; set; }
    
    [ForeignKey("TendencyId")]
    public virtual MusicalTendency MusicalTendency { get; set; } 
  } 

}
