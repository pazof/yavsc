using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Yavsc.Models.Access
{
    public class BlackListed: IBlackListed
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string OwnerId { get; set; }

        [ForeignKey("OwnerId"),JsonIgnore]
        public virtual ApplicationUser Owner { get; set; }
    }
   
}
