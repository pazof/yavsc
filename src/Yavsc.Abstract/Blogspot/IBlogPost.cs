


using Yavsc.Abstract.Identity;
using Yavsc.Abstract.Identity.Security;
using Yavsc.Interfaces;

namespace Yavsc.Blogspot
{
    public interface IBlogPost : IBlogPostPayLoad, ICircleAuthorized,  ITrackedEntity,  ITitle
    {
        IApplicationUser Author { get; }
    }
}
