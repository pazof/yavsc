
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Attributes.Validation;

namespace Yavsc.Models.Relationship
{
    public class Tag
    {
        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required()]
        public string Name { get; set; }
    }
}
