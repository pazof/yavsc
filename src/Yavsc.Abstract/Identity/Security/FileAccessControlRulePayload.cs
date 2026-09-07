
namespace Yavsc.Models.Access
{
    using Yavsc.Abstract.Identity.Security;

    public class FileAccessControlRulePayload : CircleAuthorization
    {
        public virtual string Path { get; set; }


    }
}
