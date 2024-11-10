
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Access;

namespace Yavsc.ViewModels.Blog
{
    public class BlogPostInputViewModel
{
        [StringLength(1024)]
        public  string? Photo { get; set; }

        [StringLength(1024)]
        public required string Title { get; set; }

        [StringLength(56224)]
        public required string Content { get; set; }
        
        public bool Visible { get; set; }

        [InverseProperty("Target")]
        [Display(Name="Liste de contrôle d'accès")]
        public virtual List<CircleAuthorizationToBlogPost>? ACL { get; set; }

    }
}
