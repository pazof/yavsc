using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Yavsc.Models
{
    public class Contact
    {
        [Required()]
        public string UserId { get; set; }

        [Required()]
        public string OwnerId { get; set; }

        [ForeignKeyAttribute("OwnerId"),NotMapped]
        public virtual ApplicationUser Owner { get; set; }

        [ForeignKeyAttribute("UserId"),NotMapped]
        public virtual ApplicationUser User { get; set; }
    }
}
