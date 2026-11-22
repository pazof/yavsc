using Yavsc.Models.Blog;
using Yavsc.Blogspot;

public static class PayloadHelpers
{
    public static object GetPayload(this BlogPost post)
    {
        return new
        {
            post.Id,
            post.Title,
            post.Article,
            post.DateCreated,
            post.UserCreated,
            post.DateModified,
            post.UserModified,
            post.AuthorId,
            Author = ((IBlogPost)post).Author,
            ACL = post.GetACL(),
            Tags = post.GetTags(),
            post.IsPublished
        };
    }
}
