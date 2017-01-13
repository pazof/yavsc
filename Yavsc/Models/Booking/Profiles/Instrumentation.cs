using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Workflow;
using YavscLib;

namespace Yavsc.Models.Booking.Profiles
{
    public class Instrumentation : ISpecializationSettings
    {
        public string UserId
        {
            get; set;
        }
        
        [ForeignKeyAttribute("UserId")]
        public virtual PerformerProfile User { get; set; }
        public long InstrumentId { get; set; }

        [ForeignKeyAttribute("InstrumentId")]
        public virtual Instrument Tool { get; set; }
        
    }
}