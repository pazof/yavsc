using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Yavsc.Abstract.Identity.Security;
using Yavsc.Models.Access;
using Yavsc.Models.Relationship;
using Yavsc.Blogspot;

namespace Yavsc.Models.Blog
{

    public class BlogPost : IBlogPost
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
        public virtual ApplicationUser Author { set; get; }


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

        public CircleAuthorization[] GetACL()
        {
            return ACL?.ToArray() ?? Array.Empty<CircleAuthorization>();
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
            return Tags?.Select(t => t.Tag.Name).ToArray() ?? Array.Empty<string>();
        }

        [InverseProperty("Post")]
        public virtual List<BlogTag> Tags { get; set; }

        [InverseProperty("Post")]
        public virtual List<Comment> Comments { get; set; }

        /// <summary>
        /// Whether this post is published. Not a column: the
        /// existence of a row in <c>BlogSpotPublication</c>
        /// is the source of truth. EF skips this property via
        /// <c>[NotMapped]</c> so no migration is needed. The
        /// service hydrates it after each fetch (single bulk
        /// lookup, not N+1) and it surfaces through the wire
        /// as part of the JSON-serialised <c>BlogPost</c>.
        /// </summary>
        [NotMapped]
        public bool IsPublished { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Explicit interface implementation of
        /// <see cref="IBlogPost.Author"/>. The underlying
        /// navigation property is <see cref="Author"/>
        /// (an <c>ApplicationUser</c> entity), but the wire
        /// DTO is a thin <see cref="BlogPostAuthorDto"/> with
        /// only the fields the client UI consumes. We project
        /// on demand so EF can lazy-load the navigation
        /// without forcing an eager join on every read.
        /// Returns <c>null</c> when the navigation hasn't been
        /// loaded (caller should pre-Include <c>Author</c> if
        /// they need it).
        /// </summary>
        BlogPostAuthorDto? IBlogPost.Author
        {
            get
            {
                var a = Author;
                if (a == null) return null;
                return new BlogPostAuthorDto
                {
                    Id = a.Id,
                    UserName = a.UserName,
                    Avatar = a.Avatar
                };
            }
        }

        ICollection<CircleAuthorization> ICircleAuthorized.ACL
        {
            get
            {
                return ACL?.Select(a => new CircleAuthorization
                {
                    CircleId = a.CircleId
                }).ToList() ?? new List<CircleAuthorization>();
            }
        }
    }
}
