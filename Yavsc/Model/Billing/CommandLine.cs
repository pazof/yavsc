
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Yavsc.Models.Market;

namespace Yavsc.Models.Billing
{

 public class CommandLine {

     [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
     public long Id { get; set; }
     
     [Required,MaxLength(512)]
     public string Description { get; set; }
     public BaseProduct Article { get; set; }
     public int Count { get; set; }
     public decimal UnitaryCost { get; set; }

     public long EstimateId { get; set; }
     
     [JsonIgnore,NotMapped,ForeignKey("EstimateId")]
     virtual public Estimate Estimate { get; set; }
 }

}
