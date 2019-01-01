using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Yavsc.Models.Workflow
{
        using Yavsc;
    public class CommandForm : ICommandForm
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string ActionName { get; set; }

        public string Title { get; set; }

        [Required]
        public string ActivityCode { get; set; }

        [ForeignKey("ActivityCode"),JsonIgnore]
        public virtual Activity Context { get; set; }
    }
}
