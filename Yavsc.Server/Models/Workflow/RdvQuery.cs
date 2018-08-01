using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Workflow
{
    using System.Collections.Generic;
    using Yavsc.Models.Billing;
    using Yavsc.Models.Relationship;
    using Yavsc.Billing;

    /// <summary>
    /// Query, for a date, with a given perfomer, at this given place.
    /// </summary>

    public class RdvQuery : NominativeServiceCommand
    {
        /// <summary>
        /// The command identifier
        /// </summary>
        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        override public long Id { get; set; }

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

        public LocationKind LocationType
        {
            set;
            get;
        }

        [Display(Name="GiveAnExplicitReason")]
        public string Reason { get; set; }

        public override string Description => "Rendez-vous";

        public RdvQuery()
        {
        }

        public RdvQuery(string activityCode, Location eventLocation, DateTime eventDate)
        {
            Location = eventLocation;
            EventDate = eventDate;
            ActivityCode = activityCode;
        }

        public override List<IBillItem> GetBillItems()
        {
            throw new NotImplementedException();
        }
    }
}
