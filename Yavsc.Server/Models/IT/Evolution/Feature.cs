using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Attributes.Validation;

namespace Yavsc.Models.IT.Evolution
{
    public class Feature
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [YaStringLength(256,MinLen=3)]
        public string ShortName { get; set; }
        
        [YaStringLength(10*1024,MinLen=3)]
        public string Description { get; set; }

        public FeatureStatus Status { get; set; }
    }
}
