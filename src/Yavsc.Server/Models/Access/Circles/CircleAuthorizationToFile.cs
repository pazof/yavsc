
namespace Yavsc.Models.Access
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;
    using Models.Relationship;
    using Yavsc.Services;

    public class CircleAuthorizationToFile : FileAccessControlRulePayload
    {
        public FileAccessRight Access { get; set; }

        [JsonIgnore]
        [ForeignKey("CircleId")]
        public virtual Circle Allowed { get; set; }

        public string OwnerId { get; set; }

        [JsonIgnore]
        [ForeignKey("OwnerId")]
        public virtual ApplicationUser Owner { get; set; }

    }
}
