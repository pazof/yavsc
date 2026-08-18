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
