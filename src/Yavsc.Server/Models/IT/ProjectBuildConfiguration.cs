using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Attributes.Validation;

namespace Yavsc.Server.Models.IT
{
    public class ProjectBuildConfiguration
    {
        /// <summary>
        /// A Numerical Id
        /// </summary>
        /// <value></value>
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [YaRequired]
        public string Name { get; set; }

        public long ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project TargetProject { get; set; }

    }
}
