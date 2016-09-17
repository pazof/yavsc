
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Market;

namespace Yavsc.Models.Billing
{

 public class CommandLine {

     [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
     public long Id { get; set; }
     
     [Required]
     public string Description { get; set; }
     public BaseProduct Article { get; set; }
     public int Count { get; set; }
     public decimal UnitaryCost { get; set; }
 }

}
