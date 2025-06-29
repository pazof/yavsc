using System.ComponentModel.DataAnnotations;

namespace Yavsc.ViewModels.Blog;

public class BlogPostEditViewModel : BlogPostBase
{

    [Required]

    public  required long Id { get; set; }
}
