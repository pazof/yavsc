using System.ComponentModel.DataAnnotations;
using Yavsc.Models.Blog;

namespace Yavsc.ViewModels.Blog;

public class BlogPostEditViewModel : BlogPostBase
{

    [Required]

    public  required long Id { get; set; }

    public bool Publish { get; set; }

    public BlogPostEditViewModel()
    {
    
    }


    public static BlogPostEditViewModel From(BlogPost blogInput)
    {
        return new BlogPostEditViewModel
        {
            Id = blogInput.Id,
            Title = blogInput.Title,
            Publish = false,
            Photo = blogInput.Photo,
            Content = blogInput.Content,
            ACL = blogInput.ACL
        };
    }
    public static BlogPostEditViewModel FromViewModel(BlogPostEditViewModel blogInput)
    {
        return new BlogPostEditViewModel
        {
            Id = blogInput.Id,
            Title = blogInput.Title,
            Publish = false,
            Photo = blogInput.Photo,
            Content = blogInput.Content,
            ACL = blogInput.ACL
        };
    }
}
