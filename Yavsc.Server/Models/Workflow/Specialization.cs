using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Workflow
{
    public abstract class SpecializationSettings
    {
        [Key]
        public long UserActivityId { get; set; }

        [ForeignKey("UserActivityId")]
        public virtual UserActivity Context { get; set; }

    }
}