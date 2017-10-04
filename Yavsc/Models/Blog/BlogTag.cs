using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Relationship;

namespace Yavsc.Models.Blog
{
    public partial class BlogTag
    {
        [ForeignKey("PostId")]
        public virtual BlogPost Post { get; set; }
        public long PostId { get; set; }

        [ForeignKey("TagId")]
        public virtual Tag Tag{ get; set; }
        public long TagId { get; set; }
    }
}
