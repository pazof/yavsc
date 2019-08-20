using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Workflow;

namespace Yavsc.Models.Musical.Profiles
{
    public class FormationPerformerProfile
    {
        [InverseProperty("WorkingFor")]
        public virtual List<CoWorking> CoWorking { get; set; }
    }
}
