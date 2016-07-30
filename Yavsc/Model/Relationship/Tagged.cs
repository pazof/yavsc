
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models
{
    public partial class Tagged
    {
        [ForeignKey("Blog.Id")]
        public long postid { get; set; }
        [ForeignKey("tag.Id")]
        public long TagId { get; set; }
    }
}
