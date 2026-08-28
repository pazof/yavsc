using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Yavsc.Models.Relationship;

namespace Yavsc.Models.Blog
{
    public partial class BlogTag
    {
        [JsonIgnore]
        [ForeignKey("PostId")]
        public virtual BlogPost Post { get; set; }
        public long PostId { get; set; }

        [JsonIgnore]
        [ForeignKey("TagId")]
        public virtual Tag Tag{ get; set; }
        public long TagId { get; set; }
    }
}
