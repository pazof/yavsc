using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Blog;

namespace Yavsc.Models
{
    public class BlogSpotPublication
    {
        [Key]
        public long PostId { get; set; }

        [ForeignKey("PostId")]
        public virtual BlogPost Post{ get; set; }
    }
}
