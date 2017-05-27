using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using Yavsc.Models.Access;
using Yavsc;

namespace Yavsc.Models
{
    public partial class Blog : IBlog, ICircleAuthorized
    {
        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name="Identifiant du post")]
        public long Id { get; set; }

        [Display(Name="Contenu")]
        public string Content { get; set; }
        [Display(Name="Photo")]
        public string Photo { get; set; }
        [Display(Name="Indice de qualité")]
        public int Rate { get; set; }
        [Display(Name="Titre")]
        public string Title { get; set; }
        [Display(Name="Identifiant de l'auteur")]
        public string AuthorId { get; set; }

        [Display(Name="Auteur")]

        [ForeignKey("AuthorId"),JsonIgnore]
        public ApplicationUser Author { set; get; }
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
            return ACL.Any( i=>i.CircleId == circleId);
        }

        public string GetOwnerId()
        {
            return AuthorId;
        }

        public ICircleAuthorization[] GetACL()
        {
            return ACL.ToArray();
        }
    }
}
