using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Attributes.Validation;
using Yavsc.Models.IT.Evolution;

namespace Yavsc.Models.IT.Fixing
{
    public partial class Bug
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [ForeignKey("FeatureId")]
        public virtual Feature False { get; set; }

        public long? FeatureId { get; set; }

        [StringLength(240, ErrorMessageResourceType=typeof(Yavsc.Models.IT.Fixing.Bug), ErrorMessageResourceName="TitleTooLong")]
        public string Title { get; set; }

        [StringLength(4096)]
        public string Description { get; set; }
        
        public BugStatus Status { get; set; }
        
    }
}
