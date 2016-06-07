using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models
{


    public partial class AccountBalance {
        [Key]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser Owner { get; set; }

        [Required,Display(Name="Credits in €")]
        public decimal Credits { get; set; }

        public long ContactCredits { get; set; }
    }

}