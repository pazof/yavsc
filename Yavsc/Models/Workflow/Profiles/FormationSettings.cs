using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Workflow.Profiles
{
    using Models.Workflow;
    using YavscLib;
    public class FormationSettings : ISpecializationSettings
    {
        public virtual List<CoWorking> CoWorking { get; set; }
        
        [Key]
        public string UserId
        {
            get; set;
        }

    }
}