


using Yavsc.Abstract.Identity;
using Yavsc.Abstract.Identity.Security;
using Yavsc.Interfaces;

namespace Yavsc.Blogspot
{
    public interface IBlogPost : IBlogPostPayLoad, ICircleAuthorized, ITaggable<long>,  ITrackedEntity, IIdentified<long>, ITitle
    {
        string AuthorId { get; set; }
        IApplicationUser Author { get; }
    }
}
