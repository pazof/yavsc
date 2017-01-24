using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Workflow
{
    public class CommandForm 
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string ViewName { get; set; }

        public string Title { get; set; } 
        
        [Required]
        public string ActivityCode { get; set; }

        [ForeignKey("ActivityCode")]
        public virtual Activity Context { get; set; }
    }
}