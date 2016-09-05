using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Billing;

namespace Yavsc.Models.Booking
{
    /// <summary>
    /// Query, for a date, with a given perfomer, at this given place.
    /// </summary>

    public class BookQuery : NominativeServiceCommand {
    /// <summary>
    /// The command identifier
    /// </summary>
    [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id {get; set; }

        [Display(Name="Event date")]
        public DateTime EventDate{ 
            get
                { return ((RendezVous)Bill[0]).EventDate; }
            set { ((RendezVous)Bill[0]).EventDate = value; }
         }
        public Location Location {
            get
                { return ((RendezVous)Bill[0]).Location; }
            set { ((RendezVous)Bill[0]).Location = value; }}

        public BookQuery()
        {
            this.Bill.Add(new RendezVous());
        }

       public BookQuery(Location eventLocation, DateTime eventDate)
       {
           this.Bill.Add(new RendezVous{
               Location = eventLocation,
               EventDate = eventDate
           });
       }
       public string GetDescription() {
           return $"{Location?.Address} {EventDate.ToString()}";
       }
    }
}