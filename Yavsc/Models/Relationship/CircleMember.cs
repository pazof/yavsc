
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Relationship
{

    public partial class CircleMember
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public virtual Circle Circle { get; set; }

        [Required]
        public virtual ApplicationUser Member { get; set; }
    }
}
