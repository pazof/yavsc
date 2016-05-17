using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Booking
{
    /// <summary>
    /// Query, for a date, with a given perfomer, at this given place.
    /// </summary>

    public class BookQuery : Command {

        /// <summary>
        /// Event date
        /// </summary>
        /// <returns></returns>
        [Required(ErrorMessageResourceName="ChooseAnEventDate",
        ErrorMessageResourceType=typeof(Resources.YavscLoc)),Display(Name="EventDate")]
        public DateTime EventDate { get; set; }

        /// <summary>
        /// Location identifier
        /// </summary>
        /// <returns></returns>
        [Required]
        public long LocationId { get; set; }

        /// <summary>
        /// A Location for this event
        /// </summary>
        /// <returns></returns>
        [Required(ErrorMessage="SpecifyPlace"),Display(Name="Location"),ForeignKey("LocationId")]
        public Location Location { get; set; }

    }
}