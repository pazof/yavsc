using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Musical.Profiles
{
    public class DjSettings : IUserSettings
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
