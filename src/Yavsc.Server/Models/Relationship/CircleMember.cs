
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Attributes.Validation;

namespace Yavsc.Models.Relationship
{

    public partial class CircleMember
    {

        [YaRequired]
        public long CircleId { get; set; }

        [ForeignKey("CircleId")]
        public virtual Circle Circle { get; set; }
        
        [YaRequired]
        public string MemberId { get; set; }

        [ForeignKey("MemberId")]
        public virtual ApplicationUser Member { get; set; }
    }
}
