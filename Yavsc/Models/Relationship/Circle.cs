
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Yavsc.Models.Relationship
{
    public partial class Circle {
        [Key, DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string Name { get; set; }
        public string OwnerId { get; set; }

        [ForeignKey("OwnerId"),JsonIgnore,NotMapped]
        public virtual ApplicationUser Owner { get; set; }

        [InverseProperty("Circle")]
        public virtual List<CircleMember> Members { get; set; }
    }
}
