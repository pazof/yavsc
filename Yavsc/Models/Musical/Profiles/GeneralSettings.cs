using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using YavscLib;

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

        public bool ExistsInDb(object dbContext)
        {
            return ((ApplicationDbContext)dbContext).GeneralSettings.Any(p=>p.UserId==UserId);
        }
    }
}