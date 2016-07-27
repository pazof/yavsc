using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Yavsc.Models
{
    public class Contact
    {
        [Required()]
        public string UserId { get; set; }

        [Required()]
        public string OwnerId { get; set; }

        [ForeignKeyAttribute("OwnerId")]
        public virtual ApplicationUser Owner { get; set; }
    }
}
