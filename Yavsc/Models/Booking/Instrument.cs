using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Booking
{
    public class Instrument
    {
        
    [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id {get; set; }
    
    [MaxLength(255),Required]
    public string Name { get ; set; }
    }
}