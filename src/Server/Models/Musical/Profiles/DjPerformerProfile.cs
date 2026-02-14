using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Workflow;

namespace Yavsc.Models.Musical
{
    public class DjPerformerProfile : SpecializationSettings
    {
        public string SoundCloudId { get; set; }

        [InverseProperty("OwnerProfile")]
        public virtual List<MusicalPreference> SoundColor { get; set; }
        
    }
}