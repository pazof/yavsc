
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models
{
    public partial class BalanceImpact {
        [Required,Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required,Display(Name="Impact")]
         public decimal Impact { get; set; }

        [Required,Display(Name="Execution date")]
         public DateTime ExecDate { get; set; }

        [Required,Display(Name="Reason")]
         public string Reason { get; set; }
         [Required]
         public string BalanceId { get; set; }

         [ForeignKey("BalanceId")]
         public virtual AccountBalance Balance { get; set; }
    }
}
