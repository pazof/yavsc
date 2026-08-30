using Yavsc.Interfaces;

namespace Yavsc.Abstract.Identity.Security
{
    public interface ICircleAuthorized :  ITaggable<long>
    {

        string AuthorId { get; }

        bool AuthorizeCircle(long circleId);

        ICollection<CircleAuthorization> ACL { get; } //ICircleAuthorization [] GetACL();

    }
}
