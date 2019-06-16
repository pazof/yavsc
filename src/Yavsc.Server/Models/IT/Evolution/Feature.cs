using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Attributes.Validation;

namespace Yavsc.Models.IT.Evolution
{
    public class Feature
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [StringLength(256)]
        public string ShortName { get; set; }
        
        [StringLength(10*1024)]
        public string Description { get; set; }

        public FeatureStatus Status { get; set; }
    }
}
