using Yavsc.Models.Blog;

public class BlogPostEdition
{
    public string Content { get; internal set; }
    public string Title { get; internal set; }
    public string Photo { get; internal set; }

    internal static BlogPostEdition From(BlogPost blog)
    {
        throw new NotImplementedException();
    }
}
