


using Yavsc.Abstract.Identity.Security;

namespace Yavsc.Blogspot
{
    public interface IBlogPost : IBlogPostPayLoad, ICircleAuthorized,  ITrackedEntity,  ITitle
    {
        // Typed as a concrete wire DTO (not the IApplicationUser
        // interface) so System.Text.Json can materialise it on the
        // client without a polymorphic converter. The server-side
        // BlogPost entity implements this getter by mapping its
        // ApplicationUser navigation to a BlogPostAuthorDto.
        BlogPostAuthorDto? Author { get; }
    }
}
