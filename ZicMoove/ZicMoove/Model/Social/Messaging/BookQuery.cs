using ZicMoove.Model.Social;
using System;


namespace ZicMoove.Model.Workflow.Messaging
{
    /// <summary>
    /// Query, for a date, with a given perfomer, at this given place.
    /// </summary>
    public class BookQuery  {
    /// <summary>
    /// The command identifier
    /// </summary>
    public long Id {get; set; }
        public DateTime EventDate{get; set; }
        public Location Location { get; set; }

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