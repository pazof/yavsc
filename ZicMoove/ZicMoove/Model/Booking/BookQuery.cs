
using System;

namespace BookAStar.Model.Social
{
    public class BookQuery 
    {
        /// <summary>
        /// Client having made this book query.
        /// </summary>
        public ClientProviderInfo Client { get; set; }
        /// <summary>
        /// Location of the client event
        /// </summary>
        public Location Location { get; set; }
        /// <summary>
        /// Unique identifier
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// Date of the client event
        /// </summary>
        public DateTime EventDate { get; set; }
        /// <summary>
        /// Amount of money already available, from the site bank, for this event project.
        /// </summary>
        public decimal? Previsionnal { get; set; }
        /// <summary>
        /// Given reason from the client
        /// </summary>
        public string Reason { get; set; }
        /// <summary>
        /// True when the query has been read.
        /// </summary>
        public bool Read { get; set; }
        /// <summary>
        /// Unique identifier of the activity concerned, 
        /// the context of this query.
        /// </summary>
        public string ActivityCode { get; set; }
    }
}
