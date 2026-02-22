using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Yavsc.Abstract.Identity;
using Yavsc.Abstract.Identity.Security;
using Yavsc.Interfaces;
using Yavsc.Models.Access;
using Yavsc.Models.Relationship;

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
    
    public class BlogPost :
     IBlogPost, ICircleAuthorized, ITaggable<long>
    {
        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Identifiant du post")]
        public long Id { get; set; }

        [StringLength(1024)]
        public string? Photo { get; set; }

        [StringLength(1024)]
        [Required]
        public string Title { get; set; }

        [StringLength(56224)]
        public string? Article { get; set; }

        [InverseProperty("Target")]
        [Display(Name = "Liste de contrôle d'accès")]
        public virtual List<CircleAuthorizationToBlogPost>? ACL { get; set; }

        [Display(Name = "Identifiant de l'auteur")]
        [ForeignKey("Author")]
        public string? AuthorId { get; set; }

        [Display(Name = "Auteur")]
        public virtual ApplicationUser? Author { set; get; }


        [Display(Name = "Date de création")]
        public DateTime DateCreated
        {
            get; set;
        }

        [Display(Name = "Créateur")]
        public string? UserCreated
        {
            get; set;
        }

        [Display(Name = "Dernière modification")]
        public DateTime DateModified
        {
            get; set;
        }

        [Display(Name = "Utilisateur ayant modifé le dernier")]
        public string? UserModified
        {
            get; set;
        }

        public bool AuthorizeCircle(long circleId)
        {
            return ACL?.Any(i => i.CircleId == circleId) ?? true;
        }

        public ICircleAuthorization[] GetACL()
        {
            return ACL.ToArray();
        }

        public void Tag(Tag tag)
        {
            var existent = Tags.SingleOrDefault(t => t.PostId == Id && t.TagId == tag.Id);
            if (existent == null) Tags.Add(new BlogTag { PostId = Id, Tag = tag });
        }

        public void DeTag(Tag tag)
        {
            var existent = Tags.SingleOrDefault(t => ((t.TagId == tag.Id) && t.PostId == Id));
            if (existent != null) Tags.Remove(existent);
        }

        public string[] GetTags()
        {
            return Tags.Select(t => t.Tag.Name).ToArray();
        }

        [InverseProperty("Post")]
        public virtual List<BlogTag> Tags { get; set; }

        [InverseProperty("Post")]
        public virtual List<Comment> Comments { get; set; }

        IApplicationUser IBlogPost.Author { get => this.Author; }
    }
}
