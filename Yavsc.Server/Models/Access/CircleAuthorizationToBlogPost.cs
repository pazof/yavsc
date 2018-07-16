namespace Yavsc.Models.Access
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Models.Relationship;
    using Newtonsoft.Json;
    using Blog;
    using Yavsc.Abstract.Identity.Security;

    public class CircleAuthorizationToBlogPost : ICircleAuthorization
    {
        public long CircleId { get; set; }
        public long BlogPostId { get; set; }

        [JsonIgnore]
        [ForeignKey("BlogPostId")]
        public virtual BlogPost Target { get; set; }

        [JsonIgnore]
        [ForeignKey("CircleId")]
        public virtual Circle Allowed { get; set; }

    }
}
