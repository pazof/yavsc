
using System.ComponentModel.DataAnnotations;
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
    
    [YaRequired]
    public long TendencyId {get; set; }
  } 

}
