using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Attributes.Validation;

namespace Yavsc.Models.Musical
{
    public class Instrument
    {
        
    [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id {get; set; }
    
    [MaxLength(255), YaRequired]
    public string Name { get ; set; }
    }
}