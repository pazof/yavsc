using System.ComponentModel.DataAnnotations;

namespace Yavsc.ViewModels.Blog;

public class BlogPostEditViewModel : BlogPostInputViewModel
{

    [Required]

    public  required long Id { get; set; }
}
