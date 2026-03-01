namespace Yavsc.Models.Blog
{
  public class BlogPostEditViewModel : BlogPost
    {
        public bool Publish { get; set; }
        public BlogPostEditViewModel()
        {
        }
        
        public BlogPostEditViewModel(BlogPost post, bool publish)
        {
            Id = post.Id;
            AuthorId = post.AuthorId;
            Photo = post.Photo;
            Title = post.Title;
            Article = post.Article;
            Publish = publish;
            ACL = post.ACL;
        }


    }
}
