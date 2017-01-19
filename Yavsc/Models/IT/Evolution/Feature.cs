using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.IT.Maintaining
{
    public class Feature
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public FeatureStatus Status { get; set; }
    }
}