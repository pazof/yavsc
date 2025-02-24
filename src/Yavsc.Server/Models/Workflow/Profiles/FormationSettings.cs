using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Yavsc.Attributes.Validation;

namespace Yavsc.Models.Workflow.Profiles
{
    using Models.Workflow;
    using Yavsc;
    public class FormationSettings : IUserSettings
    {
        public virtual List<CoWorking> CoWorking { get; set; }

        [Key]
        public string UserId
        {
            get; set;
        }

        [YaStringLength(1024)]
        public string DisplayName { get; set; }

    }
}
