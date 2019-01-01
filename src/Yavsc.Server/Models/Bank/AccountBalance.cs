using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models
{
  using Yavsc;
    
    public partial class AccountBalance: IAccountBalance {

        [Key]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser Owner { get; set; }

        [Required,Display(Name="Credits en €")]
        public decimal Credits { get; set; }

        public long ContactCredits { get; set; }
    }

}
