using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Market;

namespace Yavsc.Models.Workflow
{
    using Models.Relationship;
    using Yavsc.Attributes.Validation;

    /// <summary>
    /// A date, between two persons
    /// </summary>
    public class RendezVous: Service {
        // Haut les mains.

 /// <summary>
        /// Event date
        /// </summary>
        /// <returns></returns>
        [YaRequired(),Display(Name="EventDate")]
        public DateTime EventDate { get; set; }

        /// <summary>
        /// Location identifier
        /// </summary>
        /// <returns></returns>
        [YaRequired]
        public long LocationId { get; set; }

        /// <summary>
        /// A Location for this event
        /// </summary>
        /// <returns></returns>
        [YaRequired(ErrorMessage="SpecifyPlace"),Display(Name="Location"),ForeignKey("LocationId")]
        public Location Location { get; set; }

    }

}
