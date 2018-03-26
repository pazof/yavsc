
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Yavsc.Models.Billing
{
    using Yavsc.Billing;

    public class CommandLine : ICommandLine {

     [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
     public long Id { get; set; }

     [Required,MaxLength(256)]
     public string Name { get; set; }

     [Required,MaxLength(512)]
     public string Description { get; set; }

     [Display(Name="Nombre")]
     public int Count { get; set; } = 1;

     [DisplayFormat(DataFormatString="{0:C}")]
     public decimal UnitaryCost { get; set; }

     public long EstimateId { get; set; }

     [JsonIgnore,NotMapped,ForeignKey("EstimateId")]
     virtual public Estimate Estimate { get; set; }

        public string Currency
        {
            get;

            set;
        } = "EUR";

        [NotMapped]
        public string Reference {
            get {
                return "CL/"+this.Id;
            }
        }
    }


}
