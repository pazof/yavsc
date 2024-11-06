using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Yavsc.Attributes.Validation;

namespace Yavsc.Models.Relationship
{
    public class Contact: IContact
    {
        [Required()]
        public string UserId { get; set; }

        [Required()]
        public string OwnerId { get; set; }

        public string Name { get; set; }
        public string EMail { get; set; }

        public long AddressId { get ; set; }

        [ForeignKey("AddressId")]
        public virtual PostalAddress PostalAddress { get; set; }


        [ForeignKey("OwnerId"),NotMapped]
        public virtual ApplicationUser Owner { get; set; }

        [ForeignKey("UserId"),NotMapped]
        public virtual ApplicationUser User { get; set; }
    }
}
