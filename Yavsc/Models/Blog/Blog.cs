using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using Yavsc.Models.Access;
using YavscLib;

namespace Yavsc.Models
{
    public partial class Blog : IBlog, ICircleAuthorized
    {
        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string Content { get; set; }
        public string Photo { get; set; }
        public int Rate { get; set; }
        public string Title { get; set; }
        public string AuthorId { get; set; }
        
        [ForeignKey("AuthorId"),JsonIgnore]
        public ApplicationUser Author { set; get; }
        public bool Visible { get; set; }

        public DateTime DateCreated
        {
            get; set;
        }

        public string UserCreated
        {
            get; set;
        }

        public DateTime DateModified
        {
            get; set;
        }

        public string UserModified
        {
            get; set;
        }
        
        [InverseProperty("Target")]
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
