using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models
{
    public partial class PostTag
    {
        [ForeignKey("PostId")]
        public virtual Blog Post { get; set; }
        public long PostId { get; set; }
        public long TagId { get; set; }
    }
}
