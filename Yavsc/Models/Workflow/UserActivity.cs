using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Workflow
{
    public class UserActivity
    {
        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual PerformerProfile User { get; set; }
        
        [Required]
        public string DoesCode { get; set; }

        [ForeignKey("DoesCode")]
        public virtual Activity Does { get; set; }

        [Range(0,100)]
        public int Weight { get; set; }
    }
}