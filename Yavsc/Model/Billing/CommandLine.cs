

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models
{

 public class CommandLine {

     [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
     public long Id { get; set; }
     public string Comment { get; set; }
     public BaseProduct Article { get; set; }
     public int Count { get; set; }
     public decimal UnitaryCost { get; set; }
 }

}
