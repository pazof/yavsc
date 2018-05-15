using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Attributes.Validation;
using Yavsc.Models.IT.Maintaining;

namespace Yavsc.Models.IT.Fixing
{
    public class Bug
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [ForeignKey("FeatureId")]
        public virtual Feature False { get; set; }
        public long? FeatureId { get; set; }

        [YaStringLength(2048)]
        public string Description { get; set; }
        
        public BugStatus Status { get; set; }
        
    }
}