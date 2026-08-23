using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Relationship
{

    public class Location : Position, ILocation {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required(),
        Display(Name="Address"),
        MaxLength(512)]
        public string Address { get; set; }
    }
}
