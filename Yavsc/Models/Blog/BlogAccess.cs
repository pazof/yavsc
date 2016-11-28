
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models
{

    public partial class BlogAccess
    {
        [ForeignKey("Blog.Id")]
        public long PostId { get; set; }

        [ForeignKey("Circle.Id")]
        public long CircleId { get; set; }
    }
}
