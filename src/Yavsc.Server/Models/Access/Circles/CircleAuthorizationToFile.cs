
namespace Yavsc.Models.Access
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Yavsc.Abstract.Identity.Security;
    using Models.Relationship;
    using Yavsc.Services;

    public class CircleAuthorizationToFile : FileAccessControlRulePayload
    {
        public FileAccessRight Access { get; set; }

    }

    public class FileAccessControlRulePayload : CircleAuthorization
    {
        public virtual string FileId { get; set; }

        [ForeignKey("CircleId")]
        public virtual Circle Allowed { get; set; }

    }
}
