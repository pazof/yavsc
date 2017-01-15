using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Billing;
using Yavsc.Models.Relationship;

namespace Yavsc.Models.Booking
{
    /// <summary>
    /// Query, for a date, with a given perfomer, at this given place.
    /// </summary>

    public class BookQuery : NominativeServiceCommand<RendezVous> {
    /// <summary>
    /// The command identifier
    /// </summary>
    [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id {get; set; }

        [Display(Name="Event date")]
        public DateTime EventDate{ 
            get;
            set;
         }
        public Location Location {
            get;
            set;
            }
        
        public LocationType LocationType {
            set; 
            get;
        }
        public string Reason { get; set; }
        
        public BookQuery()
        {
        }

       public BookQuery(Location eventLocation, DateTime eventDate)
       {
               Location = eventLocation;
               EventDate = eventDate;
       }
    }
}