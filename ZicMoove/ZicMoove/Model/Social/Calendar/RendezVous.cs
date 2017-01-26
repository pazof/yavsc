using BookAStar.Model.Social;
using BookAStar.Model.Workflow.Marketing;
using System;

namespace BookAStar.Model.Workflow.Calendar
{
    /// <summary>
    /// A date, between two persons
    /// </summary>

    public class RendezVous: Service {
        // Haut les mains.

 /// <summary>
        /// Event date
        /// </summary>
        /// <returns></returns>
        public DateTime EventDate { get; set; }

        /// <summary>
        /// Location identifier
        /// </summary>
        /// <returns></returns>
        public long LocationId { get; set; }

        /// <summary>
        /// A Location for this event
        /// </summary>
        /// <returns></returns>
        public Location Location { get; set; }

    }

}