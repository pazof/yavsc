using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Yavsc.Abstract.Identity.Security;
using Yavsc.Attributes.Validation;
using Yavsc.Interfaces;
using Yavsc.Models.Access;
using Yavsc.Models.Relationship;

namespace Yavsc.Models.Blog
{
    public class BlogPost : IBlogPost, ICircleAuthorized, ITaggable<long>, IIdentified<long>
    {
        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name="Identifiant du post")]
        public long Id { get; set; }

        [Display(Name="Contenu")][YaStringLength(56224)]
        public string Content { get; set; }

        [Display(Name="Photo")][YaStringLength(1024)]
        public string Photo { get; set; }

        [YaStringLength(8)]
        public string Lang { get; set; }

        [Display(Name="Indice de qualité")]
        public int Rate { get; set; }

        [Display(Name="Titre")][YaStringLength(1024)]
        public string Title { get; set; }

        [Display(Name="Identifiant de l'auteur")]
        [ForeignKey("Author")]
        public string AuthorId { get; set; }

        [Display(Name="Auteur")]
        public virtual ApplicationUser Author { set; get; }

        [Display(Name="Visible")]
        public bool Visible { get; set; }

        [Display(Name="Date de création")]
        public DateTime DateCreated
        {
            get; set;
        }

        [Display(Name="Créateur")]
        public string UserCreated
        {
            get; set;
        }

        [Display(Name="Dernière modification")]
        public DateTime DateModified
        {
            get; set;
        }

        [Display(Name="Utilisateur ayant modifé le dernier")]
        public string UserModified
        {
            get; set;
        }

        [InverseProperty("Target")]
        [Display(Name="Liste de contrôle d'accès")]
        public virtual List<CircleAuthorizationToBlogPost> ACL { get; set; }

        public bool AuthorizeCircle(long circleId)
        {
            return ACL?.Any( i=>i.CircleId == circleId) ?? true;
        }
        
        public string GetOwnerId()
        {
            return AuthorId;
        }

        public ICircleAuthorization[] GetACL()
        {
            return ACL.ToArray();
        }

        public void Tag(Tag tag)
        {
           var existent = Tags.SingleOrDefault(t => t.PostId == Id && t.TagId == tag.Id);
            if (existent==null) Tags.Add(new BlogTag { PostId = Id, Tag = tag } );
        }

        public  void Detag(Tag tag)
        {
            var existent = Tags.SingleOrDefault(t => (( t.TagId == tag.Id) && t.PostId == Id));
            if (existent!=null) Tags.Remove(existent);
        }

        public string[] GetTags()
        {
            return Tags.Select(t=>t.Tag.Name).ToArray();
        }

        [InverseProperty("Post")]
        public virtual List<BlogTag> Tags { get; set; }

        [InverseProperty("Post")]
        public virtual List<Comment> Comments { get; set; }
    }
}
