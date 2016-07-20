
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models
{
    public partial class Circle: ICircle {
        [Key, DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string Name { get; set; }
        public string OwnerId { get; set; }

        [ForeignKey("OwnerId")]
        public virtual IApplicationUser Owner { get; set; }

        [InverseProperty("Circle")]
        public virtual IList<ICircleMember> Members { get; set; }
    }
}
