using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Market;

namespace Yavsc.Models.Workflow
{
    using Models.Relationship;
    /// <summary>
    /// A date, between two persons
    /// </summary>
    public class RendezVous: Service {
        // Haut les mains.

 /// <summary>
        /// Event date
        /// </summary>
        /// <returns></returns>
        [Required(),Display(Name="EventDate")]
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