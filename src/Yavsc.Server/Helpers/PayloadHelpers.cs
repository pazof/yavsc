using Yavsc.Models.Blog;

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
            ACL = post.GetACL(),
            Tags = post.GetTags()
        };
    }
}