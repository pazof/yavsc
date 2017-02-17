using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Workflow;

namespace Yavsc.Models.Musical.Profiles
{
    public class StarPerformerProfile : PerformerProfile
    {
        [InverseProperty("OwnerProfile")]
        public virtual List<MusicalPreference> SoundColor { get; set; }
    }
}