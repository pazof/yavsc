using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Relationship
{
    public partial class PostTag
    {
        [ForeignKey("PostId")]
        public virtual Blog Post { get; set; }
        public long PostId { get; set; }
        public long TagId { get; set; }
    }
}
