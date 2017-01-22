
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Relationship
{

    public partial class CircleMember
    {
      
        [Required]
        public long CircleId { get; set; }      

        [ForeignKey("CircleId")]
        public virtual Circle Circle { get; set; }
        [Required]
        public string MemberId { get; set; }

        [ForeignKey("MemberId")]
        public virtual ApplicationUser Member { get; set; }
    }
}
