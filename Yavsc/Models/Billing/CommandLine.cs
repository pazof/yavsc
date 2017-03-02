
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Yavsc.Models.Billing
{
    using YavscLib.Billing;

    public class CommandLine : ICommandLine {

     [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
     public long Id { get; set; }
     
     [Required,MaxLength(512)]
     public string Description { get; set; }

     public int Count { get; set; }

     [DisplayFormat(DataFormatString="{0:C}")]
     public decimal UnitaryCost { get; set; }

     public long EstimateId { get; set; }
     
     [JsonIgnore,NotMapped,ForeignKey("EstimateId")]
     virtual public Estimate Estimate { get; set; }
 }

}
