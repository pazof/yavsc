using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using YavscLib;

namespace Yavsc.Models.Musical.Profiles
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

        public bool ExistsInDb(object dbContext)
        {
             return ((ApplicationDbContext)dbContext).DjSettings.Any(p=>p.UserId==UserId);
        }
    }
}