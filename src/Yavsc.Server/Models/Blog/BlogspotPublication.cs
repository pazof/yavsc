using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Blog;

namespace Yavsc.Models
{
    public class BlogspotPublication
    {
        [Key]
        public long BlogpostId { get; set; }
        
        [ForeignKey("BlogpostId")]
        public virtual BlogPost BlogPost{ get; set; }
    }
}
