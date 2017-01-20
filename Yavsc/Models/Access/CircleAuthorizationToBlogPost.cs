namespace Yavsc.Models.Access
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Models.Relationship;
    using Newtonsoft.Json;

    public class CircleAuthorizationToBlogPost
    {
        public long CircleId { get; set; }
        public long BlogPostId { get; set; }

        [JsonIgnore]
        [ForeignKey("BlogPostId")]
        public virtual Blog Post { get; set; }

        [JsonIgnore]
        [ForeignKey("CircleId")]
        public virtual Circle Allowed { get; set; }
    }
}