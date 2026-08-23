using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [Required]
        public string Name { get; set; }

        public long ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project TargetProject { get; set; }

    }
}
