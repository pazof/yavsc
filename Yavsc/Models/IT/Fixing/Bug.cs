using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.IT.Fixing
{
    public class Bug
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long FeatureId { get; set; }

        public BugStatus Status { get; set; }
        
    }
}