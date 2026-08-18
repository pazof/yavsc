using System;
using Yavsc.Abstract.Identity;
using Yavsc.Abstract.Identity.Security;

namespace Yavsc.Blogspot;

public class BlogPostDto : IBlogPost
{
    public string AuthorId { get; set; }

    public IApplicationUser Author { get; set; }

    public string Article { get; set ; }
    public string Photo { get; set ; }
    public long Id { get; set; }
    public DateTime DateCreated { get; set; }
    public string UserCreated { get; set; }
    public DateTime DateModified { get; set; }
    public string UserModified { get; set; }
    public string Title { get; set; }

    /// <summary>
    /// Whether this post is published. Derived server-side from
    /// the existence of a row in <c>BlogSpotPublication</c>
    /// (a row means published, no row means draft). Not stored
    /// on <c>BlogPost</c> — it's a computed projection of the
    /// publication table, surfaced through the wire DTO so
    /// clients can render the current state without a
    /// follow-up request. Toggled via
    /// <c>PUT /api/BlogApi/{id}/publish</c>.
    /// </summary>
    public bool IsPublished { get; set; }

    public bool AuthorizeCircle(long circleId)
    {
        throw new NotImplementedException();
    }

    public ICircleAuthorization[] GetACL()
    {
        throw new NotImplementedException();
    }

    public string[] GetTags()
    {
        throw new NotImplementedException();
    }
}
