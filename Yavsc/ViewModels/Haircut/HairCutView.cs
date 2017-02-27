using System.Collections.Generic;
using Yavsc.Models.Haircut;
using Yavsc.Models.Workflow;

namespace Yavsc.ViewModels.Haircut
{
    public class HairCutView
    {
        public List<PerformerProfile> HairBrushers { get; set; }
        
        public HairPrestation Topic { get; set; }
    }
}