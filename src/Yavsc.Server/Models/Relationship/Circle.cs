
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Yavsc.Attributes.Validation;

namespace Yavsc.Models.Relationship
{
    public class Circle {
        
        [Key, DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public bool Public { get; set; }

        [YaRequired]
        public string Name { get; set; }
        
        [YaRequired]
        public string OwnerId { get; set; }

        [ForeignKey("OwnerId"),JsonIgnore,NotMapped]
        public virtual ApplicationUser Owner { get; set; }

        [InverseProperty("Circle"),JsonIgnore]
        public virtual List<CircleMember> Members { get; set; }
    }
}
