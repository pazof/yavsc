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
    /// Visibility of this post. Mirrors the EF entity
    /// <c>Yavsc.Models.Blog.BlogPost.Visibility</c>: serialised
    /// as an <c>int</c> by <c>System.Text.Json</c> (the enum's
    /// underlying type), so clients see <c>0</c> or <c>1</c>
    /// rather than <c>"Private"</c>/<c>"Public"</c>. Defaults
    /// to <see cref="Visibility.Private"/> on construction, so
    /// existing client code that doesn't set it explicitly
    /// stays safe (private-by-default).
    /// </summary>
    public Visibility Visibility { get; set; } = Visibility.Private;

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
