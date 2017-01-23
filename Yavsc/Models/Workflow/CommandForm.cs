using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Workflow
{
    public class CommandForm : Forms.Form
    {
        public string Title { get; set; } 
        
        [Required]
        public string ActivityCode { get; set; }

        [ForeignKey("ActivityCode")]
        public virtual Activity Context { get; set; }
    }
}