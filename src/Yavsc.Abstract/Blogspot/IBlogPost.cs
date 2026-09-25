#nullable enable annotations




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

        /// <summary>
        /// Whether this post is published. The server derives this
        /// from the existence of a row in <c>BlogSpotPublication</c>
        /// and serves it on the wire so clients (PostIt) can render
        /// the current publication state without a follow-up request.
        ///
        /// <para>This MUST be declared on the interface — not just on
        /// the concrete <c>BlogPost</c> entity / <c>BlogPostDto</c> —
        /// because the blog API's list endpoint returns
        /// <c>IEnumerable&lt;IBlogPost&gt;</c>, and the blog API host
        /// serialises with System.Text.Json, which emits only the
        /// members of the declared (interface) type. When
        /// <c>IsPublished</c> lived only on the concrete classes, STJ
        /// dropped it from the list payload and PostIt read
        /// <c>false</c> for every post, including published ones.</para>
        /// </summary>
        bool IsPublished { get; set; }
    }
}
