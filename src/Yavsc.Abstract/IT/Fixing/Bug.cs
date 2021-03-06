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

        [YaStringLength(240, MinimumLength=4 , 
        ErrorMessageResourceType=typeof(Yavsc.Models.IT.Fixing.Bug), 
        ErrorMessageResourceName="TitleSizeError")]
        public string Title { get; set; }

        [YaStringLength(10240, 
        ErrorMessageResourceType=typeof(Yavsc.Models.IT.Fixing.Bug), 
        ErrorMessageResourceName="DescSizeError")]
        public string Description { get; set; }
        
        public BugStatus Status { get; set; }
        
    }
}
