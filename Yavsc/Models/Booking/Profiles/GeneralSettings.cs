using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Workflow;
using YavscLib;

namespace Yavsc.Models.Booking.Profiles
{
    public class GeneralSettings : ISpecializationSettings
    {
        public virtual List<MusicalPreference> SoundColor { get; set; }
        [Key]
        public string UserId
        {
            get; set;
        }
    }
}