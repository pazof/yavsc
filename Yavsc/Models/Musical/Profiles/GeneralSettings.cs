using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Musical.Profiles
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
