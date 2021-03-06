using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Yavsc.Attributes.Validation;

namespace Yavsc.Models.Workflow
{
    public class UserActivity
    {
        [YaRequired]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual PerformerProfile User { get; set; }
        
        [YaRequired]
        public string DoesCode { get; set; }

        [ForeignKey("DoesCode")]
        public virtual Activity Does { get; set; }

        [Range(0,100)]
        public int Weight { get; set; }

        [NotMapped,JsonIgnore]
        public object Settings { get; internal set; }
    }
}