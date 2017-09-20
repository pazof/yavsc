using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Relationship
{
    public partial class BlogTag
    {
        [ForeignKey("PostId")]
        public virtual Blog Post { get; set; }
        public long PostId { get; set; }

        [ForeignKey("TagId")]
        public virtual Tag Tag{ get; set; }
        public long TagId { get; set; }
    }
}
