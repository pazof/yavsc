using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Yavsc.Models.Workflow
{
  using Yavsc.Models.Billing;
  using Yavsc.Models.Relationship;
    /// <summary>
    /// Query, for a date, with a given perfomer, at this given place.
    /// </summary>

    public class BookQuery : NominativeServiceCommand
    {
        /// <summary>
        /// The command identifier
        /// </summary>
        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Display(Name = "Event date")]
        public DateTime EventDate
        {
            get;
            set;
        }
        public Location Location
        {
            get;
            set;
        }

        public LocationType LocationType
        {
            set;
            get;
        }
        public string Reason { get; set; }

        public BookQuery()
        {
        }

        public BookQuery(string activityCode, Location eventLocation, DateTime eventDate)
        {
            Location = eventLocation;
            EventDate = eventDate;
            ActivityCode = activityCode;
        }

        [Required]
        public string ActivityCode { get; set; }

        [ForeignKey("ActivityCode"),JsonIgnore]
        public virtual Activity Context  { get; set ; }

    }
}
