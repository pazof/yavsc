using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Yavsc.Abstract.Identity.Security;
using Yavsc.Models.Relationship;

namespace Yavsc.Server.Models.Access
{
    public class CircleAuthorizationToFile : ICircleAuthorization
    {
        [Required]
        public long CircleId
        {
            get; set;
        }
        
        [Required]
        public string FullPath
        {
            get; set;
        }

        [ForeignKey("CircleId"), JsonIgnore]
        public virtual Circle Circle { get; set; }
    }
}