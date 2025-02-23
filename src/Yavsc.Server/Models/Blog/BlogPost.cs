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
using Yavsc.ViewModels.Blog;

namespace Yavsc.Models.Blog
{
    public class BlogPost : BlogPostInputViewModel, IBlogPost, ICircleAuthorized, ITaggable<long>, IIdentified<long>
    {
        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name="Identifiant du post")]
        public  long Id { get; set; }

        [Display(Name="Identifiant de l'auteur")]
        [ForeignKey("Author")]
        public  string AuthorId { get; set; }

        [Display(Name="Auteur")]
        public  virtual ApplicationUser Author { set; get; }


        [Display(Name="Date de création")]
        public DateTime DateCreated
        {
            get; set;
        }

        [Display(Name="Créateur")]
        public  string UserCreated
        {
            get; set;
        }

        [Display(Name="Dernière modification")]
        public DateTime DateModified
        {
            get; set;
        }

        [Display(Name="Utilisateur ayant modifé le dernier")]
        public  string UserModified
        {
            get; set;
        }

        public bool AuthorizeCircle(long circleId)
        {
            return ACL?.Any( i=>i.CircleId == circleId) ?? true;
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

        public void DeTag(Tag tag)
        {
            var existent = Tags.SingleOrDefault(t => (( t.TagId == tag.Id) && t.PostId == Id));
            if (existent!=null) Tags.Remove(existent);
        }

        public string[] GetTags()
        {
            return Tags.Select(t=>t.Tag.Name).ToArray();
        }

        [InverseProperty("Post")]
        public  virtual List<BlogTag> Tags { get; set; }

        [InverseProperty("Post")]
        public  virtual List<Comment> Comments { get; set; }

        [NotMapped]
        public string OwnerId => AuthorId;
    }
}
