namespace Yavsc.Models.Access
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Models.Relationship;
    using Newtonsoft.Json;
    using YavscLib;

    public class CircleAuthorizationToBlogPost : ICircleAuthorization
    {
        public long CircleId { get; set; }
        public long BlogPostId { get; set; }

        [JsonIgnore]
        [ForeignKey("BlogPostId")]
        public virtual Blog Target { get; set; }

        [JsonIgnore]
        [ForeignKey("CircleId")]
        public virtual Circle Allowed { get; set; }

    }
}