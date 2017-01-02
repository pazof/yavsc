using System.Collections.Generic;

namespace Yavsc.Models.Booking {

  public class MusicalPreference : MusicalTendency {

    public long OwnerId { get; set; }  
    public int Rate { get; set; }

  } 

}
