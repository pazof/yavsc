using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Workflow;

namespace Yavsc.Models.Booking {

  public class MusicalPreference : MusicalTendency {

    public string OwnerProfileId { get; set; }  

    [ForeignKey("OwnerProfileId")]
    public virtual PerformerProfile OwnerProfile { get; set; }
    public int Rate { get; set; }

  } 

}
