using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using YavscLib;

namespace Yavsc.Models.Booking.Profiles
{
    public class DjSettings : ISpecializationSettings
    {

        public string SoundCloudId { get; set; }

        public virtual List<MusicalPreference> SoundColor { get; set; }

        [Key]
        public string UserId
        {
            get; set;
        }

    }
}